namespace VogelWedding.Services;

public class SettingsService
{
	private readonly SupabaseService _supabaseService;
	private AppSettings _currentSettings;
	public event Action OnSettingsChanged;

	public SettingsService(SupabaseService supabaseService)
	{
		_supabaseService = supabaseService;
	}

	public AppSettings CurrentSettings => _currentSettings ?? new AppSettings();

	public async Task LoadSettings()
	{
		try
		{
			var settings = await _supabaseService.GetSettingsAsync();
			if (settings != null)
			{
				_currentSettings = settings;
				OnSettingsChanged?.Invoke();
			}
			else
			{
				// Handle case where no settings exist
				_currentSettings = new AppSettings
				{
					Id = Guid.NewGuid(),
					SiteTitle = "Our Wedding",
					RsvpEnabled = false,
					NotificationEmail = string.Empty
				};

				// Insert initial settings
				await SaveSettings(_currentSettings);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error loading settings: {ex.Message}");
			_currentSettings = new AppSettings
			{
				Id = Guid.NewGuid(),
				SiteTitle = "Our Wedding",
				RsvpEnabled = false,
				NotificationEmail = string.Empty
			};
		}
	}

	public async Task SaveSettings(AppSettings settings)
	{
		try
		{
			if (settings.Id == Guid.Empty)
			{
				settings.Id = Guid.NewGuid();
			}

			var response = await _supabaseService.Client
				.From<AppSettings>()
				.Upsert(settings);

			if (response?.Models?.Any() == true)
			{
				_currentSettings = response.Models.First();
			}
			else
			{
				_currentSettings = settings;
			}

			OnSettingsChanged?.Invoke();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error saving settings: {ex.Message}");
			throw;
		}
	}
}