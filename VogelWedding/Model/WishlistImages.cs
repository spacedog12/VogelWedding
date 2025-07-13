namespace VogelWedding.Model;
using Supabase.Postgrest.Attributes;

[Table("WishlistImages")]
public class WishlistImages : BaseModel
{
	[Column("wishlist_item_id")]
	public Guid WishlistItemId { get; set; }
	
	[Column("image_url")]
	public string ImageUrl { get; set; }
}
