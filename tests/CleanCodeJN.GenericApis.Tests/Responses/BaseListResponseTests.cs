using CleanCodeJN.GenericApis.Abstractions.Responses;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Responses;

/// <summary>
/// Contains unit tests for <see cref="BaseListResponse{T}"/> factory methods and property behaviour.
/// </summary>
public class BaseListResponseTests
{
    /// <summary>
    /// Verifies that creating a list response with <see cref="ResultEnum.SUCCESS"/> sets all properties correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that a list response created with a failure result enum is marked as not succeeded.
    /// </summary>
    [Fact]
    public async Task Create_WithResultEnum_Failure_ShouldSetCorrectProperties()
    {
        var response = await BaseListResponse<TestEntity>.Create(ResultEnum.FAILURE_NOT_FOUND, message: "not found");

        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_NOT_FOUND, response.ResultState);
        Assert.Null(response.Data);
        Assert.Equal("not found", response.Message);
    }

    /// <summary>
    /// Verifies that passing <c>true</c> to the bool overload results in a success list response.
    /// </summary>
    [Fact]
    public async Task Create_WithBoolTrue_ShouldSetSuccess()
    {
        var data = new List<TestEntity> { new() { Id = 1 } };

        var response = await BaseListResponse<TestEntity>.Create(true, data: data);

        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(data, response.Data);
    }

    /// <summary>
    /// Verifies that passing <c>false</c> to the bool overload results in a bad-request failure list response.
    /// </summary>
    [Fact]
    public async Task Create_WithBoolFalse_ShouldSetFailure()
    {
        var response = await BaseListResponse<TestEntity>.Create(false);

        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, response.ResultState);
        Assert.Null(response.Data);
    }

    /// <summary>
    /// Verifies that the interrupt flag is set when the list response is created with interrupt set to true.
    /// </summary>
    [Fact]
    public async Task Create_WithInterruptTrue_ShouldSetInterruptFlag()
    {
        var response = await BaseListResponse<TestEntity>.Create(ResultEnum.SUCCESS, interrupt: true);

        Assert.True(response.Interrupt);
    }

    /// <summary>
    /// Verifies that a success list response with an empty data collection is valid and contains no items.
    /// </summary>
    [Fact]
    public async Task Create_WithEmptyList_ShouldSucceedWithEmptyData()
    {
        var response = await BaseListResponse<TestEntity>.Create(ResultEnum.SUCCESS, data: []);

        Assert.True(response.Succeeded);
        Assert.Empty(response.Data);
    }

    /// <summary>
    /// Verifies that the info string is correctly stored on the list response when specified.
    /// </summary>
    [Fact]
    public async Task Create_WithInfo_ShouldSetInfo()
    {
        var response = await BaseListResponse<TestEntity>.Create(ResultEnum.SUCCESS, info: "someBlock");

        Assert.Equal("someBlock", response.Info);
    }

    /// <summary>
    /// Verifies that the <c>Succeeded</c> flag on a list response correctly reflects each possible <see cref="ResultEnum"/> value.
    /// </summary>
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

    /// <summary>
    /// A minimal test entity used as a generic type argument within base list response tests.
    /// </summary>
    public class TestEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier of the test entity.
        /// </summary>
        public int Id { get; set; }
    }
}
