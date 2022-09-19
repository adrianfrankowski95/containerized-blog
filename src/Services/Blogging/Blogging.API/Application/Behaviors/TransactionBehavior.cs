using System.Data;
using Blog.Services.Blogging.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Blog.Services.Blogging.API.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;
    private readonly BloggingDbContext _bloggingDbContext;

    public TransactionBehavior(ILogger<TransactionBehavior<TRequest, TResponse>> logger, BloggingDbContext bloggingDbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bloggingDbContext = bloggingDbContext ?? throw new ArgumentNullException(nameof(bloggingDbContext));
    }
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        TResponse response = default!;
        string requestType = request.GetType().Name;

        try
        {
            if (!_bloggingDbContext.HasActiveTransaction)
                return await next();

            var strategy = _bloggingDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _bloggingDbContext.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

                _logger.LogInformation("----- Beginning transaction {TransactionId} for a {RequestType} ({Request})",
                    transaction.TransactionId, requestType, request);

                response = await next();

                _logger.LogInformation("----- Committing transaction {TransactionId} for a {RequestType}",
                    transaction.TransactionId, requestType);

                await _bloggingDbContext.CommitTransactionAsync(transaction, cancellationToken);
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