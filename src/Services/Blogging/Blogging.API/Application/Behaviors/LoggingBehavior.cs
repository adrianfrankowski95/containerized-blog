using Blog.Services.Blogging.API.Extensions;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogHandlingRequest(request);

        var response = await next();

        _logger.LogReceivedRequestResponse(request, response);

        return response;
    }
}