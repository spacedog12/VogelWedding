using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
namespace VogelWedding.Model;

[Table("Wishlist")]
public class WishlistItem: BaseModel
{
	[Column("id")]
	[PrimaryKey("id")]
	public Guid ID { get; set; }
	
	
	[Column("sort_number")]
	public int SortNumber { get; set; }
	
	[Column("title")]
	public string Title { get; set; }
	
	
	[Column("description")]
	public string Description { get; set; }
	
	
	[Column("price")]
	public double? Price { get; set; }
	
	
	[Column("paid_amount")]
	public double? PaidAmount { get; set; }
	
	
	[Column("quantity")]
	public int? Quantity { get; set; }
	
	
	[Column("number_paid_users")]
	public int? NumberPaidUsers { get; set; }
	
	
	[Column("image_url")]
	public string ImageUrl { get; set; }
}
