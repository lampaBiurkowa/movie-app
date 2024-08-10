namespace Api.Options;

public class ExternalClientOptions
{
    public const string SECTION = "ExternalClient";

    public required string BaseAddress { get; set; }
}