namespace CleanCodeJN.GenericApis.Abstractions.Responses;
public class Response
{
    public Response()
    {
    }

    public Response(ResultEnum resultState, string message = null, TimeSpan? delay = null, string info = null, int? count = 0, bool? interrupt = false)
    {
        ResultState = resultState;
        Message = message;
        Delay = delay;
        Info = info;
        Count = count.Value;
        Interrupt = interrupt.Value;
    }

    public bool Succeeded => (int)ResultState < 300;

    public ResultEnum ResultState { get; set; }

    public string Message { get; set; }

    public TimeSpan? Delay { get; set; }

    public string Info { get; set; }

    public int Count { get; set; }

    public bool Interrupt { get; set; }
}
