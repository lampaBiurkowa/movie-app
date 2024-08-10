using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class Collection {
    public ulong Id { get; set; }
    [StringLength(200, ErrorMessage = "Title too long. Up to 200 characters allowed")]
    public required string Title { get; set; }
    public List<Movie> Movies { get; set; } = [];
}