using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MovieController(MovieService service) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Movie> GetMovies() => service.GetMovies();

    [HttpGet("{id}")]
    public async Task<Movie?> GetMovie(ulong id, CancellationToken ct) => await service.GetMovie(id, ct);
    
    [HttpPost]
    public async Task<ActionResult<ulong>> AddMovie(SubmitMovieRequest movie, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Ok(await service.AddMovie(movie, ct));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMovie(ulong id, SubmitMovieRequest movie, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await service.UpdateMovie(id, movie, ct);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMovie(ulong id, CancellationToken ct)
    {
        await service.DeleteMovie(id, ct);
        return Ok();
    }
}