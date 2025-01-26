using mediporta.Entities;

namespace mediporta.Services
{
    public interface ITagsService
    {
        Task<List<Tag>> FetchTagsFromApi(string url);
        Task SaveTags(List<Tag> tags);
        Task<int> DeleteAllTags();
        Task<(List<Tag>,int)> GetPaginatedTags(int page, int pageSize, string sort, string direction);
        Task<List<Tag>> GetAllTags();
    }
}
