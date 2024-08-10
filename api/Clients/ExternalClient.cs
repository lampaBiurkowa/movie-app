using System.Runtime.Serialization;
using System.Text.Json;
using Api.Models;

namespace Api.Clients;

public class ExternalClient(IWebHostEnvironment env, HttpClient httpClient)
{
    readonly JsonSerializerOptions serializationOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<ExternalMovie>> GetMovies(CancellationToken ct) =>
        await HandleResponse<List<ExternalMovie>>("MyMovies", ct) ?? [];

    public async Task<T?> HandleResponse<T>(string endpoint, CancellationToken ct)
    {
        HttpResponseMessage response = await httpClient.GetAsync(endpoint, ct);
        response.EnsureSuccessStatusCode();

        string responseContent = await response.Content.ReadAsStringAsync(ct);

        try
        {
            return JsonSerializer.Deserialize<T?>(responseContent, serializationOptions);
        }
        catch
        {
            if (env.IsProduction())
            {
                throw new SerializationException("Error deserializing response from an external API");
            }
            else
            {
                throw;
            }
        }
    }
}
