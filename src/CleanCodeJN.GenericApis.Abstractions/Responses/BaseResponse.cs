namespace CleanCodeJN.GenericApis.Abstractions.Responses;
public class BaseResponse<T> : Response
    where T : class
{
    public BaseResponse()
    {
    }

    public T Data { get; set; }

    public BaseResponse(ResultEnum resultState, T data = default, string message = null, TimeSpan? delay = null, string info = null, bool interrupt = false)
        : base(resultState, message, delay, info, data != default ? 1 : 0, interrupt) => Data = data;

    public static Task<BaseResponse<T>> Create(ResultEnum resultState, T data = default, string message = null, TimeSpan? delay = null, string info = null,
        bool interrupt = false) => Task.FromResult(new BaseResponse<T>(resultState, data, message, delay, info, interrupt));

    public static Task<BaseResponse<T>> Create(bool success, T data = default, string message = null, TimeSpan? delay = null, string info = null,
      bool interrupt = false) => Task.FromResult(new BaseResponse<T>(success ? ResultEnum.SUCCESS : ResultEnum.FAILURE_BAD_REQUEST, data, message, delay, info, interrupt));
}

