using Serilog.Context;

namespace DossieImobiliario.Middlewares;

public class TraceIdMiddleware
{
    public const string ItemKey = "traceId";

    private readonly RequestDelegate _next;
    private readonly ILogger<TraceIdMiddleware> _logger;

    public TraceIdMiddleware(RequestDelegate next, ILogger<TraceIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Guid.NewGuid().ToString("N");

        context.Items[ItemKey] = traceId;

        using (LogContext.PushProperty(ItemKey, traceId))
        {
            _logger.LogInformation("TraceId definido: {traceId}", traceId);
            await _next(context);
        }
    }
}
