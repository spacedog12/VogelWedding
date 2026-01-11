using BlazorCurrentDevice;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using VogelWedding.Interfaces;
using VogelWedding.Services;

namespace VogelWedding.ViewModels;

public class PhotosViewModel
{
	private readonly ISupabasePhotosService _photosService;
	private readonly AccessService _accessService;
	private readonly IToastService _toastService;
	private readonly IBlazorCurrentDeviceService _currentDevice;

	private const string CeremonyFolder = "/Ceremony";
	private const string PartyFolder = "/Party";

	// State
	public bool IsMobile { get; private set; }
	public bool IsUploading { get; private set; }
	public bool IsLoadingImages { get; private set; } = true;
	public List<string> ImageUrls { get; } = new();
	public string SelectedUploadFolder { get; private set; } = CeremonyFolder;

	// Modal & Selection State
	public bool ShowPreviewModal { get; set; }
	public bool IsPreviewLoading { get; private set; }
	public List<IBrowserFile> FilesToUpload { get; } = new();
	public List<string> PreviewUrls { get; } = new();
	public int UploadedCount { get; private set; }
	public int TotalToUpload { get; private set; }

	public bool ShowFullscreenModal { get; set; }
	public string SelectedFullsizeUrl { get; set; } = "";

	public event Action? OnStateChanged;

	private void NotifyStateChanged() => OnStateChanged?.Invoke();

	public PhotosViewModel(
		ISupabasePhotosService photosService,
		AccessService accessService,
		IToastService toastService,
		IBlazorCurrentDeviceService currentDevice
	)
	{
		_photosService = photosService;
		_accessService = accessService;
		_toastService = toastService;
		_currentDevice = currentDevice;
	}

	public async Task InitializeAsync()
	{
		IsMobile = await _currentDevice.Mobile();

		SelectedUploadFolder = _accessService.CurrentLevel >= AccessLevel.GuestInvited
			? PartyFolder
			: CeremonyFolder;

		await LoadImages();
	}

	public async Task OnFilesSelected(InputFileChangeEventArgs e)
	{
		IReadOnlyList<IBrowserFile> selectedFiles;
		try
		{
			selectedFiles = e.GetMultipleFiles(100);
		}
		catch (Exception ex)
		{
			_toastService.ShowError($"Zu viele Dateien ausgewählt: Maximal 100 auf einmal erlaubt.");
			return;
		}

		if (selectedFiles == null || !selectedFiles.Any()) return;

		ShowPreviewModal = true;
		PreviewUrls.Clear();
		FilesToUpload.Clear();
		IsPreviewLoading = true;
		NotifyStateChanged();

		long maxFileSize = 1024 * 1024 * 20;
		var invalidFilesFound = false;
		var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".heif", ".heic" };

		foreach (var file in selectedFiles)
		{
			try
			{
				var extension = Path.GetExtension(file.Name).ToLowerInvariant();
				
				if (allowedExtensions.Contains(extension))
				{
					var resizedFile = await file.RequestImageFileAsync("image/jpeg", 300, 300);
					var buffer = new byte[resizedFile.Size];
					await resizedFile.OpenReadStream(maxFileSize).ReadAsync(buffer);
					PreviewUrls.Add($"data:image/jpeg;base64,{Convert.ToBase64String(buffer)}");
					FilesToUpload.Add(file);
					NotifyStateChanged();
				}
				else
				{
					invalidFilesFound = true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Preview failed: {ex.Message}");
				_toastService.ShowError($"Fehler beim Anzeigen von {file.Name}: {ex.Message}");
			}
		}
		
		if (invalidFilesFound)
		{
			_toastService.ShowInfo("Einige Dateien wurden übersprungen, da sie ein falsches Format haben.");
		}

		IsPreviewLoading = false;
		NotifyStateChanged();
	}

	public void RemoveFileFromSelection(int index)
	{
		if (index >= 0 && index < FilesToUpload.Count)
		{
			FilesToUpload.RemoveAt(index);
			PreviewUrls.RemoveAt(index);
			NotifyStateChanged();
		}
	}

	public void ClosePreviewModal()
	{
		ShowPreviewModal = false;
		FilesToUpload.Clear();
		PreviewUrls.Clear();
	}

	public async Task ConfirmUpload()
	{
		if (!FilesToUpload.Any()) return;

		ShowPreviewModal = false;
		IsUploading = true;
		TotalToUpload = FilesToUpload.Count;
		UploadedCount = 0;
		NotifyStateChanged();

		try
		{
			var results = await _photosService.UploadFilesAsync(FilesToUpload, SelectedUploadFolder, count =>
			{
				UploadedCount = count;
				NotifyStateChanged();
			});

			if (results.Any())
				_toastService.ShowSuccess($"{results.Count} Bilder erfolgreich hochgeladen!");
			else
				_toastService.ShowInfo("Keine Bilder hochgeladen.");

			await LoadImages();
		}
		catch (Exception ex)
		{
			_toastService.ShowError($"Upload fehlgeschlagen: {ex.Message}");
		}
		finally
		{
			IsUploading = false;
			FilesToUpload.Clear();
			NotifyStateChanged();
		}
	}

	public double CalculateProgress() => TotalToUpload == 0 ? 0 : (double)UploadedCount / TotalToUpload * 100;

	public void OpenFullscreen(string url)
	{
		SelectedFullsizeUrl = url;
		ShowFullscreenModal = true;
	}

	public void CloseFullscreen()
	{
		ShowFullscreenModal = false;
		SelectedFullsizeUrl = "";
	}

	// ================= Private Methods ===================

	async Task LoadImages()
	{
		IsLoadingImages = true;
		ImageUrls.Clear();

		try
		{
			var level = _accessService.CurrentLevel;

			if (level >= AccessLevel.GuestAll)
			{
				var ceremonyPhotos = await _photosService.GetImageUrlsAsync(CeremonyFolder);
				ImageUrls.AddRange(ceremonyPhotos);
			}

			if (level >= AccessLevel.GuestInvited)
			{
				var partyPhotos = await _photosService.GetImageUrlsAsync(PartyFolder);
				ImageUrls.AddRange(partyPhotos);
			}
		}
		catch (Exception ex)
		{
			_toastService.ShowError($"Fehler beim Laden: {ex.Message}");
		}
		finally
		{
			IsLoadingImages = false;
		}
	}
}
