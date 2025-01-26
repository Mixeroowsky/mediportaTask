using mediporta.Context;
using mediporta.Dtos;
using mediporta.Entities;
using mediporta.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace mediporta.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController(ITagsService tagService) : ControllerBase
    {
        private readonly ITagsService _tagService = tagService;

        /// <summary>
        /// Fetches list of tags from external api and stores them into database
        /// </summary>
        /// <returns>List of sorted tags</returns>
        /// <response code="200">List of tags fetched and saved successfully.</response>
        /// <response code="404">Tags not found.</response>
        [HttpGet("fetch-tags")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Tag>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult<List<Tag>>> FetchAndSaveTags()
        {
            try
            {
                var tags = await _tagService.FetchTagsFromApi("/tags?order=desc&pageSize=100&sort=popular&site=stackoverflow&");
                if (tags == null || tags.Count == 0)
                {
                    return NotFound("Tags not found.");
                }

                await _tagService.SaveTags(tags);
                return Ok($"Fetched and saved {tags.Count} tags");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, new { Message = "External API error", Detail = ex.Message });                
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { Message = "Internal Server Error", Detail = ex.Message });
            }
        }

        /// <summary>
        /// Deletes all tags
        /// </summary>
        /// <returns>Number of deleted tags</returns>
        /// <response code="200">Tags deleted successfully</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [HttpDelete("delete-tags")]
        public async Task<IActionResult> DeleteAllTags()
        {
            try
            {
                int deletedRows = await _tagService.DeleteAllTags();
                return Ok(new { deletedRows });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error", Detail = ex.Message });                
            }            
        }

        /// <summary>
        /// Fetches list of tags with sort and order option from database
        /// </summary>
        /// <param name="sort">Sort: 'name' or 'count'.</param>
        /// <param name="order">Order: 'asc' or 'desc'.</param>
        /// <returns>List of sorted tags</returns>
        /// <response code="200">List of tags fetched successfully.</response>
        /// <response code="204">None of the tags were fetched</response>
        [HttpGet("paginate-tags")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Tag>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<TagDto>>> GetPaginatedTags([FromQuery] string sort = "name",
                                                                    [FromQuery] string order = "asc",                                                                    
                                                                    [FromQuery] int pageSize = 20,
                                                                    [FromQuery] int pages = 1) 
        {
            try
            {
                var (tags, totalTagsCount) = await _tagService.GetPaginatedTags(pages, pageSize, sort, order);
                if (tags == null || tags.Count == 0)
                {
                    return NotFound("No tags found.");
                }
                var tagsDto = tags.Select(tag => new TagDto
                {
                    name = tag.name,
                    count = tag.count,
                    percentage = totalTagsCount > 0
                ? Math.Round(CountPercentage(tag), 2)
                : 0
                }).ToList();
                return Ok(new
                {
                    TotalCount = totalTagsCount,
                    Page = pages,
                    PageSize = pageSize,
                    Data = tagsDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal Server Error", Detail = ex.Message });
            }            
        }

        private double CountPercentage(Tag tag)
        {
            double totalCount = 0;
            var tags = _tagService.GetAllTags();
            foreach (var item in tags.Result)
            {
                totalCount += item.count;
            }
            return 100 * tag.count / totalCount;
        }
    }
}
