namespace VogelWedding.Model;

using Supabase.Postgrest.Attributes;

[Table("InformationImages")]
public class InformationImages : BaseModel
{
	[Column("image_url")]
	public string ImageUrl { get; set; } 
	
	[Column("title")]
	public string Title { get; set; }
}
