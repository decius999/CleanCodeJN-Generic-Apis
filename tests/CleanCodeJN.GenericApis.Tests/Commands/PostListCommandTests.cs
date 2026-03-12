using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using FluentValidation;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

/// <summary>
/// Contains unit tests for <see cref="PostListCommand{TEntity, TDto, TKey}"/>.
/// </summary>
public class PostListCommandTests
{
    /// <summary>
    /// Verifies that the handler maps each DTO, creates the entities in bulk, and returns a success response.
    /// </summary>
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

    /// <summary>
    /// Verifies that the handler returns a bad-request failure response when any DTO fails validation.
    /// </summary>
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

    /// <summary>
    /// A simple test entity used as a stand-in for real domain entities within post-list command tests.
    /// </summary>
    public class TestEntity : IEntity<int>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the test entity.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the test entity.
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// A simple DTO used to supply data for bulk-creating test entities.
    /// </summary>
    public class TestDto
    {
        /// <summary>
        /// Gets or sets the name value supplied in the DTO.
        /// </summary>
        public string Name { get; set; }
    }
}
