using System.Data;
using Blog.Services.Identity.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Blogging.API.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
    private readonly IdentityDbContext _identityDbContext;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger, IdentityDbContext identityDbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _identityDbContext = identityDbContext ?? throw new ArgumentNullException(nameof(identityDbContext));
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = default!;
        string requestType = request.GetType().Name;

        try
        {
            if (!_identityDbContext.HasActiveTransaction)
                return await next();

            var strategy = _identityDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _identityDbContext.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

                _logger.LogInformation("----- Beginning transaction {TransactionId} for a {RequestType} ({Request})",
                    transaction.TransactionId, requestType, request);

                response = await next();

                _logger.LogInformation("----- Committing transaction {TransactionId} for a {RequestType}",
                    transaction.TransactionId, requestType);

                await _identityDbContext.CommitTransactionAsync(transaction, cancellationToken);
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "----- Error while processing a request {RequestType} ({Request})", requestType, request);
            throw;
        }
    }
}