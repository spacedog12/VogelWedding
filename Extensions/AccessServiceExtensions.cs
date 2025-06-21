namespace VogelWedding.Services;

public static class AccessServiceExtensions
{
    public static bool CanAccessPage(this AccessService accessService, bool isPageEnabled)
    {
        return accessService.CurrentLevel == AccessLevel.Admin || isPageEnabled;
    }
    
    public static bool CanAccessRsvp(this AccessService accessService, bool isRsvpEnabled)
    {
        return accessService.CurrentLevel == AccessLevel.Admin || 
               (accessService.CurrentLevel >= AccessLevel.GuestInvited && isRsvpEnabled);
    }
    
    public static bool CanAccessPhotos(this AccessService accessService, bool isPhotosEnabled)
    {
        return accessService.CurrentLevel == AccessLevel.Admin || 
               (accessService.CurrentLevel >= AccessLevel.GuestInvited && isPhotosEnabled);
    }
}