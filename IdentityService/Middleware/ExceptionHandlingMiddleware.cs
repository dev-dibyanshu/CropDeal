using System.Net;
using System.Text.Json;

namespace IdentityService.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var message = GetMessage(exception, statusCode);

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "An unhandled exception occurred.");
        }
        else
        {
            _logger.LogWarning(exception, "A handled exception occurred: {Message}", message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            statusCode = statusCode,
            message = message
        };

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string GetMessage(Exception exception, int statusCode)
    {
        return statusCode switch
        {
            (int)HttpStatusCode.NotFound => exception.Message ?? "Resource not found.",
            (int)HttpStatusCode.Unauthorized => "Unauthorized.",
            (int)HttpStatusCode.BadRequest => exception.Message ?? "Invalid request.",
            _ => "An unexpected error occurred."
        };
    }
}
