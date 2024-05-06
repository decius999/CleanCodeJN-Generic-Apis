namespace CleanCodeJN.GenericApis.Commands;
public class BaseListResponse<T> : Response
{
    public BaseListResponse(bool success, List<T> data = default, string message = null, int? count = 0, string info = null, bool interrupt = false) : base(success, message, count: count, info: info, interrupt: interrupt) => Data = data;

    public List<T> Data { get; }

    public static Task<BaseListResponse<T>> Create(bool success, List<T> data = default, string message = null, int? count = 0, string info = null, bool interrupt = false) => Task.FromResult(new BaseListResponse<T>(success, data, message, count, info, interrupt));
}
