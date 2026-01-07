using System.Diagnostics;
using MediatR;

namespace PhotosApi.Services.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
        )
    {
        var requestName = typeof(TRequest).Name;
        
        logger.LogInformation("Handling {RequestName} with payload: {@Request}", requestName, request);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            
            stopwatch.Stop();
            
            logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms with response: {@Response}",
                requestName, stopwatch.ElapsedMilliseconds, response);
            
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex,
                "Request {RequestName} failed after {ElapsedMilliseconds}ms with exception: {ExceptionMessage}",
                requestName, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}