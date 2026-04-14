using System.ComponentModel.DataAnnotations;

namespace KnotShoreRealty.Core.Models;

public class Inquiry
{
    public int Id { get; set; }
    public int ListingId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Phone { get; set; }

    [Required]
    [MinLength(10)]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }

    public Listing Listing { get; set; } = null!;
}
