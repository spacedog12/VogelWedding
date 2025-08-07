using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;
namespace VogelWedding.Model;

[Table("WishlistPurchases")]
public class WishlistPurchase : BaseModel
{
	[Column("wishlist_item_id")]
	public Guid WishlistItemId { get; set; }
	
	
	[Column("first_name")]
	public string FirstName { get; set; }
	
	
	[Column("family_name")]
	public string FamilyName { get; set; }
	
	
	[Column("email")]
	[EmailAddress(ErrorMessage = "Bitte geben Sie eine gültige E-Mail-Adresse ein")]
	public string Email { get; set; }
	
	
	[Column("paid_amount")]
	public double PaidAmount { get; set; }
	
	
	// [Column("quantity")]
	// public int Quantity { get; set; }
	
}
