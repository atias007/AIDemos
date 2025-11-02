using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyFirstRag.Models;

namespace MyFirstRag;

internal class ChatService(
    OllamaEmbeddingService embeddingService,
    AppSettings appSettings,
    IHttpClientFactory clientFactory)
{
    /// <summary>
    /// Ask question with RAG pattern - retrieve relevant context and generate answer
    /// </summary>
    public async Task<RagResponse> AskQuestionWithRAGAsync(
        string question,
        IEnumerable<VectorChunk> documentChunks,
        int topK = 3,
        double temperature = 0.7)
    {
        // Step 1: Find relevant chunks using embeddings
        Console.WriteLine($"\nSearching for relevant context...");
        var relevantChunks = await FindRelevantChunksAsync(question, documentChunks, topK);

        Console.WriteLine($"Found {relevantChunks.Count} relevant chunks:");
        foreach (var chunk in relevantChunks)
        {
            Console.WriteLine($"  - Chunk {chunk.Index} (similarity: {chunk.SimilarityScore:F4})");
        }

        // Step 2: Build context from relevant chunks
        var context = PromptBuilder.BuildContext(relevantChunks);

        // Step 3: Create prompt with context and question
        var prompt = PromptBuilder.BuildPrompt(context, question);

        // Step 4: Send to OpenAI using Semantic Kernel
        Console.WriteLine($"\nGenerating answer...");
        var answer = await GetChatCompletionAsync(prompt, temperature);

        return new RagResponse
        {
            Question = question,
            Answer = answer,
            RelevantChunks = relevantChunks,
            Context = context,
            Prompt = prompt
        };
    }

    /// <summary>
    /// Find relevant chunks using semantic search
    /// </summary>
    private async Task<List<VectorChunk>> FindRelevantChunksAsync(
        string query,
        IEnumerable<VectorChunk> allChunks,
        int topK = 3)
    {
        // Generate embedding for the query
        var queryEmbedding = await embeddingService.CreateVectorStore(query);
        var queryVector = queryEmbedding.Embedding;

        // Calculate similarities and sort
        var rankedChunks = allChunks
            .Select(chunk => new
            {
                Chunk = chunk,
                Similarity = CosineSimilarity(queryVector, chunk.Embedding)
            })
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .Select(x =>
            {
                x.Chunk.SimilarityScore = x.Similarity;
                return x.Chunk;
            })
            .ToList();

        return rankedChunks;
    }

    private static float CosineSimilarity(float[] vector1, float[] vector2)
    {
        float dotProduct = 0;
        float magnitude1 = 0;
        float magnitude2 = 0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        return dotProduct / (float)(Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
    }

    /// <summary>
    /// Get chat completion using Semantic Kernel
    /// </summary>
    private async Task<string> GetChatCompletionAsync(string prompt, double temperature = 0.7)
    {
        var settings = new OpenAIPromptExecutionSettings
        {
            Temperature = temperature,
            MaxTokens = 1000,
            TopP = 1.0
        };

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(PromptBuilder.GetSystemMessage());
        chatHistory.AddUserMessage(prompt);

        using var httpClient = clientFactory.CreateClient("openai-client");

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(appSettings.ChatModel, appSettings.ApiKey, httpClient: httpClient);
        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var result = await chatService.GetChatMessageContentAsync(chatHistory, settings);
        return result.Content ?? string.Empty;
    }
}