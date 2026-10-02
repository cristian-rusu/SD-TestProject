using Company.Webshop.Shared.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Company.Webshop.Main.Modules.WebApi;

public static class WebApiModule
{
    public static IServiceCollection AddWebApiModule(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        int statusCode = exception switch
        {
            BusinessRuleException => StatusCodes.Status400BadRequest,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            return false;
        }

        await Results.Problem(exception.Message, statusCode: statusCode).ExecuteAsync(httpContext);
        return true;
    }
}
