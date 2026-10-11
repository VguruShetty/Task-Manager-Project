using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common.Exceptions;

namespace TaskManager.API.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            _logger.LogError(
                exception,
                "An unhandled exception occurred while processing request on {Path}. TraceId: {TraceId}",
                httpContext.Request.Path,
                traceId);

            var (statusCode, title, detail) = MapException(exception);

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path,
                Type = $"https://httpstatuses.io/{statusCode}"
            };

            problemDetails.Extensions["traceId"] = traceId;

            // Include stack trace only in local Development environment
            if (_environment.IsDevelopment() && statusCode == StatusCodes.Status500InternalServerError)
            {
                problemDetails.Extensions["exceptionDetails"] = exception.ToString();
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // Confirms the exception was handled
        }

        private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
        {
            return exception switch
            {
                NotFoundException notFound => (
                    StatusCodes.Status404NotFound,
                    "Resource Not Found",
                    notFound.Message
                ),
                ConflictException conflict => (
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    conflict.Message
                ),
                UnauthorizedAccessException unauthorized => (
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    unauthorized.Message
                ),
                ForbiddenAccessException forbidden => (
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    forbidden.Message
                ),
                ArgumentException argEx => (
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    argEx.Message
                ),
                InvalidOperationException invalidOp => (
                    StatusCodes.Status400BadRequest,
                    "Invalid Operation",
                    invalidOp.Message
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred. Please contact support or try again later."
                )
            };
        }
    }
}
