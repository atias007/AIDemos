internal class ChunksUtil
{
    // Split text into overlapping chunks
    public static List<string> SplitIntoChunks(string text, int chunkSize, int overlap)
    {
        var chunks = new List<string>();
        int position = 0;

        while (position < text.Length)
        {
            int length = Math.Min(chunkSize, text.Length - position);
            string chunk = text.Substring(position, length);
            chunks.Add(chunk);

            position += chunkSize - overlap;
        }

        return chunks;
    }
}