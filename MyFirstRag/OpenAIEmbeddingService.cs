using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using MyFirstRag.Models;

namespace MyFirstRag;

internal class OpenAIEmbeddingService(AppSettings appSettings, IHttpClientFactory clientFactory)
{
    // Create embeddings and store them with original text
    public async Task<List<VectorChunk>> CreateVectorStore(List<string> chunks)
    {
        using var httpClient = clientFactory.CreateClient("openai-client");

        var kernelBuilder = Kernel.CreateBuilder();
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        kernelBuilder.AddOpenAIEmbeddingGenerator(
            modelId: appSettings.OpenAIEmbeddingModel,
            apiKey: appSettings.ApiKey,
            httpClient: httpClient);
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        var kernel = kernelBuilder.Build();
        var embeddingService = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        var embeddings = await embeddingService.GenerateAsync(chunks);
        var vectorStore = chunks.Zip(embeddings).Select((z, i) => new VectorChunk { Embedding = z.Second.Vector.ToArray(), Text = z.First, Index = i }).ToList();
        return vectorStore;
    }

    public async Task<VectorChunk> CreateVectorStore(string value)
    {
        var result = await CreateVectorStore([value]);
        return result.First();
    }
}