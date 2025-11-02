using AIVectorSearch;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Embeddings;
using OpenSearch.Client;
using OpenSearch.Net;

// Step 1: Create the embedding generator.
var embeddClient = InitializeEmbeddingClient();

// Step 2: Create the OpenSearch client
var openSearchClient = InitializeOpenSearchClient();

// Step 3: Create index in OpenSearch
var success = await CreateOpenSearchIndex(openSearchClient);

if (success)
{
    // Step 4: Load Data
    var services = Data.CloudServices;
    var descriptions = services.Select(s => s.Description);

    // Step 5: Create embeddings
    var embeddings = await CreateEmbeddings(descriptions, embeddClient);

    // Step 6: Save embeddings to db
    await SaveEmbeddings(embeddings, services, openSearchClient);
}

// Step 7: Ask user for prompt
Console.Write("Enter your search prompt: ");
var userPrompt = Console.ReadLine() ?? string.Empty;
Console.WriteLine();

// Step 8: Create embedding for user prompt
var searchVector = await CreateEmbedding(userPrompt, embeddClient);

// Step 9: Search for similar service
var similarService = await SearchSimilarStudents(searchVector, 1, openSearchClient);

// Step 10: Print result
PrintResult(similarService);

static EmbeddingClient InitializeEmbeddingClient()
{
    // Initialize OpenAI client
    const string model = "text-embedding-3-small";
    var openAIKey = Environment.GetEnvironmentVariable("OPEN_AI_API_KEY") ?? throw new InvalidDataException("api key is null");
    var openAIClient = new OpenAIClient(openAIKey);
    var embeddingClient = openAIClient.GetEmbeddingClient(model);
    Console.WriteLine("✓ Initialized OpenAI client\n");
    return embeddingClient;
}

static OpenSearchClient InitializeOpenSearchClient()
{
    // Initialize OpenSearch client
    const string openSearchUrl = "http://localhost:9200";

    var connectionSettings = new ConnectionSettings(new Uri(openSearchUrl))
        .ServerCertificateValidationCallback((o, certificate, chain, errors) => true) // For dev/testing
        .DefaultIndex(CloudService.IndexName);

    var openSearchClient = new OpenSearchClient(connectionSettings);

    Console.WriteLine("✓ Initialized OpenAI and OpenSearch clients\n");
    return openSearchClient;
}

static async Task<float[]> CreateEmbedding(string text, EmbeddingClient client)
{
    var response = await client.AsIEmbeddingGenerator().GenerateAsync(text);
    Console.WriteLine("✓ Created embedding from prompt\n");
    return response.Vector.ToArray();
}

static async Task<IEnumerable<float[]>> CreateEmbeddings(IEnumerable<string> texts, EmbeddingClient client)
{
    var response = await client.AsIEmbeddingGenerator().GenerateAsync(texts);
    Console.WriteLine($"✓ Create OpenAI embeddings for {texts.Count()} items\n");
    return response.Select(r => r.Vector.ToArray());
}

static async Task SaveEmbeddings(IEnumerable<float[]> embeddings, IEnumerable<CloudService> services, OpenSearchClient openSearchClient)
{
    foreach (var (i, service) in services.Index())
    {
        service.Vector = embeddings.ElementAt(i);
    }

    foreach (var service in services)
    {
        Console.WriteLine($"Processing service: {service.Name}...");

        // Index document in OpenSearch
        var indexResponse = await openSearchClient.IndexAsync(service, idx => idx
            .Index(CloudService.IndexName)
            .Id(service.Key.ToString())
            .Refresh(Refresh.True)
        );

        if (!indexResponse.IsValid)
        {
            throw new Exception($"Failed to index service {service.Key}: {indexResponse.ServerError?.Error?.Reason}");
        }
    }

    Console.WriteLine("✓ Save Services To DB\n");
}

static async Task<bool> CreateOpenSearchIndex(OpenSearchClient openSearchClient)
{
    var exists = await openSearchClient.Indices.ExistsAsync(CloudService.IndexName);
    if (exists.Exists) { return false; }

    // Delete index if it exists
    // var deleteResponse = await openSearchClient.Indices.DeleteAsync(CloudService.IndexName);

    // Create index with vector field mapping
    var createIndexResponse = await openSearchClient.Indices.CreateAsync(CloudService.IndexName, c => c
        .Settings(s => s
            .Setting("index.knn", true)
        )
        .Map<CloudService>(m => m
            .Properties(p => p
                .Number(n => n.Name(f => f.Key))
                .Text(t => t.Name(f => f.Name))
                .Text(t => t.Name(f => f.Description))
                .KnnVector(k => k
                    .Name(f => f.Vector)
                    .Dimension(1536) // OpenAI text-embedding-3-small dimension
                    .Method(km => km
                        .Name("hnsw")
                        .SpaceType("l2") //"cosinesimil"
                    //.Engine("nmslib")
                    )
                )
            )
        )
    );

    if (!createIndexResponse.IsValid)
    {
        throw new Exception($"Failed to create index: {createIndexResponse.ServerError?.Error?.Reason}");
    }

    Console.WriteLine("✓ Created embedding index\n");
    return true;
}

static async Task<IEnumerable<SearchResult>> SearchSimilarStudents(float[] queryVector, int k, OpenSearchClient openSearchClient)
{
    var searchResponse = await openSearchClient.SearchAsync<CloudService>(s => s
        .Index(CloudService.IndexName)
        .Size(k)
        .Query(q => q
            .Knn(knn => knn
                .Field(f => f.Vector)
                .Vector(queryVector)
                .K(k)
            )
        )
    );

    if (!searchResponse.IsValid)
    {
        throw new Exception($"Search failed: {searchResponse.ServerError?.Error?.Reason}");
    }

    return searchResponse.Documents.Select((doc, index) => new SearchResult
    {
        Key = doc.Key,
        Name = doc.Name,
        Description = doc.Description,
        Score = searchResponse.Hits.ElementAt(index).Score ?? 0
    });
}

static void PrintResult(IEnumerable<SearchResult> results)
{
    Console.WriteLine($"Top {results.Count()} most similar service:\n");
    Console.WriteLine("=".PadRight(80, '='));

    foreach (var result in results)
    {
        Console.WriteLine($"\nService Key: {result.Key}");
        Console.WriteLine($"Name: {result.Name}");
        Console.WriteLine($"Description: {result.Description}");
        Console.WriteLine($"Similarity Score: {result.Score:F4}");
        Console.WriteLine("-".PadRight(80, '-'));
    }
}