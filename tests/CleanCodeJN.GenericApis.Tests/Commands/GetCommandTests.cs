using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

public class GetCommandTests
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

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
    }
}
