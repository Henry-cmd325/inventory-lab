using Inventory.Domain;
using Inventory.Domain.Stock;
using Microsoft.AspNetCore.Diagnostics;

namespace Inventory.Api.Infrastructure;

/// <summary>Turns broken business rules into ProblemDetails answers. Handlers never catch these exceptions.</summary>
internal sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            InsufficientStockException => (StatusCodes.Status409Conflict, "Insufficient stock"),
            ConcurrencyConflictException => (StatusCodes.Status409Conflict, "Concurrent update"),
            DomainException => (StatusCodes.Status400BadRequest, "Business rule broken"),
            _ => (0, string.Empty),
        };

        if (status == 0)
        {
            return false;
        }

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = { Status = status, Title = title, Detail = exception.Message },
        });
    }
}
