using System.Text.Json;
using API.Models;
using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "An unhandled exception occurred.");

        var errorResponse = new ErrorResponse
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            Message = "Internal Server Error",
            Detail = exception.Message,
            TraceId = httpContext.TraceIdentifier
        };

        if (exception is BaseException baseException)
        {
            errorResponse.StatusCode = baseException.StatusCode;
            errorResponse.Message = baseException.Message;
        }
        else if (exception is UnauthorizedAccessException)
        {
            errorResponse.StatusCode = StatusCodes.Status401Unauthorized;
            errorResponse.Message = "Unauthorized access.";
        }
        else if (exception is KeyNotFoundException)
        {
            errorResponse.StatusCode = StatusCodes.Status404NotFound;
            errorResponse.Message = "Resource not found.";
        }
        else if (exception is JsonException)
        {
            errorResponse.StatusCode = StatusCodes.Status400BadRequest;
            errorResponse.Message = "Invalid JSON format.";
        }

        httpContext.Response.StatusCode = errorResponse.StatusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

        return true;
    }
}
