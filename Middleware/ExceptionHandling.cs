using DocumentManagement.Exceptions;

namespace DocumentManagement.Middleware
{
    public class ExceptionHandling
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandling> _logger;

        public ExceptionHandling(RequestDelegate next, ILogger<ExceptionHandling> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DocumentTooLargeException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status413PayloadTooLarge, ex.Message);
            }
            catch (UnsupportedDocumentTypeException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status415UnsupportedMediaType, ex.Message);
            }
            catch (DocumentValidationException ex)
            {
                await HandleExceptionAsync(context, StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");

                await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, "An unexpected server error occurred.");
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message
            });
        }
    }
}