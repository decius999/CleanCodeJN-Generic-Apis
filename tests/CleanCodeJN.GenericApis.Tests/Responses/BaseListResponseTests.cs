using CleanCodeJN.GenericApis.Abstractions.Responses;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Responses;

public class BaseListResponseTests
{
    [Fact]
    public async Task Create_WithResultEnum_Success_ShouldSetCorrectProperties()
    {
        var data = new List<TestEntity> { new() { Id = 1 }, new() { Id = 2 } };

        var response = await BaseListResponse<TestEntity>.Create(ResultEnum.SUCCESS, data: data, count: 2);

        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(data, response.Data);
        Assert.Equal(2, response.Count);
    }

    [Fact]
    public async Task Create_WithResultEnum_Failure_ShouldSetCorrectProperties()
    {
        var response = await BaseListResponse<TestEntity>.Create(ResultEnum.FAILURE_NOT_FOUND, message: "not found");

        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_NOT_FOUND, response.ResultState);
        Assert.Null(response.Data);
        Assert.Equal("not found", response.Message);
    }

    [Fact]
    public async Task Create_WithBoolTrue_ShouldSetSuccess()
    {
        var data = new List<TestEntity> { new() { Id = 1 } };

        var response = await BaseListResponse<TestEntity>.Create(true, data: data);

        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(data, response.Data);
    }

    [Fact]
    public async Task Create_WithBoolFalse_ShouldSetFailure()
    {
        var response = await BaseListResponse<TestEntity>.Create(false);

        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, response.ResultState);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task Create_WithInterruptTrue_ShouldSetInterruptFlag()
    {
        var response = await BaseListResponse<TestEntity>.Create(ResultEnum.SUCCESS, interrupt: true);

        Assert.True(response.Interrupt);
    }

    [Fact]
    public async Task Create_WithEmptyList_ShouldSucceedWithEmptyData()
    {
        var response = await BaseListResponse<TestEntity>.Create(ResultEnum.SUCCESS, data: []);

        Assert.True(response.Succeeded);
        Assert.Empty(response.Data);
    }

    [Fact]
    public async Task Create_WithInfo_ShouldSetInfo()
    {
        var response = await BaseListResponse<TestEntity>.Create(ResultEnum.SUCCESS, info: "someBlock");

        Assert.Equal("someBlock", response.Info);
    }

    [Theory]
    [InlineData(ResultEnum.SUCCESS, true)]
    [InlineData(ResultEnum.SUCCESS_CREATED, true)]
    [InlineData(ResultEnum.FAILURE_BAD_REQUEST, false)]
    [InlineData(ResultEnum.FAILURE_NOT_FOUND, false)]
    [InlineData(ResultEnum.FAILURE_INTERNAL_SERVER_ERROR, false)]
    public async Task Succeeded_ShouldReflectResultState(ResultEnum state, bool expectedSucceeded)
    {
        var response = await BaseListResponse<TestEntity>.Create(state);

        Assert.Equal(expectedSucceeded, response.Succeeded);
    }

    public class TestEntity
    {
        public int Id { get; set; }
    }
}
