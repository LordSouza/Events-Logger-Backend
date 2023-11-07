using System.Net;

namespace EventsLogger.Entities.DbSet;

public class APIResponse
{

    public APIResponse()
    {
        Messages = new List<string>();
    }

    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; } = true;
    public List<string> Messages { get; set; }
    public object? Result { get; set; }
}
