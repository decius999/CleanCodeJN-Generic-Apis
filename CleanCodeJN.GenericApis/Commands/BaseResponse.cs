namespace CleanCodeJN.GenericApis.Commands;
public class BaseResponse<T> : Response
    where T : class
{
    public T Data { get; }

    public BaseResponse(bool success, T data = default, string message = null, TimeSpan? delay = null, string info = null, bool interrupt = false)
        : base(success, message, delay, info, data != default ? 1 : 0, interrupt) => Data = data;

    public static Task<BaseResponse<T>> Create(bool success, T data = default, string message = null, TimeSpan? delay = null, string info = null,
        bool interrupt = false) => Task.FromResult(new BaseResponse<T>(success, data, message, delay, info, interrupt));
}

