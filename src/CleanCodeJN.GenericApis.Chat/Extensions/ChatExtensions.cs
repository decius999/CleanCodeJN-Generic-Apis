using CleanCodeJN.GenericApis.Chat.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CleanCodeJN.GenericApis.Chat.Extensions;

/// <summary>Provides extension methods for registering AI chat services with the dependency injection container.</summary>
public static class ChatExtensions
{
    /// <summary>Registers the AI chat services, including the named HTTP client and <see cref="ChatService"/>, and applies the provided configuration.</summary>
    /// <param name="services">The service collection to add the chat services to.</param>
    /// <param name="configure">An action to configure <see cref="ChatOptions"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddCleanCodeJNWithAiChat(this IServiceCollection services, Action<ChatOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient("CleanCodeJNChat");
        services.AddScoped<ChatService>();
        return services;
    }
}
