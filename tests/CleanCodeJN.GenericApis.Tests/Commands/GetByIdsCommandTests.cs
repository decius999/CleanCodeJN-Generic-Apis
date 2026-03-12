using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

/// <summary>
/// Contains unit tests for <see cref="GetByIdsCommand{TEntity, TKey}"/>.
/// </summary>
public class GetByIdsCommandTests
{
    /// <summary>
    /// Verifies that the handler returns a success response containing all matched entities.
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

        var getByIdsCommand = new GetByIdsCommand<TestEntity, int>(mockRepository.Object);
        var request = new GetByIdsRequest<TestEntity, int>
        {
            Ids = [1, 2],
            AsNoTracking = true,
            IgnoreQueryFilters = false,
            AsSplitQuery = false,
            Includes = null
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await getByIdsCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(testEntities, response.Data);
    }

    /// <summary>
    /// Verifies that the handler returns a bad-request failure when no entities match the provided IDs.
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

        var getByIdsCommand = new GetByIdsCommand<TestEntity, int>(mockRepository.Object);
        var request = new GetByIdsRequest<TestEntity, int>
        {
            Ids = [1, 2],
            AsNoTracking = true,
            IgnoreQueryFilters = false,
            AsSplitQuery = false,
            Includes = null
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await getByIdsCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, response.ResultState);
        Assert.Empty(response.Data);
    }

    /// <summary>
    /// A simple test entity used as a stand-in for real domain entities within get-by-ids command tests.
    /// </summary>
    public class TestEntity : IEntity<int>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the test entity.
        /// </summary>
        public int Id { get; set; }
    }
}
