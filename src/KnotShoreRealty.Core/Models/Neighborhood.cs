namespace KnotShoreRealty.Core.Models;

public class Neighborhood
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? HeroImageUrl { get; set; }
    public int? ParentId { get; set; }

    public Neighborhood? Parent { get; set; }
    public List<Neighborhood> Children { get; set; } = new();
    public List<Listing> Listings { get; set; } = new();
}
