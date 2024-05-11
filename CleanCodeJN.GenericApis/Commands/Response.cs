namespace CleanCodeJN.GenericApis.Commands;
public class Response
{
    public Response(ResultEnum resultState, string message = null, TimeSpan? delay = null, string info = null, int? count = 0, bool? interrupt = false)
    {
        ResultState = resultState;
        Message = message;
        Delay = delay;
        Info = info;
        Count = count.Value;
        Interrupt = interrupt.Value;
    }

    public ResultEnum ResultState { get; }

    public string Message { get; }

    public TimeSpan? Delay { get; }

    public string Info { get; }

    public int Count { get; set; }

    public bool Interrupt { get; set; }
}
