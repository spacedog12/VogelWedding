using Supabase.Postgrest.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VogelWedding.Model;

[Table("RsvpEntry")]
public class RsvpEntry : BaseModel, IValidatableObject
{
    [PrimaryKey("id", shouldInsert: true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Column("first_name")]
    [Required(ErrorMessage = "Bitte Vorname angeben")]
    [Display(Name = "Vorname")]
    public string FirstName { get; set; } = string.Empty;
    
    
    [Column("last_name")]
    [Required(ErrorMessage = "Bitte Nachname angeben")]
    [Display(Name = "Nachname")]
    public string FamilyName { get; set; } = string.Empty;
    
    
    [Column("attending")]
    [Required(ErrorMessage = "Bitte gib an ob du teilnehmen wirst.")]
    public string Attending { get; set; }
    
    
    [Column("invited")]
    public bool Invited { get; set; }
    
    
    [Column("number_of_attendees")]
    [Range(0, 11, ErrorMessage = "Bitte gib die Anzahl Teilnehmer an.")]
    [Display(Name = "Anzahl Personen")]
    public int NumberOfAttendees { get; set; }
    
    
    // [Column("address")]
    // // [Required(ErrorMessage = "Wir brauchen deine Addresse für die Dankeskarte.")]
    // [Display(Name = "Addresse")]
    // public string Address { get; set; }
    
    
    [Column("street")]
    [Required(ErrorMessage = "Bitte Strasse angeben.")]
    [Display(Name = "Strasse")]
    public string Street { get; set; }
    
    
    [Column("zip_code")]
    [Required(ErrorMessage = "Bitte Postleitzahl angeben.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Bitte geben Sie eine gültige Postleitzahl ein (4 Ziffern)")]
    [Display(Name = "Postleitzahl")]
    public string ZipCode { get; set; }
    
    
    [Column("place")]
    [Required(ErrorMessage = "Bitte Ort angeben.")]
    [Display(Name = "Ort")]
    public string Place { get; set; }
    
    
    [Column("email_address")]
    [Required(ErrorMessage = "Bitte gib deine Email Addresse an.")]
    [EmailAddress(ErrorMessage = "Bitte geben Sie eine gültige E-Mail-Adresse ein")]
    [Display(Name = "Email Addresse")]
    public string EmailAddress { get; set; }
    
    
    [Column("buffet_contribution")]
    public bool BuffetContribution { get; set; }
    
    
    [Column("message")]
    [Display(Name = "Nachricht")]
    public string Message { get; set; }
    
    
    [Column("created_at")]
    [Display(Name = "Erstellt am")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.Equals(Attending, "CanNotAttend", StringComparison.OrdinalIgnoreCase))
        {
            if (NumberOfAttendees < 1 || NumberOfAttendees > 10)
            {
                yield return new ValidationResult(
                    "Bitte gib die Anzahl Teilnehmer an.",
                    new[] { nameof(NumberOfAttendees) });
            }
        }
    }
}