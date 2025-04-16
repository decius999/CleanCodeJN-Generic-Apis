using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

public class GetByIdCommandTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntityIsFound()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var testEntity = new TestEntity { Id = 1 };
        mockRepository
            .Setup(repo => repo.Query(
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<Expression<Func<TestEntity, object>>[]>()))
            .Returns(new[] { testEntity }.AsQueryable());

        var getByIdCommand = new GetByIdCommand<TestEntity, int>(mockRepository.Object);
        var request = new GetByIdRequest<TestEntity, int>
        {
            Id = 1,
            Where = x => x.Id == 1,
            AsNoTracking = true,
            IgnoreQueryFilters = false,
            AsSplitQuery = false,
            Includes = null
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await getByIdCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(testEntity, response.Data);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenEntityIsNotFound()
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

        var getByIdCommand = new GetByIdCommand<TestEntity, int>(mockRepository.Object);
        var request = new GetByIdRequest<TestEntity, int>
        {
            Id = 1,
            Where = x => x.Id == 1,
            AsNoTracking = true,
            IgnoreQueryFilters = false,
            AsSplitQuery = false,
            Includes = null
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await getByIdCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_NOT_FOUND, response.ResultState);
        Assert.Null(response.Data);
        Assert.Equal("Id '1' not found!", response.Message);
    }

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
    }
}
