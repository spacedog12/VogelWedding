using Microsoft.AspNetCore.Components.Forms;
using VogelWedding.Interfaces;
using FileOptions = Supabase.Storage.FileOptions;

namespace VogelWedding.Services;

public class SupabasePhotosService : ISupabasePhotosService
{
    private readonly Supabase.Client _supabase;
    private const string BucketName = "wedding-photos"; 
    // Increase max file size limit to 20MB to be safe
    private const long MaxFileSize = 1024 * 1024 * 20; 

    public SupabasePhotosService(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    public async Task<List<string>> UploadFilesAsync(IReadOnlyList<IBrowserFile> files, string folder)
    {
        var uploadedUrls = new List<string>();

        // CHANGE: Upload sequentially instead of Task.WhenAll.
        // Blazor WASM struggles with parallel stream reading from IBrowserFile.
        foreach (var file in files)
        {
            var result = await UploadSingleFileAsync(file, folder);
            if (result != null)
            {
                uploadedUrls.Add(result);
            }
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
            
            var uniqueName = $"{Guid.NewGuid()}_{safeFileName}{ext}";
            var fullPath = string.IsNullOrEmpty(cleanFolder) ? uniqueName : $"{cleanFolder}/{uniqueName}";

            var storage = _supabase.Storage.From(BucketName);

            await storage.Upload(bytes, fullPath, new FileOptions { Upsert = false });

            // CHANGE: Use CreateSignedUrl instead of GetPublicUrl
            // This generates a link valid for 1 hour (3600 seconds)
            return await storage.CreateSignedUrl(fullPath, 3600);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Supabase] Upload failed for {file.Name}: {e.Message}");
            return null;
        }
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
                    .OrderByDescending(item => item.CreatedAt) // Show newest first
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
}