namespace AIVectorSearch;

public class SearchResult
{
    public int Key { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Score { get; set; }
}