namespace IsakChat.App.Services;

public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
}
