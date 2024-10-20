namespace CleanCodeJN.GenericApis.Abstractions.Responses;
public class BaseListResponse<T> : Response
{
    public BaseListResponse()
    {
    }

    public BaseListResponse(ResultEnum resultState, List<T> data = default, string message = null, int? count = 0, string info = null, bool interrupt = false) : base(resultState, message, count: count, info: info, interrupt: interrupt) => Data = data;

    public List<T> Data { get; set; }

    public static Task<BaseListResponse<T>> Create(ResultEnum resultState, List<T> data = default, string message = null, int? count = 0, string info = null, bool interrupt = false) => Task.FromResult(new BaseListResponse<T>(resultState, data, message, count, info, interrupt));

    public static Task<BaseListResponse<T>> Create(bool success, List<T> data = default, string message = null, int? count = 0, string info = null, bool interrupt = false) => Task.FromResult(new BaseListResponse<T>(success ? ResultEnum.SUCCESS : ResultEnum.FAILURE_BAD_REQUEST, data, message, count, info, interrupt));
}
