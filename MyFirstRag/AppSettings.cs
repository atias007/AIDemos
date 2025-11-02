namespace MyFirstRag;

internal class AppSettings
{
    public readonly string ChatModel = "gpt-4o-mini";
    public readonly string OpenAIEmbeddingModel = "text-embedding-3-small";
    public readonly string OllamaEmbeddingModel = "nomic-embed-text";
    public string ApiKey => Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new InvalidDataException("api key is null");
}