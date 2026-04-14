namespace KnotShoreRealty.Core.Models;

public class Agent
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }

    public List<Listing> Listings { get; set; } = new();
}
