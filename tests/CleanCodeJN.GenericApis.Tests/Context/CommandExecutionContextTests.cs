using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Context;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;
using Moq;
using Xunit;

namespace CleanCodeJN.GenericApis.Tests.Context;

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

    [Fact]
    public async Task Get_ShouldReturnNull_WhenBlockNameNotInCache()
    {
        var context = new CommandExecutionContext(new Mock<IMediator>().Object);

        var cached = context.Get<TestEntity>("nonExistentBlock");

        Assert.Null(cached);
    }

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

    [Fact]
    public async Task ExecuteList_ShouldReturnInterrupt_WhenResponseHasInterruptFlag()
    {
        var mockMediator = CreateMediatorMock(new BaseListResponse<TestEntity>(ResultEnum.SUCCESS, interrupt: true));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestListRequest());
        var result = await context.ExecuteList<TestEntity>(CancellationToken.None);

        Assert.True(result.Interrupt);
    }

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

    [Fact]
    public async Task ExecuteResponse_ShouldReturnFailure_WhenRequestFails()
    {
        var mockMediator = CreateMediatorMock(new Response(ResultEnum.FAILURE_BAD_REQUEST, message: "error"));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestResponseRequest(), continueOnCheckError: false);
        var result = await context.Execute(CancellationToken.None);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ExecuteResponse_ShouldReturnInterrupt_WhenResponseHasInterruptFlag()
    {
        var mockMediator = CreateMediatorMock(new Response(ResultEnum.SUCCESS, interrupt: true));
        var context = new CommandExecutionContext(mockMediator.Object);

        context.WithRequest(() => new TestResponseRequest());
        var result = await context.Execute(CancellationToken.None);

        Assert.True(result.Interrupt);
    }

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

    [Fact]
    public async Task GetParallelWhenAllByIndex_ShouldReturnNull_WhenBlockNameNotFound()
    {
        var context = new CommandExecutionContext(new Mock<IMediator>().Object);

        var item = context.GetParallelWhenAllByIndex<TestEntity>("nonExistentBlock", 0);

        Assert.Null(item);
    }

    // ── IfRequest / IfBreakRequest ──────────────────────────────────────────

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

    public class TestEntity : IEntity<int>
    {
        public int Id { get; set; }
    }

    public class TestRequest : IRequest<BaseResponse<TestEntity>> { }
    public class TestListRequest : IRequest<BaseListResponse<TestEntity>> { }
    public class TestResponseRequest : IRequest<Response> { }
}
