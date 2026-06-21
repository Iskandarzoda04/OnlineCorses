using System.Net;
using System.Text.Json;
using Application.Common;

namespace WebApi.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var result = Result.Failure("An unexpected error occurred.", ErrorType.Failure);
            await context.Response.WriteAsync(JsonSerializer.Serialize(result));
        }
    }
}
