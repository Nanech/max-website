using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;

namespace PhotosApi.Infrastructure.Errors;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception,
        CancellationToken ct
    )
    {
        logger.LogError(
            exception,
            "An unhandled exception occurred during {Method} request to {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path
            );

        var statusCode = exception switch
        {
            KeyNotFoundException or FileNotFoundException => StatusCodes.Status404NotFound,
            ValidationException or InvalidOperationException or ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = new
        {
            error = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexcepted error occurred on the server"
                : exception.Message
        };
        
        await httpContext.Response.WriteAsJsonAsync(response, ct);

        return true;
    }
}