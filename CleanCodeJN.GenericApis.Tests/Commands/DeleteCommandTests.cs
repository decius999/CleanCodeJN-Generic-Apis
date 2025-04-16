using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

public class DeleteCommandTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntityIsDeleted()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var testEntity = new TestEntity { Id = 1 };
        mockRepository
            .Setup(repo => repo.Delete(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEntity);

        var deleteCommand = new DeleteCommand<TestEntity, int>(mockRepository.Object);
        var request = new DeleteRequest<TestEntity, int> { Id = 1 };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await deleteCommand.Handle(request, cancellationToken);

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
            .Setup(repo => repo.Delete(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity)null);

        var deleteCommand = new DeleteCommand<TestEntity, int>(mockRepository.Object);
        var request = new DeleteRequest<TestEntity, int> { Id = 1 };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await deleteCommand.Handle(request, cancellationToken);

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
