using CleanCodeJN.GenericApis.Abstractions.Responses;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Responses;

public class BaseResponseTests
{
    [Fact]
    public async Task Create_WithResultEnum_Success_ShouldSetCorrectProperties()
    {
        var data = new TestEntity { Id = 1 };

        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS, data: data, message: "ok");

        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(data, response.Data);
        Assert.Equal("ok", response.Message);
        Assert.Equal(1, response.Count);
    }

    [Fact]
    public async Task Create_WithResultEnum_Failure_ShouldSetCorrectProperties()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.FAILURE_NOT_FOUND, message: "not found");

        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_NOT_FOUND, response.ResultState);
        Assert.Null(response.Data);
        Assert.Equal("not found", response.Message);
    }

    [Fact]
    public async Task Create_WithBoolTrue_ShouldSetSuccess()
    {
        var data = new TestEntity { Id = 5 };

        var response = await BaseResponse<TestEntity>.Create(true, data: data);

        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(data, response.Data);
    }

    [Fact]
    public async Task Create_WithBoolFalse_ShouldSetFailure()
    {
        var response = await BaseResponse<TestEntity>.Create(false);

        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, response.ResultState);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task Create_WithNoData_ShouldHaveCountZero()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS);

        Assert.Equal(0, response.Count);
    }

    [Fact]
    public async Task Create_WithData_ShouldHaveCountOne()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS, data: new TestEntity { Id = 1 });

        Assert.Equal(1, response.Count);
    }

    [Fact]
    public async Task Create_WithInterruptTrue_ShouldSetInterruptFlag()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS, interrupt: true);

        Assert.True(response.Interrupt);
    }

    [Fact]
    public async Task Create_WithDelay_ShouldSetDelay()
    {
        var delay = TimeSpan.FromSeconds(30);

        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS, delay: delay);

        Assert.Equal(delay, response.Delay);
    }

    [Fact]
    public async Task Create_WithInfo_ShouldSetInfo()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS, info: "block1");

        Assert.Equal("block1", response.Info);
    }

    [Theory]
    [InlineData(ResultEnum.SUCCESS, true)]
    [InlineData(ResultEnum.SUCCESS_CREATED, true)]
    [InlineData(ResultEnum.SUCCESS_ACCEPTED, true)]
    [InlineData(ResultEnum.SUCCESS_NO_CONTENT, true)]
    [InlineData(ResultEnum.FAILURE_BAD_REQUEST, false)]
    [InlineData(ResultEnum.FAILURE_NOT_FOUND, false)]
    [InlineData(ResultEnum.FAILURE_UNAUTHORIZED, false)]
    [InlineData(ResultEnum.FAILURE_INTERNAL_SERVER_ERROR, false)]
    public async Task Succeeded_ShouldReflectResultState(ResultEnum state, bool expectedSucceeded)
    {
        var response = await BaseResponse<TestEntity>.Create(state);

        Assert.Equal(expectedSucceeded, response.Succeeded);
    }

    public class TestEntity
    {
        public int Id { get; set; }
    }
}
