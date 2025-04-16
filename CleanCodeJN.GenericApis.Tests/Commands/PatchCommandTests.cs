using System.Text;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

public class PatchCommandTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPatchDocumentIsApplied()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var testEntity = new TestEntity { Id = 1, Name = "Old Name" };
        mockRepository
            .Setup(repo => repo.Query())
            .Returns(new[] { testEntity }.AsQueryable());
        mockRepository
            .Setup(repo => repo.Update(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEntity);

        var patchDocument = new JsonPatchDocument<TestEntity>();
        patchDocument.Replace(e => e.Name, "New Name");

        var patchCommand = new PatchCommand<TestEntity, int>(mockRepository.Object);
        var request = new PatchRequest<TestEntity, int>
        {
            Id = 1,
            PatchDocument = patchDocument,
            HttpContext = null
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await patchCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal("New Name", response.Data.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenPatchDocumentIsReadFromHttpContext()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var testEntity = new TestEntity { Id = 1, Name = "Old Name" };
        mockRepository
            .Setup(repo => repo.Query())
            .Returns(new[] { testEntity }.AsQueryable());
        mockRepository
            .Setup(repo => repo.Update(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEntity);

        var patchJson = "[{\"op\": \"replace\", \"path\": \"/Name\", \"value\": \"New Name\"}]";
        var httpContext = new DefaultHttpContext
        {
            Request =
            {
                Body = new MemoryStream(Encoding.UTF8.GetBytes(patchJson))
            }
        };

        var patchCommand = new PatchCommand<TestEntity, int>(mockRepository.Object);
        var request = new PatchRequest<TestEntity, int>
        {
            Id = 1,
            PatchDocument = null,
            HttpContext = httpContext
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await patchCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal("New Name", response.Data.Name);
    }

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
