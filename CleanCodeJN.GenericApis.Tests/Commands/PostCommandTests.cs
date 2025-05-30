﻿using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using FluentValidation;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Commands;

public class PostCommandTests
{
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenEntityIsCreated()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<TestEntity, int>>();
        var mockMapper = new Mock<IMapper>();
        var mockValidators = new List<IValidator<TestDto>>();

        var testDto = new TestDto { Name = "Test Name" };
        var testEntity = new TestEntity { Id = 1, Name = "Test Name" };

        mockMapper
            .Setup(mapper => mapper.Map<TestEntity>(testDto))
            .Returns(testEntity);

        mockRepository
            .Setup(repo => repo.Create(It.IsAny<TestEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(testEntity);

        var postCommand = new PostCommand<TestEntity, TestDto, int>(mockMapper.Object, mockRepository.Object, mockValidators);
        var request = new PostRequest<TestEntity, TestDto>
        {
            Dto = testDto,
            SkipValidation = false
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await postCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, response.ResultState);
        Assert.Equal(testEntity, response.Data);
    }

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

        var postCommand = new PostCommand<TestEntity, TestDto, int>(mockMapper.Object, mockRepository.Object, mockValidators);
        var request = new PostRequest<TestEntity, TestDto>
        {
            Dto = testDto,
            SkipValidation = false
        };
        var cancellationToken = CancellationToken.None;

        // Act
        var response = await postCommand.Handle(request, cancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, response.ResultState);
        Assert.Null(response.Data);
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
