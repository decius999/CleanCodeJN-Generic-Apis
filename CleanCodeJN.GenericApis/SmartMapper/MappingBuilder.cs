namespace CleanCodeJN.GenericApis.SmartMapper;

public class MappingBuilder(ISmartMapper mapper)
{
    public void Define<TFrom, TTo>(Func<MapBuilder<TFrom, TTo>, MapBuilder<TFrom, TTo>> definition)
    {
        var builder = definition(MapBuilder<TFrom, TTo>.For(mapper));
        builder.Register();
    }
}
