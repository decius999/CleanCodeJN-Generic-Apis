using CleanCodeJN.GenericApis.Abstractions.Models;
using CleanCodeJN.GenericApis.Extensions;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Extensions;

public class FilterTypeEnumExtensionsTests
{
    [Fact]
    public void ConvertTo_ShouldReturnString_WhenTypeIsString()
    {
        var value = new FilterValue { Field = "Name", Value = "hello", Type = FilterTypeEnum.STRING };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Equal("hello", result);
        Assert.IsType<string>(result);
    }

    [Fact]
    public void ConvertTo_ShouldReturnNull_WhenValueIsNullAndTypeIsString()
    {
        var value = new FilterValue { Field = "Name", Value = null, Type = FilterTypeEnum.STRING };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Null(result);
    }

    [Fact]
    public void ConvertTo_ShouldReturnInt_WhenTypeIsInteger()
    {
        var value = new FilterValue { Field = "Id", Value = "42", Type = FilterTypeEnum.INTEGER };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Equal(42, result);
        Assert.IsType<int>(result);
    }

    [Fact]
    public void ConvertTo_ShouldReturnInt_WhenTypeIsIntegerNullable()
    {
        var value = new FilterValue { Field = "Id", Value = "7", Type = FilterTypeEnum.INTEGER_NULLABLE };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Equal(7, result);
        Assert.IsType<int>(result);
    }

    [Fact]
    public void ConvertTo_ShouldReturnDouble_WhenTypeIsDouble()
    {
        var value = new FilterValue { Field = "Amount", Value = "9.99", Type = FilterTypeEnum.DOUBLE };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Equal(Convert.ToDouble("9.99"), result);
        Assert.IsType<double>(result);
    }

    [Fact]
    public void ConvertTo_ShouldReturnDouble_WhenTypeIsDoubleNullable()
    {
        var value = new FilterValue { Field = "Amount", Value = "2.5", Type = FilterTypeEnum.DOUBLE_NULLABLE };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Equal(Convert.ToDouble("2.5"), result);
        Assert.IsType<double>(result);
    }

    [Fact]
    public void ConvertTo_ShouldReturnDateTime_WhenTypeIsDateTime()
    {
        var dateStr = "2024-01-15";
        var value = new FilterValue { Field = "CreatedAt", Value = dateStr, Type = FilterTypeEnum.DATETIME };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Equal(Convert.ToDateTime(dateStr), result);
        Assert.IsType<DateTime>(result);
    }

    [Fact]
    public void ConvertTo_ShouldReturnDateTime_WhenTypeIsDateTimeNullable()
    {
        var dateStr = "2024-06-20";
        var value = new FilterValue { Field = "UpdatedAt", Value = dateStr, Type = FilterTypeEnum.DATETIME_NULLABLE };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Equal(Convert.ToDateTime(dateStr), result);
        Assert.IsType<DateTime>(result);
    }

    [Fact]
    public void ConvertTo_ShouldReturnString_WhenTypeIsGuid()
    {
        var guidStr = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
        var value = new FilterValue { Field = "ExternalId", Value = guidStr, Type = FilterTypeEnum.GUID };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Equal(guidStr, result);
        Assert.IsType<string>(result);
    }

    [Fact]
    public void ConvertTo_ShouldReturnString_WhenTypeIsGuidNullable()
    {
        var guidStr = "b2c3d4e5-f6a7-8901-bcde-f12345678901";
        var value = new FilterValue { Field = "ExternalId", Value = guidStr, Type = FilterTypeEnum.GUID_NULLABLE };

        var result = FilterTypeEnumExtensions.ConvertTo(value);

        Assert.Equal(guidStr, result);
        Assert.IsType<string>(result);
    }
}
