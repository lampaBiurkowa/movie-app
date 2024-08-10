using Api.Clients;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class IntegrationController(ExternalClient client, MovieService service)
{
    [HttpPost]
    public async Task<IActionResult> Integrate(CancellationToken ct)
    {
        var movies = await client.GetMovies(ct);
        await service.AddExternalMovies(movies, ct);

        return new OkResult();
    }
}