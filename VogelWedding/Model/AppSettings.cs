using Supabase.Postgrest.Attributes;

[Table("AppSettings")]
public class AppSettings : BaseModel
{
    [PrimaryKey("id", false)]
    public Guid Id { get; set; }

    [Column("site_title")]
    public string SiteTitle { get; set; } = "Our Wedding";

    [Column("rsvp_enabled")]
    public bool RsvpEnabled { get; set; }

    [Column("notification_email")]
    public string NotificationEmail { get; set; } = string.Empty;

    [Column("page_home_visible")]
    public bool HomePageVisible { get; set; }

    [Column("page_about_visible")]

    public bool AboutPageVisible { get; set; }

    [Column("page_wishlist_visible")]
    public bool WishlistPageVisible { get; set; }

    [Column("page_photos_visible")]
    public bool PhotosPageVisible { get; set; }

    [Column("page_contact_visible")]
    public bool ContactPageVisible { get; set; }


}