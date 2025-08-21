using System.Diagnostics;

namespace SmartAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString("N")[..8];
            
            context.Items["RequestId"] = requestId;

            _logger.LogInformation(
                "[{RequestId}] {Method} {Path} started from {RemoteIpAddress}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress
            );

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "[{RequestId}] Request failed with exception", 
                    requestId);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    "[{RequestId}] {Method} {Path} completed with {StatusCode} in {ElapsedMilliseconds}ms",
                    requestId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );
            }
        }
    }
}