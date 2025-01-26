namespace mediporta.Dtos
{
    public class TagsResponse
    {
        public List<TagDto> items { get; set; }
    }
    public class TagDto
    {        
        public string name { get; set; }
        public int count { get; set; }
        public double percentage { get; set; }
    }
}
