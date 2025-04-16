using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

public class GetByIdsCommandTests
{
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

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
    }
}
