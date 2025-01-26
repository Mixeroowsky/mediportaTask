using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using mediporta.Entities;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using mediporta.Services;
using mediporta.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class TagsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{    
    private readonly WebApplicationFactory<Program> _factory;
    public TagsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {       
        _factory = factory;
    }
    private TagsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TagsDbContext>()
            .UseInMemoryDatabase(databaseName: "TagsDatabase")
            .Options;
        return new TagsDbContext(options);
    }

    [Fact]
    public async Task FetchTagsFromApi()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        var client = new HttpClient();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<TagsService>();
        var service = new TagsService(context, client, logger);

        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("mediportaTask/1.0");
        
        string testApiUrl = "https://api.stackexchange.com/2.3/tags?order=desc&pageSize=100&sort=popular&site=stackoverflow";

        // Act
        var tags = await service.FetchTagsFromApi(testApiUrl);
        await service.SaveTags(tags);
        var savedTags = await context.Tags.ToListAsync();

        // Assert
        Assert.NotEmpty(savedTags);
        Assert.True(savedTags.Count > 0);
        Assert.Contains(savedTags, t => t.name == "javascript" || t.name == "csharp");

        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task GetAllTags_ShouldReturnTags()
    {
        // Arrange
        var context = CreateInMemoryDbContext();
        context.Tags.Add(new Tag { name = "csharp", count = 150 });
        context.Tags.Add(new Tag { name = "aspnet", count = 200 });
        await context.SaveChangesAsync();

        var httpClient = new HttpClient();
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<TagsService>();

        var service = new TagsService(context, httpClient, logger);

        // Act
        var tags = await service.GetAllTags();

        // Assert
        Assert.Equal(2, tags.Count);
        Assert.Contains(tags, t => t.name == "csharp");
        Assert.Contains(tags, t => t.name == "aspnet");

        await context.Database.EnsureDeletedAsync();
    }
}