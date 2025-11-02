namespace AIVectorSearch;

internal class CloudService
{
    public const string IndexName = "services";

    public int Key { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public float[] Vector { get; set; } = [];
}