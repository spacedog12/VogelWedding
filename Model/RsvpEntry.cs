// using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Attributes;

namespace VogelWedding.Model;

[System.ComponentModel.DataAnnotations.Schema.Table("RsvpEntry")]
public class RsvpEntry : BaseModel
{
    [PrimaryKey("id", shouldInsert: true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;
    
    [Column("last_name")]
    public string FamilyName { get; set; } = string.Empty;
    
    [Column("attending")]
    public bool Attending { get; set; }
    
    [Column("menu")]
    public bool Menu { get; set; }
    
}