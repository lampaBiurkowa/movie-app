using Api.Models;
using Api.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace Api.Infrastructure;

public class MovieContext : DbContext
{
    private readonly DbOptions _options;

    public MovieContext(IOptions<DbOptions> options) : base()
    {
        _options = options.Value;
        Database.EnsureCreated();
    }
    
    public DbSet<Movie> Movie { get; set; }
    public DbSet<Collection> Collection { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_options.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Movie>()
            .HasIndex(e => e.ExternalId)
            .IsUnique();
    }
}