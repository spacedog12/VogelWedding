public enum AccessLevel
{
	None,
	GuestAll,
	GuestInvited,
	TestUserAll,
	TestUserInvited,
	Admin
}

public class AccessService(SupabaseService supabaseService)
{
	private readonly ILogger<AccessService> _logger;

	public event Action? OnChange;

	// private const string Key = "access_level";
	private AccessLevel currentLevel = AccessLevel.None;

	public AccessLevel CurrentLevel
	{
		get => currentLevel;
		private set
		{
			if (currentLevel != value)
			{
				// Console.WriteLine($"Access level changing from {currentLevel} to {value}"); // Debug print
				currentLevel = value;
				NotifyStateChange();
			}
		}
	}

	public event Action<AccessLevel>? OnAccessLevelChanged;

	public async Task<bool> TryLoginWithCode(string code)
	{
		// Console.WriteLine($"TryLoginWithCode called with code: {code}"); // Debug print

		CurrentLevel = await supabaseService.LoginUserAsync(code.ToUpper());

		return currentLevel != AccessLevel.None;

	}

	public bool SetAdminAccess()
	{
		CurrentLevel = AccessLevel.Admin;
		return true;
	}

	public bool HasRequiredAccess(AccessLevel requiredLevel)
	{
		return CurrentLevel >= requiredLevel;
	}

	public async void Logout()
	{
		CurrentLevel = await supabaseService.Logout();
	}

	private void NotifyStateChange()
	{
		// Console.WriteLine("NotifyStateChanged called"); // Debug print
		OnChange?.Invoke();
	}
}
