
namespace DossieImobiliario.Middlewares;
public class TimeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimeMiddleware> _logger;

    public TimeMiddleware(RequestDelegate next, ILogger<TimeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();

            _logger.LogInformation("Request {Method} {Path} finalizada em {ElapsedMs}ms (Status {StatusCode}) - {traceId}",
                context.Request.Method,
                context.Request.Path.Value,
                sw.ElapsedMilliseconds,
                context.Response.StatusCode,
                context.Items[TraceIdMiddleware.ItemKey]?.ToString() ?? string.Empty);
        }
    }
}