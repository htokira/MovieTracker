using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MovieTracker.API.Services;
using MovieTracker.Core.Dtos;
using MovieTracker.Data;
using Moq;

namespace MovieTracker.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "IfthisworkworkIwillgencry6767yayhappyhappy",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                IDbContextOptionsConfiguration<MovieTrackerContext>>();
            services.RemoveAll<DbContextOptions<MovieTrackerContext>>();

            services.AddDbContext<MovieTrackerContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<ITmdbService>();

            var tmdbServiceMock = new Mock<ITmdbService>();
            tmdbServiceMock
                .Setup(s => s.GetMovieDetailsAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new MovieDetailDto
                {
                    Id = id,
                    Title = $"Test Movie {id}",
                    Description = "Test Description",
                    ReleaseYear = 2024,
                    Rating = 8.5,
                    MediaType = "Movie",
                    PosterUrl = "https://example.com/poster.jpg"
                });

            services.AddSingleton(tmdbServiceMock.Object);
        });
    }
}