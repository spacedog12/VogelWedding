namespace VogelWedding.Model;
using Supabase.Postgrest.Attributes;

[Table("AboutImages")]
public class AboutImages : BaseModel
{
	[Column("image_url")]
	public string ImageUrl { get; set; }
	
	[Column("section")]
	public string Section { get; set; }
	
	[Column("title")]
	public bool Title { get; set; }
}
