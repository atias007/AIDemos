using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace MyFirstRag;

internal static class DependencyInjectionFactory
{
    private static IServiceProvider _serviceProvider;

    static DependencyInjectionFactory()
    {
        var services = new ServiceCollection();
        services
            .AddHttpClient("openai-client")
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler { CheckCertificateRevocationList = false };
            })
            .AddStandardResilienceHandler();

        services
            .AddHttpClient("ollama-client")
            .ConfigureHttpClient(opt =>
            {
                opt.BaseAddress = new Uri("http://127.0.0.1:11434");
                opt.Timeout = TimeSpan.FromMinutes(2);
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler { CheckCertificateRevocationList = false };
            })
            .AddStandardResilienceHandler();

        services.AddSingleton<AppSettings>();
        services.AddSingleton<OpenAIEmbeddingService>();
        services.AddSingleton<OllamaEmbeddingService>();
        services.AddSingleton<ChatService>();
        _serviceProvider = services.BuildServiceProvider();
    }

    public static T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }
}