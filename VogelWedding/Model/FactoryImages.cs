namespace VogelWedding.Model;

using Supabase.Postgrest.Attributes;

[Table("FactoryImages")]
public class FactoryImages : BaseModel
{
	[Column("image_url")]
	public string ImageUrl { get; set; }
}
