namespace Api.Options;

public class DbOptions
{
    public const string SECTION = "Db";

    public required string ConnectionString { get; set; }
}