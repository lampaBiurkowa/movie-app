using System.ComponentModel.DataAnnotations;
using Api.Models;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public record CollectionMoviePair(Movie Movie, Collection Collection);

public class CollectionService(MovieContext ctx)
{
    public IEnumerable<Collection> GetCollections() => ctx.Collection;

    public async Task<Collection?> GetCollection(ulong id, CancellationToken ct) => await ctx.Collection.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ct);

    public async Task<ulong> AddCollection(CreateCollectionRequest collectionRequest, CancellationToken ct)
    {
        var collection = Map(collectionRequest);
        TryInsertCollection(collection);
        await ctx.SaveChangesAsync(ct);

        return collection.Id;
    }

    public async Task DeleteCollection(ulong id, CancellationToken ct)
    {
        using var transaction = await ctx.Database.BeginTransactionAsync(ct);
        try
        {
            var movies = await ctx.Movie.Where(x => x.CollectionId == id).ToListAsync(ct);

            foreach (var movie in movies)
            {
                movie.CollectionId = null;
            }

            await ctx.SaveChangesAsync(ct);

            var collection = await ctx.Collection.FindAsync(id);
            if (collection != null)
            {
                ctx.Collection.Remove(collection);
                await ctx.SaveChangesAsync(ct);
            }

            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task AddToCollection(ulong collectionId, ulong movieId, CancellationToken ct)
    {
        var collectionMoviePair = GetCollectionMoviePair(collectionId, movieId, ct);
        collectionMoviePair.Collection.Movies.Add(collectionMoviePair.Movie);

        await ctx.SaveChangesAsync(ct);
    }

    public async Task RemoveFromCollection(ulong collectionId, ulong movieId, CancellationToken ct)
    {
        var collectionMoviePair = GetCollectionMoviePair(collectionId, movieId, ct);
        collectionMoviePair.Collection.Movies.RemoveAll(x => x.Id == movieId);

        await ctx.SaveChangesAsync(ct);
    }

    public async Task<int> GetMoviesCount(ulong collectionId, CancellationToken ct) =>
        await ctx.Movie.CountAsync(x => x.CollectionId == collectionId, cancellationToken: ct);

    private CollectionMoviePair GetCollectionMoviePair(ulong collectionId, ulong movieId, CancellationToken ct)
    {
        var collectionTask = ctx.Collection.FirstOrDefaultAsync(x => x.Id == collectionId, cancellationToken: ct);
        var movieTask = ctx.Movie.FirstOrDefaultAsync(x => x.Id == movieId, cancellationToken: ct);
        var entities = Task.WhenAll(collectionTask, movieTask);

        var movie = movieTask.Result ?? throw new($"No movie with id {movieId}");
        var collection = collectionTask.Result ?? throw new($"No collection with id {collectionId}");

        return new CollectionMoviePair(movie, collection);
    }

    private void TryInsertCollection(Collection collection)
    {
        var validationResults = new List<ValidationResult>();
        if (ValidateCollection(collection, validationResults))
        {
            ctx.Collection.Add(collection);
        }
        else
        {
            throw new ValidationException($"Collection model is invalid: {string.Join(',', validationResults.Select(x => x.ErrorMessage))}");
        }
    }

    private static bool ValidateCollection(Collection collection, List<ValidationResult> results)
    {
        var context = new ValidationContext(collection);
        return Validator.TryValidateObject(collection, context, results, true);
    }

    private static Collection Map(CreateCollectionRequest external)
    {
        return new() {
            Title = external.Title ?? string.Empty,
        };
    }
}