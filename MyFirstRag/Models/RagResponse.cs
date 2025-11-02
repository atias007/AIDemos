namespace MyFirstRag.Models;

public class RagResponse
{
    public required string Question { get; set; }
    public required string Answer { get; set; }
    public required List<VectorChunk> RelevantChunks { get; set; }
    public required string Context { get; set; }
    public required string Prompt { get; set; }
}