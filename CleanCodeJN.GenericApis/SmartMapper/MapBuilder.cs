using System.Linq.Expressions;
using System.Reflection;

namespace CleanCodeJN.GenericApis.SmartMapper;

public class MapBuilder<TFrom, TTo>
{
    private readonly List<Action<TFrom, TTo>> _forwardMappings = [];
    private readonly List<Action<TTo, TFrom>> _reverseMappings = [];
    private readonly ISmartMapper _mapper;

    private MapBuilder(ISmartMapper mapper = null) => _mapper = mapper;

    public static MapBuilder<TFrom, TTo> For(ISmartMapper mapper = null) => new(mapper);

    public MapBuilder<TFrom, TTo> Map<TPropFrom, TPropTo>(
        Expression<Func<TFrom, TPropFrom>> fromSelector,
        Expression<Func<TTo, TPropTo>> toSelector)
    {
        var fromGetter = fromSelector.Compile();
        var toSetter = BuildSetter(toSelector);

        _forwardMappings.Add((from, to) =>
        {
            var value = fromGetter(from);
            toSetter(to, value);
        });

        var toGetter = toSelector.Compile();
        var fromSetter = BuildSetter(fromSelector);

        _reverseMappings.Add((to, from) =>
        {
            var value = toGetter(to);
            fromSetter(from, value);
        });

        return this;
    }

    public (MapConfig<TFrom, TTo> forward, MapConfig<TTo, TFrom> reverse) Build() => (
            new MapConfig<TFrom, TTo>(_forwardMappings),
            new MapConfig<TTo, TFrom>(_reverseMappings)
        );

    public void Register()
    {
        if (_mapper == null)
        {
            throw new InvalidOperationException("Cannot register without a mapper instance.");
        }

        var (forward, reverse) = Build();
        _mapper.Register(forward);
        _mapper.Register(reverse);
    }

    private static Action<TObject, object> BuildSetter<TObject, TProperty>(Expression<Func<TObject, TProperty>> expression) => expression.Body is MemberExpression memberExpr && memberExpr.Member is PropertyInfo propInfo
            ? ((obj, value) => propInfo.SetValue(obj, value))
            : throw new InvalidOperationException("Only simple property expressions are supported.");
}
