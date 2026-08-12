using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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
        
        // FluentValidation errors
        if (exception is ValidationException fluentException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            
            var errors = fluentException.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var problemDetails = new HttpValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred",
                Detail = "See the errors property for details."
            };
            
            await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
            return true;
        }
        
        // other validation errors
        var statusCode = exception switch
        {
            KeyNotFoundException or FileNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException or ArgumentException => StatusCodes.Status400BadRequest,
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