using System.ComponentModel.DataAnnotations;
using Api.Models;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class MovieService(MovieContext ctx)
{

    public IEnumerable<Movie> GetMovies() => ctx.Movie;

    public async Task<Movie?> GetMovie(ulong id, CancellationToken ct) => await ctx.Movie.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ct);

    public async Task<ulong> AddMovie(SubmitMovieRequest movieRequest, CancellationToken ct)
    {
        var movie = Map(movieRequest);
        TryInsertMovie(movie);
        await ctx.SaveChangesAsync(ct);

        return movie.Id;
    }

    public async Task AddExternalMovies(IEnumerable<ExternalMovie> externalMovies, CancellationToken ct)
    {
        var movies = externalMovies.Select(Map).ToList();
        var externalIds = externalMovies.Select(x => x.Id).ToList();

        using var transaction = await ctx.Database.BeginTransactionAsync(ct);
        //race-resistant
        try
        {
            var existingExternalMovies = await GetMoviesByExtrnalIds(externalIds, ct);
            var newMovies = movies.Where(m => !existingExternalMovies.Any(y => y.ExternalId == m.ExternalId)).ToList();

            foreach (var movie in newMovies)
            {
                if (!await ctx.Movie.AnyAsync(e => e.ExternalId == movie.ExternalId, ct))
                {
                    ctx.Movie.Add(movie);
                }
            }

            await ctx.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task DeleteMovie(ulong id, CancellationToken ct)
    {
        await ctx.Movie.Where(x => x.Id == id).ExecuteDeleteAsync(ct);
        await ctx.SaveChangesAsync(ct);
    }

    public async Task UpdateMovie(ulong id, SubmitMovieRequest movieRequest, CancellationToken ct)
    {
        var existingMovie = await ctx.Movie.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new($"No movie with id {id}");
        existingMovie.Director = movieRequest.Director; // 😐
        existingMovie.Rate = movieRequest.Rate;
        existingMovie.Title = movieRequest.Title;
        existingMovie.Year = movieRequest.Year;
        existingMovie.CollectionId = movieRequest.CollectionId;

        var validationResults = new List<ValidationResult>();
        if (ValidateMovie(existingMovie, validationResults))
        {
            ctx.Entry(existingMovie).State = EntityState.Modified;
            await ctx.SaveChangesAsync(ct);
        }
        else
        {
            throw new ValidationException($"Movie model is invalid: {string.Join(',', validationResults.Select(x => x.ErrorMessage))}");
        }
    }

    private void TryInsertMovie(Movie movie)
    {
        var validationResults = new List<ValidationResult>();
        if (ValidateMovie(movie, validationResults))
        {
            ctx.Movie.Add(movie);
        }
        else
        {
            throw new ValidationException($"Movie model is invalid: {string.Join(',', validationResults.Select(x => x.ErrorMessage))}");
        }
    }

    private static bool ValidateMovie(Movie movie, List<ValidationResult> results)
    {
        var context = new ValidationContext(movie);
        return Validator.TryValidateObject(movie, context, results, true);
    }

    private Movie Map(ExternalMovie external)
    {
        return new() {
            ExternalId = external.Id,
            Title = external.Title ?? string.Empty,
            Director = external.Director,
            Year = external.Year,
            Rate = external.Rate
        };
    }


    private static Movie Map(SubmitMovieRequest request)
    {
        return new() {
            Title = request.Title ?? string.Empty,
            Director = request.Director,
            Year = request.Year,
            Rate = request.Rate,
            CollectionId = request.CollectionId
        };
    }

    private async Task<List<Movie>> GetMoviesByExtrnalIds(IEnumerable<ulong> ids, CancellationToken ct) =>
        await ctx.Movie.Where(x => ids.Any(y => y == x.ExternalId)).ToListAsync(ct);
}