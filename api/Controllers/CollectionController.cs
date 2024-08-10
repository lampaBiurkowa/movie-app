using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CollectionController(CollectionService service) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Collection> GetCollections() => service.GetCollections();

    [HttpGet("{id}")]
    public async Task<Collection?> GetCollection(ulong id, CancellationToken ct) => await service.GetCollection(id, ct);
    
    [HttpPost]
    public async Task<ActionResult<ulong>> AddCollection(CreateCollectionRequest collection, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        return Ok(await service.AddCollection(collection, ct));
    }

    [HttpPost("{collectionId}/movie/{movieId}")]
    public async Task<IActionResult> AddToCollection(ulong collectionId, ulong movieId, CancellationToken ct)
    {
        await service.AddToCollection(collectionId, movieId, ct);
        return Ok();
    }

    [HttpDelete("{collectionId}/movie/{movieId}")]
    public async Task<IActionResult> RemoveFromCollection(ulong collectionId, ulong movieId, CancellationToken ct)
    {
        await service.RemoveFromCollection(collectionId, movieId, ct);
        return Ok();
    }

    [HttpGet("{collectionId}/count")]
    public async Task<ActionResult<int>> Count(ulong collectionId, CancellationToken ct) =>
        Ok(await service.GetMoviesCount(collectionId, ct));

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCollection(ulong id, CancellationToken ct)
    {
        await service.DeleteCollection(id, ct);
        return Ok();
    }
}