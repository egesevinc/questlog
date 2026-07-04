using System.Text.Json;
using Questlog.Application.Common;

namespace Questlog.Api.Middleware;

/// <summary>
/// Translates AppException into clean JSON problem responses with the right
/// status code, and turns anything unexpected into a 500 without leaking
/// internals. Keeps controllers free of try/catch boilerplate.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await WriteProblem(context, ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblem(context, 500, "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblem(HttpContext context, int status, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new { status, detail });
        await context.Response.WriteAsync(payload);
    }
}
