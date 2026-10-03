using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
namespace TaskManager.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title, detail) = exception switch
            {
                UnauthorizedAccessException uae => (
                    context.User.Identity?.IsAuthenticated == true
                        ? HttpStatusCode.Forbidden
                        : HttpStatusCode.Unauthorized,
                    context.User.Identity?.IsAuthenticated == true
                        ? "Forbidden"
                        : "Unauthorized",
                    uae.Message
                ),

                KeyNotFoundException knf => (
                    HttpStatusCode.NotFound,
                    "Resource Not Found",
                    knf.Message
                ),

                ArgumentException ae => (
                    HttpStatusCode.BadRequest,
                    "Bad Request",
                    ae.Message
                ),

                InvalidOperationException ioe => (
                    HttpStatusCode.Conflict,
                    "Conflict",
                    ioe.Message
                ),

                _ => (
                    HttpStatusCode.InternalServerError,
                    "Internal Server Error",
                    _env.IsDevelopment()
                        ? exception.Message
                        : "An unexpected error occurred. Please try again later."
                )
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
            }
            else
            {
                _logger.LogWarning("Handled domain exception ({StatusCode} - {Title}): {Message}",
                    (int)statusCode, title, exception.Message);
            }

            var problemDetails = new ProblemDetails
            {
                Status = (int)statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            if (_env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
        }
    }
}
