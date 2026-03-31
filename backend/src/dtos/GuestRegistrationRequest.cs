using System.ComponentModel.DataAnnotations;

public class GuestRegistrationRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    public string DocumentId { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    [Required]
    public string Country { get; set; } = string.Empty;
}
