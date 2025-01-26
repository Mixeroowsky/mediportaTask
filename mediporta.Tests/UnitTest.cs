using mediporta.Context;
using mediporta.Entities;
using mediporta.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

public class TagServiceTests
{
    private TagsDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<TagsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TagsDbContext(options);
    }

    [Fact]
    public async Task SaveTags()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        
        context.Tags.Add(new Tag { Id = 1, name = "csharp", count = 150 });
        await context.SaveChangesAsync();

        var newTags = new List<Tag>
        {
            new() { name = "aspnet", count = 200 },  
            new() { name = "csharp", count = 150 }  
        };

        var service = new TagsService(context, null, null);

        // Act
        await service.SaveTags(newTags);

        // Assert
        var savedTags = context.Tags.ToList();
        Assert.Equal(2, savedTags.Count);
        Assert.Contains(savedTags, t => t.name == "aspnet"); 
        Assert.Contains(savedTags, t => t.name == "csharp"); 
    }

    [Fact]
    public async Task GetAllTags()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();

        context.Tags.Add(new Tag { name = "csharp", count = 150 });
        context.Tags.Add(new Tag { name = "aspnet", count = 200 });
        await context.SaveChangesAsync();

        var service = new TagsService(context, null, null);

        // Act
        var tags = await service.GetAllTags();

        // Assert
        Assert.Equal(2, tags.Count); 
        Assert.Contains(tags, t => t.name == "csharp");
        Assert.Contains(tags, t => t.name == "aspnet");
    }
}