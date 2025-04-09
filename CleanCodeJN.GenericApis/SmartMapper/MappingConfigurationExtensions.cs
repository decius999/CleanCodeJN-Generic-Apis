namespace CleanCodeJN.GenericApis.SmartMapper;

public static class MappingConfigurationExtensions
{
    public static IServiceCollection AddMappingConfigurations(this IServiceCollection services, Action<MappingBuilder> configure = null)
    {
        var mapper = new SmartMapper();
        var builder = new MappingBuilder(mapper);

        configure?.Invoke(builder);

        services.AddSingleton<ISmartMapper>(mapper);
        return services;
    }
}
