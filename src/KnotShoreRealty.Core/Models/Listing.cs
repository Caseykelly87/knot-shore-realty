using KnotShoreRealty.Core.Enums;

namespace KnotShoreRealty.Core.Models;

public class Listing
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Bedrooms { get; set; }
    public decimal Bathrooms { get; set; }
    public int SquareFeet { get; set; }
    public string Description { get; set; } = string.Empty;
    public ListingStatus Status { get; set; } = ListingStatus.Active;
    public ListingType ListingType { get; set; }
    public PropertyType PropertyType { get; set; }
    public DateTime ListedDate { get; set; }
    public int NeighborhoodId { get; set; }
    public int AgentId { get; set; }

    public Neighborhood Neighborhood { get; set; } = null!;
    public Agent Agent { get; set; } = null!;
    public List<ListingImage> Images { get; set; } = new();
}
