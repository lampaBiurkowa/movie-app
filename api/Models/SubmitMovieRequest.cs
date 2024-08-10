using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class SubmitMovieRequest {
    public required string Title { get; set; }
    public string? Director { get; set; }
    [Range(1900, 2200, ErrorMessage = "Invalid year. Values from range 1900 to 2200 allowd")]
    public int Year { get; set; }
    public float? Rate { get; set; }
    public ulong? CollectionId { get; set; }
}