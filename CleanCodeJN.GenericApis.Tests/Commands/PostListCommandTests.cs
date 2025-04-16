using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using FluentValidation;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

public class PostListCommandTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntitiesAreCreated()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();
        var mockValidators = new List<IValidator<TestDto>>();

        var testDtos = new List<TestDto>
        {
            new TestDto { Name = "Entity 1" },
            new TestDto { Name = "Entity 2" }
        };

        var testEntities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Entity 1" },
            new TestEntity { Id = 2, Name = "Entity 2" }
        };

        mockMapper
            .Setup(mapper => mapper.Map<TestEntity>(It.IsAny<TestDto>()))
            .Returns((TestDto dto) => new TestEntity { Name = dto.Name });

        mockRepository
            .Setup(repo => repo.Create(It.IsAny<List<TestEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEntities);

        var postListCommand = new PostListCommand<TestEntity, TestDto, int>(mockMapper.Object, mockRepository.Object, mockValidators);
        var request = new PostListRequest<TestEntity, TestDto>
        {
            Dtos = testDtos,
            SkipValidation = false
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await postListCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(testEntities, response.Data);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();
        var mockValidator = new Mock<IValidator<TestDto>>();
        var mockValidators = new List<IValidator<TestDto>> { mockValidator.Object };

        var testDtos = new List<TestDto>
        {
            new TestDto { Name = "Valid Entity" },
            new TestDto { Name = "Invalid Entity" }
        };

        mockValidator
            .Setup(validator => validator.Validate(It.Is<TestDto>(dto => dto.Name == "Invalid Entity")))
            .Returns(new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Name", "Invalid name")
            }));

        mockValidator
            .Setup(validator => validator.Validate(It.Is<TestDto>(dto => dto.Name == "Valid Entity")))
            .Returns(new FluentValidation.Results.ValidationResult());

        var postListCommand = new PostListCommand<TestEntity, TestDto, int>(mockMapper.Object, mockRepository.Object, mockValidators);
        var request = new PostListRequest<TestEntity, TestDto>
        {
            Dtos = testDtos,
            SkipValidation = false
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await postListCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, response.ResultState);
        Assert.Null(response.Data);
        Assert.Contains("Invalid name", response.Message);
    }

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TestDto
    {
        public string Name { get; set; }
    }
}
