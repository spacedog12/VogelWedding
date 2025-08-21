namespace VogelWedding.Model;

using Supabase.Postgrest.Attributes;

[Table("ChapelImages")]
public class ChapelImages : BaseModel
{
	[Column("image_url")]
	public string ImageUrl { get; set; }
}
