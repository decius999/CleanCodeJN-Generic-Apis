namespace CleanCodeJN.GenericApis.Commands;
public class Response
{
    public Response(bool success, string message = null, TimeSpan? delay = null, string info = null, int? count = 0, bool? interrupt = false)
    {
        Success = success;
        Message = message;
        Delay = delay;
        Info = info;
        Count = count.Value;
        Interrupt = interrupt.Value;
    }

    public bool Success { get; }

    public string Message { get; }

    public TimeSpan? Delay { get; }

    public string Info { get; }

    public int Count { get; set; }

    public bool Interrupt { get; set; }
}
