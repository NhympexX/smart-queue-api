using Microsoft.AspNetCore.Diagnostics;

namespace SmartQueueApi.Handlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(new
            {
                statusCode = StatusCodes.Status500InternalServerError,
                code = "Internal Server Exception",
                message = "Error_Occured"
            }, cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
    }
}
