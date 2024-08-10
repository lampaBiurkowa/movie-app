namespace Api.Models;

public class ExternalMovie {
    public ulong Id { get; set; }
    public string? Title { get; set; }
    public string? Director { get; set; }
    public int Year { get; set; }
    public float Rate { get; set; }
}