using Supabase.Storage.Exceptions;
namespace VogelWedding.Model;

using Supabase.Postgrest.Attributes;

[Table("InvoiceUrl")]
public class InvoiceModel : BaseModel
{
	[Column("id")]
	public int ID { get; set; }
	
	[Column("pdf_url")]
	public string PdfUrl { get; set; }
}
