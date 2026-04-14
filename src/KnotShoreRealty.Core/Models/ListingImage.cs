namespace KnotShoreRealty.Core.Models;

public class ListingImage
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsPrimary { get; set; } = false;

    public Listing Listing { get; set; } = null!;
}
