using CleanCodeJN.GenericApis.Chat.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CleanCodeJN.GenericApis.Chat.Extensions;

public static class ChatExtensions
{
    public static IServiceCollection AddCleanCodeJNWithAiChat(this IServiceCollection services, Action<ChatOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient("CleanCodeJNChat");
        services.AddScoped<ChatService>();
        return services;
    }
}
