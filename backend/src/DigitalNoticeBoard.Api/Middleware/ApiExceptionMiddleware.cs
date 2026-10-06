using Microsoft.AspNetCore.Mvc;

namespace DigitalNoticeBoard.Api.Middleware;

public sealed class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException exception)
        {
            await WriteProblemAsync(context, 400, "Validation failed", exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            await WriteProblemAsync(context, 404, "Resource not found", exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled API error occurred.");
            await WriteProblemAsync(
                context,
                500,
                "Unexpected server error",
                "The request could not be completed.");
        }
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        });
    }
}
