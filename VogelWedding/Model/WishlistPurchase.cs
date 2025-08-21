using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;
namespace VogelWedding.Model;

[Table("WishlistPurchases")]
public class WishlistPurchase : BaseModel
{
	[PrimaryKey("id", shouldInsert: false)]
	[Column("id")]
	public Guid ID { get; set; } = Guid.NewGuid();
	
	
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
	
	
	[Column("purchased_at")]
	public DateTimeOffset PurchasedAt { get; set; }
	
	
	[Column("email_sent")]
	public bool EmailSent { get; set; }
	
	
	[Column("money_received")]
	public bool MoneyReceived { get; set; }


	[Column("money_received_date")]
	public DateTimeOffset? MoneyReceivedDate { get; set; }
	
	[Column("email_sent_date")]
	public DateTimeOffset? EmailSentDate { get; set; }
}
