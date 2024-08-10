using Api.Clients;
using Api.Infrastructure;
using Api.Options;
using Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<ExternalClient>(client =>
{
    var options = builder.Configuration.GetSection(ExternalClientOptions.SECTION).Get<ExternalClientOptions>() ?? throw new("No ext client options");
    client.BaseAddress = new Uri(options.BaseAddress);
});

builder.Services.AddDbContext<DbContext, MovieContext>();
builder.Services.AddOptions<ExternalClientOptions>().Bind(builder.Configuration.GetSection(ExternalClientOptions.SECTION));
builder.Services.AddOptions<DbOptions>().Bind(builder.Configuration.GetSection(DbOptions.SECTION));
builder.Services.AddScoped<MovieService>();
builder.Services.AddScoped<CollectionService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
