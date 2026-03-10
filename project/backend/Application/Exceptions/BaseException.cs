namespace Application.Exceptions;

public class BaseException : Exception
{
    public int StatusCode { get; }

    public BaseException(string message, int statusCode = 500) : base(message)
    {
        StatusCode = statusCode;
    }
}
