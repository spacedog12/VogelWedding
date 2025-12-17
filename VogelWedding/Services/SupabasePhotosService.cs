using Microsoft.AspNetCore.Components.Forms;
using VogelWedding.Interfaces;
using FileOptions = Supabase.Storage.FileOptions;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Globalization;

namespace VogelWedding.Services;

public class SupabasePhotosService : ISupabasePhotosService
{
    private readonly Supabase.Client _supabase;
    private readonly ILogger<SupabasePhotosService> _logger;
    
    private const string BucketName = "wedding-photos"; 
    // Increase max file size limit to 20MB to be safe
    private const long MaxFileSize = 1024 * 1024 * 20; 

    public SupabasePhotosService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<List<string>> UploadFilesAsync(IReadOnlyList<IBrowserFile> files, string folder, Action<int>? onProgressUpdate = null)
    {
        var uploadedUrls = new List<string>();
        var uploadedCount = 0;

        // CHANGE: Upload sequentially instead of Task.WhenAll.
        // Blazor WASM struggles with parallel stream reading from IBrowserFile.
        foreach (var file in files)
        {
            var result = await UploadSingleFileAsync(file, folder);
            if (result != null)
            {
                uploadedUrls.Add(result);
            }

            uploadedCount++;
            onProgressUpdate?.Invoke(uploadedCount);
        }

        return uploadedUrls;
    }

    private async Task<string?> UploadSingleFileAsync(IBrowserFile file, string folder)
    {
        try
        {
            using var stream = file.OpenReadStream(MaxFileSize);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();

            var cleanFolder = folder.Trim('/');
            // Sanitize filename to remove special characters that might break URLs
            var safeFileName = Path.GetFileNameWithoutExtension(file.Name)
                .Replace(" ", "-")
                .Replace("ä", "ae")
                .Replace("ö", "oe")
                .Replace("ü", "ue");
            var ext = Path.GetExtension(file.Name);

            var captureTimeUtc = TryGetCaptureTimeUtc(bytes) ?? file.LastModified.UtcDateTime;
            
            // Prefix that sorts lexicographically by time (UTC)
            var captureTimePrefix = captureTimeUtc.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);

            var uniqueName = $"{captureTimePrefix}_{Guid.NewGuid()}_{safeFileName}{ext}";
            var fullPath = string.IsNullOrEmpty(cleanFolder) ? uniqueName : $"{cleanFolder}/{uniqueName}";

            var storage = _supabase.Storage.From(BucketName);

            await storage.Upload(bytes, fullPath, new FileOptions
            {
                Upsert = false
            });

            // CHANGE: Use CreateSignedUrl instead of GetPublicUrl
            // This generates a link valid for 1 hour (3600 seconds)
            return await storage.CreateSignedUrl(fullPath, 3600);
        }
        catch (ArgumentNullException ane)
        {
            _logger.LogError(ane, "Upload failed for {file.Name}: {ane.Message}", file.Name, ane.Message);
        }
        catch (ObjectDisposedException ode)
        {
            _logger.LogError(ode, "Upload failed for {file.Name}: {ode.Message}", file.Name, ode.Message);
        }
        catch (ArgumentException ae)
        {
            _logger.LogError(ae, "Upload failed for {file.Name}: {ae.Message}", file.Name, ae.Message);
        }
        catch (NullReferenceException nre)
        {
            _logger.LogError(nre, "Upload failed for {file.Name}: {nre.Message}", file.Name, nre.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "[Supabase] Upload failed for {file.Name}: {e.Message}", file.Name, e.Message);
        }
        
        return null;
    }

    public async Task<List<string>> GetImageUrlsAsync(string folder)
    {
        var imageUrls = new List<string>();
        try
        {
            var cleanFolder = folder.Trim('/');
            var storage = _supabase.Storage.From(BucketName);

            var result = await storage.List(cleanFolder);

            if (result != null)
            {
                // Sorting by CreatedAt (if available) or Name ensures consistent order
                // Using a standard loop is safer here than parallel tasks for consistency, 
                // though parallel is okay for generating signed URLs.
                var sortedResult = result
                    .Where(item => !string.IsNullOrEmpty(item.Name) && IsImageFile(item.Name))
                    // .OrderByDescending(item => item.CreatedAt) // Show newest first
                    .OrderByDescending(item => item.Name)
                    .ToList();

                var urlTasks = new List<Task<string>>();

                foreach (var item in sortedResult)
                {
                     var fullPath = string.IsNullOrEmpty(cleanFolder) 
                            ? item.Name 
                            : $"{cleanFolder}/{item.Name}";
                        
                     urlTasks.Add(storage.CreateSignedUrl(fullPath, 3600));
                }

                var urls = await Task.WhenAll(urlTasks);
                imageUrls.AddRange(urls);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Supabase] GetImageUrls failed: {ex.Message}");
        }

        return imageUrls;
    }

    private bool IsImageFile(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext is ".jpg" or ".jpeg" or ".png" or ".heic" or ".webp";
    }

    private DateTime? TryGetCaptureTimeUtc(byte[] bytes)
    {
        try
        {
            using var ms = new MemoryStream(bytes);
            var directories = ImageMetadataReader.ReadMetadata(ms);
            
            var exifSubIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var dateTime = exifSubIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal)
                 ?? exifSubIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeDigitized);
            
            // converts EXIF DateTime to UTC
            if (dateTime is null) return null;
            
            var local = DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Local);
            return local.ToUniversalTime();
        }
        catch (ImageProcessingException ipe)
        {
            _logger.LogError(ipe, "Could not read the Image metadata {bytes.Length}: {ipe.Message}", bytes.Length, ipe.Message);
        }
        catch (IOException ioe)
        {
            _logger.LogError(ioe, "Error in ");
        }

        return null;
    }
}