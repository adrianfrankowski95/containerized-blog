using Blog.Services.Blogging.Infrastructure.Idempotency;
using MediatR;

namespace Blog.Services.Blogging.API.Application.Commands;

public class IdentifiedCommandHandler<TRequest> : IRequestHandler<IdentifiedCommand<TRequest>, Unit>
    where TRequest : IRequest<Unit>
{
    private readonly IRequestManager _requestManager;
    private readonly IMediator _mediator;
    private readonly ILogger<IdentifiedCommandHandler<TRequest>> _logger;

    public IdentifiedCommandHandler(IRequestManager requestManager, IMediator mediator, ILogger<IdentifiedCommandHandler<TRequest>> logger)
    {
        _requestManager = requestManager ?? throw new ArgumentNullException(nameof(requestManager));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<Unit> Handle(IdentifiedCommand<TRequest> request, CancellationToken cancellationToken)
    {
        bool exists = await _requestManager.ExistsAsync<TRequest>(request.Id).ConfigureAwait(false);

        if (exists)
        {
            _logger.LogInformation("----- Duplicated command {CommandType} detected at {UtcNow} - ({@Command})",
                request.Command.GetType(), DateTime.UtcNow, request.Command);

            return Unit.Value;
        }

        await _requestManager.AddRequestAsync<TRequest>(request.Id).ConfigureAwait(false);

        _logger.LogInformation("----- Sending command {CommandType} at {UtcNow} - ({@Command})",
            request.Command.GetType(), DateTime.UtcNow, request.Command);

        var result = await _mediator.Send(request.Command, cancellationToken);

        _logger.LogInformation("----- Command result of {CommandType}: {CommandResult} - ({@Command})",
            request.Command.GetType(), result, request.Command);

        return result;
    }
}