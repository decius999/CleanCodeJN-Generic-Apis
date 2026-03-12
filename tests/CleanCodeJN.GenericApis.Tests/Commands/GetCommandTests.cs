using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

/// <summary>
/// Contains unit tests for <see cref="GetCommand{TEntity, TKey}"/>.
/// </summary>
public class GetCommandTests
{
    /// <summary>
    /// Verifies that the handler returns a success response with entities and a correct count.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntitiesAreFound()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var testEntities = new List<TestEntity>
        {
            new() { Id = 1 },
            new() { Id = 2 }
        };
        mockRepository
            .Setup(repo => repo.Query(
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<TestEntity, object>>[]>()))
            .Returns(testEntities.AsQueryable());

        var getCommand = new GetCommand<TestEntity, int>(mockRepository.Object);
        var request = new GetRequest<TestEntity, int>
        {
            Where = x => x.Id > 0,
            AsNoTracking = true,
            IgnoreQueryFilters = false,
            AsSplitQuery = false,
            Includes = null,
            Skip = 0,
            Take = 10,
            SortField = "Id",
            SortOrder = "asc",
            Filter = null
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await getCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(testEntities, response.Data);
        Assert.Equal(testEntities.Count, response.Count);
    }

    /// <summary>
    /// Verifies that the handler returns a successful response with an empty list when no entities exist.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoEntitiesAreFound()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        mockRepository
            .Setup(repo => repo.Query(
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<TestEntity, object>>[]>()))
            .Returns(Enumerable.Empty<TestEntity>().AsQueryable());

        var getCommand = new GetCommand<TestEntity, int>(mockRepository.Object);
        var request = new GetRequest<TestEntity, int>
        {
            Where = x => x.Id > 0,
            AsNoTracking = true,
            IgnoreQueryFilters = false,
            AsSplitQuery = false,
            Includes = null,
            Skip = 0,
            Take = 10,
            SortField = "Id",
            SortOrder = "asc",
            Filter = null
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await getCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Empty(response.Data);
        Assert.Equal(0, response.Count);
    }

    /// <summary>
    /// A simple test entity used as a stand-in for real domain entities within get command tests.
    /// </summary>
    public class TestEntity : IEntity<int>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the test entity.
        /// </summary>
        public int Id { get; set; }
    }
}
