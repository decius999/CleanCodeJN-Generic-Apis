using CleanCodeJN.GenericApis.Abstractions.Responses;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Responses;

/// <summary>
/// Contains unit tests for <see cref="BaseResponse{T}"/> factory methods and property behaviour.
/// </summary>
public class BaseResponseTests
{
    /// <summary>
    /// Verifies that creating a response with <see cref="ResultEnum.SUCCESS"/> sets all properties correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that creating a response with a failure result enum marks the response as not succeeded.
    /// </summary>
    [Fact]
    public async Task Create_WithResultEnum_Failure_ShouldSetCorrectProperties()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.FAILURE_NOT_FOUND, message: "not found");

        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_NOT_FOUND, response.ResultState);
        Assert.Null(response.Data);
        Assert.Equal("not found", response.Message);
    }

    /// <summary>
    /// Verifies that passing <c>true</c> to the bool overload results in a success response.
    /// </summary>
    [Fact]
    public async Task Create_WithBoolTrue_ShouldSetSuccess()
    {
        var data = new TestEntity { Id = 5 };

        var response = await BaseResponse<TestEntity>.Create(true, data: data);

        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(data, response.Data);
    }

    /// <summary>
    /// Verifies that passing <c>false</c> to the bool overload results in a bad-request failure response.
    /// </summary>
    [Fact]
    public async Task Create_WithBoolFalse_ShouldSetFailure()
    {
        var response = await BaseResponse<TestEntity>.Create(false);

        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, response.ResultState);
        Assert.Null(response.Data);
    }

    /// <summary>
    /// Verifies that a response created without data reports a count of zero.
    /// </summary>
    [Fact]
    public async Task Create_WithNoData_ShouldHaveCountZero()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS);

        Assert.Equal(0, response.Count);
    }

    /// <summary>
    /// Verifies that a response created with a single data object reports a count of one.
    /// </summary>
    [Fact]
    public async Task Create_WithData_ShouldHaveCountOne()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS, data: new TestEntity { Id = 1 });

        Assert.Equal(1, response.Count);
    }

    /// <summary>
    /// Verifies that the interrupt flag is set when the response is created with interrupt set to true.
    /// </summary>
    [Fact]
    public async Task Create_WithInterruptTrue_ShouldSetInterruptFlag()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS, interrupt: true);

        Assert.True(response.Interrupt);
    }

    /// <summary>
    /// Verifies that the delay property is correctly populated when specified during response creation.
    /// </summary>
    [Fact]
    public async Task Create_WithDelay_ShouldSetDelay()
    {
        var delay = TimeSpan.FromSeconds(30);

        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS, delay: delay);

        Assert.Equal(delay, response.Delay);
    }

    /// <summary>
    /// Verifies that the info string is correctly stored on the response when specified.
    /// </summary>
    [Fact]
    public async Task Create_WithInfo_ShouldSetInfo()
    {
        var response = await BaseResponse<TestEntity>.Create(ResultEnum.SUCCESS, info: "block1");

        Assert.Equal("block1", response.Info);
    }

    /// <summary>
    /// Verifies that the <c>Succeeded</c> flag correctly reflects each possible <see cref="ResultEnum"/> value.
    /// </summary>
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

    /// <summary>
    /// A minimal test entity used as a generic type argument within base response tests.
    /// </summary>
    public class TestEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier of the test entity.
        /// </summary>
        public int Id { get; set; }
    }
}
