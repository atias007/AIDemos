namespace MyFirstRag.Models;

// Class to store text chunks with their embeddings
public class VectorChunk
{
    public required string Text { get; set; }
    public required float[] Embedding { get; set; }
    public required int Index { get; set; }
    public float SimilarityScore { get; set; }
}