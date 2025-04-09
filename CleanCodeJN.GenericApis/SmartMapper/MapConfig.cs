namespace CleanCodeJN.GenericApis.SmartMapper;

public class MapConfig<TFrom, TTo>
{
    public List<Action<TFrom, TTo>> CustomMappings { get; } = [];

    public MapConfig(IEnumerable<Action<TFrom, TTo>> customMappings) => CustomMappings.AddRange(customMappings);
}
