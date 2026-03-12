using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Context;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Context;

/// <summary>
/// Contains unit tests for <see cref="CommandExecutionContext"/>, covering sequential, parallel, conditional, and interrupt execution scenarios.
/// </summary>
public class CommandExecutionContextTests
{
    // IMediator.Send(object, CancellationToken) is what CommandExecutionContext calls internally,
    // because DynamicInvoke() returns object, causing the non-generic overload to be selected.
    private static Mock<IMediator> CreateMediatorMock(params object[] responsesInOrder)
    {
        var mock = new Mock<IMediator>();
        var sequence = mock.SetupSequence(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()));
        foreach (var response in responsesInOrder)
        {
            sequence.ReturnsAsync(response);
        }
        return mock;
    }

    // ── Execute<T> ──────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that a single successful request results in a success response with the entity data.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldReturnSuccess_WhenSingleRequestSucceeds()
    {
        var entity = new TestEntity { Id = 1 };
        var successResponse = new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity);
        var mockMediator = CreateMediatorMock(successResponse);
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestRequest());
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, result.ResultState);
        Assert.Equal(entity, result.Data);
    }

    /// <summary>
    /// Verifies that when multiple requests succeed, the data from the last request is returned.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldReturnDataFromLastRequest_WhenMultipleRequestsSucceed()
    {
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 2 };
        var mockMediator = CreateMediatorMock(
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity1),
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity2));
        var context = new CommandExecutionContext(mockMediator.Object);

        context
            .WithRequest(() => new TestRequest(), blockName: "step1")
            .WithRequest(() => new TestRequest(), blockName: "step2");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(entity2, result.Data);
    }

    /// <summary>
    /// Verifies that execution stops and returns failure when a request fails and <c>continueOnCheckError</c> is false.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldReturnFailure_WhenRequestFails_AndContinueOnCheckErrorIsFalse()
    {
        var failedResponse = new BaseResponse<TestEntity>(ResultEnum.FAILURE_NOT_FOUND, message: "not found");
        var mockMediator = CreateMediatorMock(failedResponse);
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest<TestEntity>(() => new TestRequest(), blockName: "step1", continueOnCheckError: false);
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_NOT_FOUND, result.ResultState);
    }

    /// <summary>
    /// Verifies that execution continues to the next step when a request fails and <c>continueOnCheckError</c> is true.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldContinueAndReturnSuccess_WhenRequestFails_AndContinueOnCheckErrorIsTrue()
    {
        var entity = new TestEntity { Id = 99 };
        var mockMediator = CreateMediatorMock(
            new BaseResponse<TestEntity>(ResultEnum.FAILURE_NOT_FOUND),
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity));
        var context = new CommandExecutionContext(mockMediator.Object);

        context
            .WithRequest<TestEntity>(() => new TestRequest(), blockName: "step1", continueOnCheckError: true)
            .WithRequest<TestEntity>(() => new TestRequest(), blockName: "step2");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(entity, result.Data);
    }

    /// <summary>
    /// Verifies that the request is not executed and a failure is returned when the pre-condition fails and <c>continueOnCheckError</c> is false.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldReturnFailure_WhenPreConditionFails_AndContinueOnCheckErrorIsFalse()
    {
        var mockMediator = new Mock<IMediator>();
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest<TestEntity>(
            () => new TestRequest(),
            blockName: "step1",
            checkBeforeExecution: () => false,
            continueOnCheckError: false);
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, result.ResultState);
        mockMediator.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that a step is skipped and execution continues when its pre-condition fails and <c>continueOnCheckError</c> is true.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldSkipStepAndContinue_WhenPreConditionFails_AndContinueOnCheckErrorIsTrue()
    {
        var entity = new TestEntity { Id = 42 };
        var mockMediator = CreateMediatorMock(new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity));
        var context = new CommandExecutionContext(mockMediator.Object);

        context
            .WithRequest<TestEntity>(() => new TestRequest(), blockName: "skip", checkBeforeExecution: () => false, continueOnCheckError: true)
            .WithRequest<TestEntity>(() => new TestRequest(), blockName: "run");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(entity, result.Data);
        mockMediator.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that execution stops and returns failure when the post-condition fails and <c>continueOnCheckError</c> is false.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldReturnFailure_WhenPostConditionFails_AndContinueOnCheckErrorIsFalse()
    {
        var entity = new TestEntity { Id = 1 };
        var mockMediator = CreateMediatorMock(new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest<TestEntity>(
            () => new TestRequest(),
            blockName: "step1",
            checkAfterExecution: _ => false,
            continueOnCheckError: false);
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, result.ResultState);
    }

    /// <summary>
    /// Verifies that execution continues to the next step when the post-condition fails and <c>continueOnCheckError</c> is true.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldContinue_WhenPostConditionFails_AndContinueOnCheckErrorIsTrue()
    {
        var entity2 = new TestEntity { Id = 2 };
        var mockMediator = CreateMediatorMock(
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: new TestEntity { Id = 1 }),
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity2));
        var context = new CommandExecutionContext(mockMediator.Object);

        context
            .WithRequest<TestEntity>(() => new TestRequest(), blockName: "b1", checkAfterExecution: _ => false, continueOnCheckError: true)
            .WithRequest<TestEntity>(() => new TestRequest(), blockName: "b2");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(entity2, result.Data);
    }

    /// <summary>
    /// Verifies that the interrupt flag is propagated on the result when a response carries it.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldReturnInterrupt_WhenResponseHasInterruptFlag()
    {
        var interruptResponse = new BaseResponse<TestEntity>(ResultEnum.SUCCESS, message: "interrupted", interrupt: true);
        var mockMediator = CreateMediatorMock(interruptResponse);
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestRequest(), blockName: "step1");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Interrupt);
    }

    /// <summary>
    /// Verifies that no subsequent requests are executed after a response with the interrupt flag is received.
    /// </summary>
    [Fact]
    public async Task Execute_ShouldStopProcessing_AfterInterrupt_AndNotCallNextRequest()
    {
        var interruptResponse = new BaseResponse<TestEntity>(ResultEnum.SUCCESS, interrupt: true);
        var mockMediator = CreateMediatorMock(interruptResponse);
        var context = new CommandExecutionContext(mockMediator.Object);

        context
            .WithRequest(() => new TestRequest(), blockName: "step1")
            .WithRequest(() => new TestRequest(), blockName: "step2");
        await context.Execute<TestEntity>(CancellationToken.None);

        mockMediator.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Cache (Get / GetList) ───────────────────────────────────────────────

    /// <summary>
    /// Verifies that data stored under a block name can be retrieved via <c>Get</c> after execution.
    /// </summary>
    [Fact]
    public async Task Get_ShouldReturnCachedData_AfterExecuteWithBlockName()
    {
        var entity = new TestEntity { Id = 99 };
        var mockMediator = CreateMediatorMock(new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestRequest(), blockName: "myBlock");
        await context.Execute<TestEntity>(CancellationToken.None);

        var cached = context.Get<TestEntity>("myBlock");

        Assert.Equal(entity, cached);
    }

    /// <summary>
    /// Verifies that <c>Get</c> returns null when the specified block name has no stored data.
    /// </summary>
    [Fact]
    public async Task Get_ShouldReturnNull_WhenBlockNameNotInCache()
    {
        var context = new CommandExecutionContext(new Mock<IMediator>().Object);

        var cached = context.Get<TestEntity>("nonExistentBlock");

        Assert.Null(cached);
    }

    /// <summary>
    /// Verifies that list data stored under a block name can be retrieved via <c>GetList</c> after list execution.
    /// </summary>
    [Fact]
    public async Task GetList_ShouldReturnCachedListData_AfterExecuteListWithBlockName()
    {
        var entities = new List<TestEntity> { new() { Id = 1 }, new() { Id = 2 } };
        var listResponse = new BaseListResponse<TestEntity>(ResultEnum.SUCCESS, data: entities);
        var mockMediator = CreateMediatorMock(listResponse);
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestListRequest(), blockName: "listBlock");
        await context.ExecuteList<TestEntity>(CancellationToken.None);

        var cached = context.GetList<TestEntity>("listBlock");

        Assert.Equal(entities, cached);
    }

    // ── ExecuteList<T> ──────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <c>ExecuteList</c> returns a success list response with correct data.
    /// </summary>
    [Fact]
    public async Task ExecuteList_ShouldReturnSuccess_WhenRequestSucceeds()
    {
        var entities = new List<TestEntity> { new() { Id = 1 }, new() { Id = 2 } };
        var mockMediator = CreateMediatorMock(new BaseListResponse<TestEntity>(ResultEnum.SUCCESS, data: entities, count: 2));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestListRequest());
        var result = await context.ExecuteList<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(entities, result.Data);
    }

    /// <summary>
    /// Verifies that <c>ExecuteList</c> returns a failure list response when the request fails.
    /// </summary>
    [Fact]
    public async Task ExecuteList_ShouldReturnFailure_WhenRequestFails()
    {
        var failedResponse = new BaseListResponse<TestEntity>(ResultEnum.FAILURE_NOT_FOUND, message: "not found");
        var mockMediator = CreateMediatorMock(failedResponse);
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest<TestEntity>(() => new TestListRequest(), continueOnCheckError: false);
        var result = await context.ExecuteList<TestEntity>(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_NOT_FOUND, result.ResultState);
    }

    /// <summary>
    /// Verifies that the interrupt flag is propagated on the list result when a list response carries it.
    /// </summary>
    [Fact]
    public async Task ExecuteList_ShouldReturnInterrupt_WhenResponseHasInterruptFlag()
    {
        var mockMediator = CreateMediatorMock(new BaseListResponse<TestEntity>(ResultEnum.SUCCESS, interrupt: true));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestListRequest());
        var result = await context.ExecuteList<TestEntity>(CancellationToken.None);

        Assert.True(result.Interrupt);
    }

    /// <summary>
    /// Verifies that <c>ExecuteList</c> returns a failure when the pre-condition for a step is not met.
    /// </summary>
    [Fact]
    public async Task ExecuteList_ShouldReturnFailure_WhenPreConditionFails()
    {
        var mockMediator = new Mock<IMediator>();
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest<TestEntity>(
            () => new TestListRequest(),
            blockName: "step1",
            checkBeforeExecution: () => false,
            continueOnCheckError: false);
        var result = await context.ExecuteList<TestEntity>(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, result.ResultState);
    }

    // ── Execute() → Response ────────────────────────────────────────────────

    /// <summary>
    /// Verifies that <c>Execute()</c> returns a success <see cref="Response"/> when the request succeeds.
    /// </summary>
    [Fact]
    public async Task ExecuteResponse_ShouldReturnSuccess_WhenRequestSucceeds()
    {
        var mockMediator = CreateMediatorMock(new Response(ResultEnum.SUCCESS));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestResponseRequest());
        var result = await context.Execute(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(ResultEnum.SUCCESS, result.ResultState);
    }

    /// <summary>
    /// Verifies that <c>Execute()</c> returns a failure <see cref="Response"/> when the request fails.
    /// </summary>
    [Fact]
    public async Task ExecuteResponse_ShouldReturnFailure_WhenRequestFails()
    {
        var mockMediator = CreateMediatorMock(new Response(ResultEnum.FAILURE_BAD_REQUEST, message: "error"));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestResponseRequest(), continueOnCheckError: false);
        var result = await context.Execute(CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    /// <summary>
    /// Verifies that the interrupt flag on a <see cref="Response"/> is correctly propagated by <c>Execute()</c>.
    /// </summary>
    [Fact]
    public async Task ExecuteResponse_ShouldReturnInterrupt_WhenResponseHasInterruptFlag()
    {
        var mockMediator = CreateMediatorMock(new Response(ResultEnum.SUCCESS, interrupt: true));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestResponseRequest());
        var result = await context.Execute(CancellationToken.None);

        Assert.True(result.Interrupt);
    }

    /// <summary>
    /// Verifies that <c>Execute()</c> returns a failure without calling the mediator when the pre-condition fails.
    /// </summary>
    [Fact]
    public async Task ExecuteResponse_ShouldReturnFailure_WhenPreConditionFails()
    {
        var mockMediator = new Mock<IMediator>();
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(
            () => new TestResponseRequest(),
            blockName: "step1",
            checkBeforeExecution: () => false,
            continueOnCheckError: false);
        var result = await context.Execute(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, result.ResultState);
        mockMediator.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── WithParallelWhenAllRequests ─────────────────────────────────────────

    /// <summary>
    /// Verifies that all parallel requests are executed and a success result is returned when all succeed.
    /// </summary>
    [Fact]
    public async Task Execute_WithParallelRequests_ShouldReturnSuccess_WhenAllSucceed()
    {
        var entity1 = new TestEntity { Id = 1 };
        var entity2 = new TestEntity { Id = 2 };
        var mockMediator = CreateMediatorMock(
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity1),
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity2));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithParallelWhenAllRequests(
            new List<Func<IRequest<Response>>> { () => new TestRequest(), () => new TestRequest() },
            blockName: "parallel");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        mockMediator.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that results from each parallel request can be retrieved by index after execution.
    /// </summary>
    [Fact]
    public async Task GetParallelWhenAllByIndex_ShouldReturnNonNullItems_AfterParallelExecution()
    {
        var entity1 = new TestEntity { Id = 10 };
        var entity2 = new TestEntity { Id = 20 };
        var mockMediator = CreateMediatorMock(
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity1),
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity2));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithParallelWhenAllRequests(
            new List<Func<IRequest<Response>>> { () => new TestRequest(), () => new TestRequest() },
            blockName: "parallel");
        await context.Execute<TestEntity>(CancellationToken.None);

        var item0 = context.GetParallelWhenAllByIndex<TestEntity>("parallel", 0);
        var item1 = context.GetParallelWhenAllByIndex<TestEntity>("parallel", 1);

        Assert.NotNull(item0);
        Assert.NotNull(item1);
    }

    /// <summary>
    /// Verifies that <c>GetParallelWhenAllByIndex</c> returns null when the requested index exceeds the number of results.
    /// </summary>
    [Fact]
    public async Task GetParallelWhenAllByIndex_ShouldReturnNull_WhenIndexOutOfRange()
    {
        var mockMediator = CreateMediatorMock(new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: new TestEntity { Id = 1 }));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithParallelWhenAllRequests(new List<Func<IRequest<Response>>> { () => new TestRequest() }, blockName: "parallel");
        await context.Execute<TestEntity>(CancellationToken.None);

        var item = context.GetParallelWhenAllByIndex<TestEntity>("parallel", 99);

        Assert.Null(item);
    }

    /// <summary>
    /// Verifies that the overall result is a failure when at least one parallel request fails.
    /// </summary>
    [Fact]
    public async Task Execute_WithParallelRequests_ShouldReturnFailure_WhenOneRequestFails()
    {
        var mockMediator = CreateMediatorMock(
            new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: new TestEntity { Id = 1 }),
            new BaseResponse<TestEntity>(ResultEnum.FAILURE_NOT_FOUND, message: "not found"));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithParallelWhenAllRequests(
            new List<Func<IRequest<Response>>> { () => new TestRequest(), () => new TestRequest() },
            blockName: "parallel");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, result.ResultState);
    }

    /// <summary>
    /// Verifies that <c>GetParallelWhenAllByIndex</c> returns null when the specified block name does not exist.
    /// </summary>
    [Fact]
    public async Task GetParallelWhenAllByIndex_ShouldReturnNull_WhenBlockNameNotFound()
    {
        var context = new CommandExecutionContext(new Mock<IMediator>().Object);

        var item = context.GetParallelWhenAllByIndex<TestEntity>("nonExistentBlock", 0);

        Assert.Null(item);
    }

    // ── IfRequest / IfBreakRequest ──────────────────────────────────────────

    /// <summary>
    /// Verifies that an <c>IfRequest</c> step is skipped and execution continues when its before-predicate returns false.
    /// </summary>
    [Fact]
    public async Task IfRequest_ShouldSkipRequest_WhenBeforePredicateFails_AndContinue()
    {
        var entity = new TestEntity { Id = 1 };
        var mockMediator = CreateMediatorMock(new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity));
        ICommandExecutionContext context = new CommandExecutionContext(mockMediator.Object);

        context
            .IfRequest<TestEntity>(() => new TestRequest(), ifBeforePredicate: () => false, blockName: "skip")
            .WithRequest(() => new TestRequest(), blockName: "run");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        mockMediator.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that an <c>IfRequest</c> step is executed when its before-predicate returns true.
    /// </summary>
    [Fact]
    public async Task IfRequest_ShouldExecuteRequest_WhenBeforePredicateIsTrue()
    {
        var entity = new TestEntity { Id = 5 };
        var mockMediator = CreateMediatorMock(new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity));
        ICommandExecutionContext context = new CommandExecutionContext(mockMediator.Object);

        context.IfRequest<TestEntity>(() => new TestRequest(), ifBeforePredicate: () => true, blockName: "run");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        mockMediator.Verify(m => m.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that <c>IfBreakRequest</c> stops execution and returns a failure when the after-predicate returns false.
    /// </summary>
    [Fact]
    public async Task IfBreakRequest_ShouldBreakExecution_WhenAfterPredicateFails()
    {
        var entity = new TestEntity { Id = 1 };
        var mockMediator = CreateMediatorMock(new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity));
        ICommandExecutionContext context = new CommandExecutionContext(mockMediator.Object);

        context.IfBreakRequest<TestEntity>(
            () => new TestRequest(),
            ifAfterPredicate: _ => false,
            blockName: "break");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(ResultEnum.FAILURE_BAD_REQUEST, result.ResultState);
    }

    /// <summary>
    /// Verifies that <c>IfBreakRequest</c> allows execution to continue and returns success when the after-predicate returns true.
    /// </summary>
    [Fact]
    public async Task IfBreakRequest_ShouldContinue_WhenAfterPredicateIsTrue()
    {
        var entity = new TestEntity { Id = 7 };
        var mockMediator = CreateMediatorMock(new BaseResponse<TestEntity>(ResultEnum.SUCCESS, data: entity));
        ICommandExecutionContext context = new CommandExecutionContext(mockMediator.Object);

        context.IfBreakRequest<TestEntity>(
            () => new TestRequest(),
            ifAfterPredicate: _ => true,
            blockName: "pass");
        var result = await context.Execute<TestEntity>(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(entity, result.Data);
    }

    // ── Test types ──────────────────────────────────────────────────────────

    /// <summary>
    /// A simple test entity used as a stand-in for real domain entities within command execution context tests.
    /// </summary>
    public class TestEntity : IEntity<int>
    {
        /// <summary>
        /// Gets or sets the unique identifier of the test entity.
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// A test MediatR request that returns a single-entity base response.
    /// </summary>
    public class TestRequest : IRequest<BaseResponse<TestEntity>> { }

    /// <summary>
    /// A test MediatR request that returns a list base response.
    /// </summary>
    public class TestListRequest : IRequest<BaseListResponse<TestEntity>> { }

    /// <summary>
    /// A test MediatR request that returns a plain <see cref="Response"/>.
    /// </summary>
    public class TestResponseRequest : IRequest<Response> { }
}
