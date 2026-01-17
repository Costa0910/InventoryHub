using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ServerApp.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex, logger);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger logger)
    {
        logger.LogError(ex, "Unhandled exception occurred while processing request");

        context.Response.ContentType = "application/problem+json";

        ProblemDetails problem;
        switch (ex)
        {
            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                var validation = new ValidationProblemDetails { Title = argEx.Message };
                problem = validation;
                break;
            case InvalidOperationException invOp:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                problem = new ProblemDetails { Title = invOp.Message };
                break;
            case KeyNotFoundException notFound:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                problem = new ProblemDetails { Title = notFound.Message };
                break;
            case UnauthorizedAccessException unauth:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                problem = new ProblemDetails { Title = unauth.Message };
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                problem = new ProblemDetails { Title = "An unexpected error occurred." };
                break;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsJsonAsync(problem, options);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}