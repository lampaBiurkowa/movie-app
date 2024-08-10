using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class Movie {
    public ulong Id { get; set; }
    public ulong? ExternalId { get; set; }
    [StringLength(200, ErrorMessage = "Title too long. Up to 200 characters allowed")]
    public required string Title { get; set; }
    public string? Director { get; set; }
    [Range(1900, 2200, ErrorMessage = "Invalid year. Values from range 1900 to 2200 allowd")]
    public int Year { get; set; }
    public float? Rate { get; set; }
    public ulong? CollectionId { get; set; }
}