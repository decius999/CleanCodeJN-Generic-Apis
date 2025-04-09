namespace CleanCodeJN.GenericApis.SmartMapper;

public interface ISmartMapper
{
    void Register<TFrom, TTo>(MapConfig<TFrom, TTo> config);

    void Map<TFrom, TTo>(TFrom from, TTo to);

    TTo Map<TFrom, TTo>(TFrom from) where TTo : new();
}
