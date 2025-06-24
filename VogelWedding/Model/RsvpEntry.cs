// using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Attributes;
using System.ComponentModel.DataAnnotations;

namespace VogelWedding.Model;

[System.ComponentModel.DataAnnotations.Schema.Table("RsvpEntry")]
public class RsvpEntry : BaseModel
{
    [PrimaryKey("id", shouldInsert: true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Column("first_name")]
    [Required(ErrorMessage = "Bitte Vorname angeben")]
    public string FirstName { get; set; } = string.Empty;
    
    [Column("last_name")]
    [Required(ErrorMessage = "Bitte Nachname angeben")]
    public string FamilyName { get; set; } = string.Empty;
    
    [Column("attending")]
    [Required(ErrorMessage = "Bitte gib an ob du teilnehmen wirst.")]
    public bool? Attending { get; set; }
    
    [Column("menu")]
    public bool Menu { get; set; }
    
    [Column("invited")]
    public bool Invited { get; set; }
    
}