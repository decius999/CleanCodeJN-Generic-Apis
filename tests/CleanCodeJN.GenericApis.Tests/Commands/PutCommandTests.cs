using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using FluentValidation;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

/// <summary>
/// Contains unit tests for <see cref="PutCommand{TEntity, TDto, TKey}"/>.
/// </summary>
public class PutCommandTests
{
    /// <summary>
    /// Verifies that the handler maps the DTO, updates the entity, and returns a success response.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntityIsUpdated()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();
        var mockValidators = new List<IValidator<TestDto>>();

        var testDto = new TestDto { Name = "Updated Name" };
        var testEntity = new TestEntity { Id = 1, Name = "Updated Name" };

        mockMapper
            .Setup(mapper => mapper.Map<TestEntity>(testDto))
            .Returns(testEntity);

        mockRepository
            .Setup(repo => repo.Update(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEntity);

        var putCommand = new PutCommand<TestEntity, TestDto, int>(mockMapper.Object, mockRepository.Object, mockValidators);
        var request = new PutRequest<TestEntity, TestDto>
        {
            Dto = testDto,
            SkipValidation = false
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await putCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(testEntity, response.Data);
    }

    /// <summary>
    /// Verifies that the handler returns a bad-request failure response when validation fails.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();
        var mockValidator = new Mock<IValidator<TestDto>>();
        var mockValidators = new List<IValidator<TestDto>> { mockValidator.Object };

        var testDto = new TestDto { Name = "Invalid Name" };

        mockValidator
            .Setup(validator => validator.Validate(It.IsAny<TestDto>()))
            .Returns(new FluentValidation.Results.ValidationResult(new[]
            {
                new FluentValidation.Results.ValidationFailure("Name", "Invalid name")
            }));

        var putCommand = new PutCommand<TestEntity, TestDto, int>(mockMapper.Object, mockRepository.Object, mockValidators);
        var request = new PutRequest<TestEntity, TestDto>
        {
            Dto = testDto,
            SkipValidation = false
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await putCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, response.ResultState);
        Assert.Null(response.Data);
    }

    /// <summary>
    /// A simple test entity used as a stand-in for real domain entities within put command tests.
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
    /// A simple DTO used to supply data for updating a test entity.
    /// </summary>
    public class TestDto
    {
        /// <summary>
        /// Gets or sets the name value supplied in the DTO.
        /// </summary>
        public string Name { get; set; }
    }
}
