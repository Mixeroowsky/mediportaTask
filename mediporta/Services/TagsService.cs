using mediporta.Context;
using mediporta.Dtos;
using mediporta.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace mediporta.Services
{
    public class TagsService(TagsDbContext context, HttpClient httpClient, ILogger<TagsService> logger) : ITagsService
    {
        private readonly TagsDbContext _context = context;
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<TagsService> _logger = logger;

        public async Task<List<Tag>> FetchTagsFromApi(string url)
        {
            _logger.LogInformation("Fetching tags from StackOverflow API started.");
            var tags = new List<Tag>();
            int page = 1;
            try
            {
                do
                {
                    var a = $"{url}&page={page}";
                    var response = await _httpClient.GetAsync($"{url}&page={page}");
                    response.EnsureSuccessStatusCode();

                    var content = await response.Content.ReadAsStringAsync();
                    var tagsResponse = JsonSerializer.Deserialize<TagsResponse>(content);

                    if (tagsResponse?.items == null || tagsResponse.items.Count == 0)
                    {
                        break;
                    }
                    tags.AddRange(tagsResponse.items.Select(t => new Tag
                    {
                        name = t.name,
                        count = t.count,
                    }));
                    page++;
                }
                while (tags.Count < 1000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching tags.");
                throw;
            }
            _logger.LogInformation("Fetching tags from StackOverflow API completed.");
            return tags;
        }

        public async Task SaveTags(List<Tag> tags)
        {
            try
            {
                foreach (var tag in tags)
                {
                    if (!_context.Tags.Any(t => t.name == tag.name))
                    {
                        await _context.Tags.AddAsync(tag);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving tags.");
                throw;
            }
            
        }

        public async Task<int> DeleteAllTags()
        {
            try
            {
                var Odds = await _context.Tags.ToListAsync();
                _context.RemoveRange(Odds);
                return await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting tags.");
                throw;
            }            
        }

        public async Task<(List<Tag>, int)> GetPaginatedTags(int page, int pageSize, string sort, string direction)
        {
            try
            {
                var query = _context.Tags.AsQueryable();
                query = sort.ToLower() switch
                {
                    "name" => direction.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(t => t.name) : query.OrderBy(t => t.name),
                    "percentage" => direction.Equals("desc", StringComparison.CurrentCultureIgnoreCase) ? query.OrderByDescending(t => t.count) : query.OrderBy(t => t.count),
                    _ => query.OrderBy(t => t.Id)
                };

                int totalCount = await query.CountAsync();
                var paginatedTags = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                return (paginatedTags, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting paginated tags.");
                throw;
            }            
        }

        public async Task<List<Tag>> GetAllTags()
        {
            try
            {
                var tags = await _context.Tags.ToListAsync();
                return tags;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting all tags.");
                throw;
            }            
        }
    }    
}
