public enum AccessLevel
{
    None,
    GuestAll,
    GuestInvited,
    Admin
}

public class AccessService
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
                Console.WriteLine($"Access level changing from {currentLevel} to {value}"); // Debug print
                currentLevel = value;
                NotifyStateChange();
            }
        }
    }

    public event Action<AccessLevel>? OnAccessLevelChanged;

    public bool TryLoginWithCode(string? code)
    {
        Console.WriteLine($"TryLoginWithCode called with code: {code}"); // Debug print
        
        switch (code?.ToUpper())
        {
            case "FREUDE2025":
                CurrentLevel = AccessLevel.GuestAll;
                return true;
            case "FEST2025":
                CurrentLevel = AccessLevel.GuestInvited;
                return true;
            default:
                return false;
        }
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

    public void Logout()
    {
        CurrentLevel = AccessLevel.None;
    }

    private void NotifyStateChange()
    {
        Console.WriteLine("NotifyStateChanged called"); // Debug print
        OnChange?.Invoke();
    }
}