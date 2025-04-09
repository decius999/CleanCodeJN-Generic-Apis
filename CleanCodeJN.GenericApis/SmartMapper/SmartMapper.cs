using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CleanCodeJN.GenericApis.SmartMapper;

public class SmartMapper : ISmartMapper
{
    private readonly Dictionary<(Type from, Type to), object> _configs = [];

    public void Register<TFrom, TTo>(MapConfig<TFrom, TTo> config)
    {
        _configs[(typeof(TFrom), typeof(TTo))] = config;
        _configs[(typeof(TTo), typeof(TFrom))] = new MapConfig<TTo, TFrom>([]); // leere Rückrichtung
    }

    public void Map<TFrom, TTo>(TFrom from, TTo to)
    {
        if (from == null || to == null)
        {
            return;
        }

        AutoMap(from, to, [], []);

        if (_configs.TryGetValue((typeof(TFrom), typeof(TTo)), out var configObj) &&
            configObj is MapConfig<TFrom, TTo> config)
        {
            foreach (var map in config.CustomMappings)
            {
                map(from, to);
            }
        }
    }

    public TTo Map<TFrom, TTo>(TFrom from) where TTo : new()
    {
        var to = new TTo();
        Map(from, to);
        return to;
    }

    private void AutoMap(object from, object to, HashSet<(int, int)> visited, Dictionary<object, object> mapped)
    {
        var key = (RuntimeHelpers.GetHashCode(from), RuntimeHelpers.GetHashCode(to));
        if (!visited.Add(key))
        {
            return;
        }

        if (mapped.TryGetValue(from, out var existing))
        {
            to = existing;
            return;
        }

        mapped[from] = to;

        var fromProps = from.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var toProps = to.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var fromProp in fromProps)
        {
            var toProp = toProps.FirstOrDefault(p => p.Name == fromProp.Name && p.CanWrite);
            if (toProp == null || !fromProp.CanRead)
            {
                continue;
            }

            var fromValue = fromProp.GetValue(from);
            if (fromValue == null)
            {
                continue;
            }

            var toValue = toProp.GetValue(to);

            if (toProp.PropertyType.IsAssignableFrom(fromProp.PropertyType))
            {
                toProp.SetValue(to, fromValue);
            }
            else if (IsEnumerable(fromProp.PropertyType, out var fromItemType) &&
                     IsEnumerable(toProp.PropertyType, out var toItemType))
            {
                var fromList = ((IEnumerable)fromValue).Cast<object>().ToList();
                var toList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(toItemType))!;

                foreach (var fromItem in fromList)
                {
                    if (!mapped.TryGetValue(fromItem, out var toItem))
                    {
                        toItem = Activator.CreateInstance(toItemType)!;
                        mapped[fromItem] = toItem;
                        AutoMap(fromItem, toItem, visited, mapped);
                    }

                    toList.Add(toItem);
                }

                toProp.SetValue(to, toList);
            }
            else
            {
                if (toValue == null)
                {
                    toValue = Activator.CreateInstance(toProp.PropertyType);
                    toProp.SetValue(to, toValue);
                }

                AutoMap(fromValue, toValue, visited, mapped);
            }
        }
    }

    private bool IsEnumerable(Type type, out Type itemType)
    {
        itemType = typeof(object);

        if (type == typeof(string))
        {
            return false;
        }

        if (type.IsArray)
        {
            itemType = type.GetElementType()!;
            return true;
        }

        if (type.IsGenericType)
        {
            var genericDef = type.GetGenericTypeDefinition();
            if (genericDef == typeof(IEnumerable<>) || genericDef == typeof(List<>))
            {
                itemType = type.GetGenericArguments()[0];
                return true;
            }
        }

        var ienum = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (ienum != null)
        {
            itemType = ienum.GetGenericArguments()[0];
            return true;
        }

        return false;
    }
}
