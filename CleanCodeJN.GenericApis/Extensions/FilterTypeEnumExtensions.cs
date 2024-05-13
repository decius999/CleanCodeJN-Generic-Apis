using CleanCodeJN.GenericApis.Models;

namespace CleanCodeJN.GenericApis.Extensions;

public static class FilterTypeEnumExtensions
{
    public static object ConvertTo(FilterValue value) => value.Type switch
    {
        FilterTypeEnum.STRING => value.Value?.ToString(),
        FilterTypeEnum.INTEGER => Convert.ToInt32(value.Value),
        FilterTypeEnum.INTEGER_NULLABLE => Convert.ToInt32(value.Value),
        FilterTypeEnum.DOUBLE => Convert.ToDouble(value.Value),
        FilterTypeEnum.DOUBLE_NULLABLE => Convert.ToDouble(value.Value),
        FilterTypeEnum.DATETIME => Convert.ToDateTime(value.Value),
        FilterTypeEnum.DATETIME_NULLABLE => Convert.ToDateTime(value.Value),
        _ => value.Value?.ToString(),
    };
}
