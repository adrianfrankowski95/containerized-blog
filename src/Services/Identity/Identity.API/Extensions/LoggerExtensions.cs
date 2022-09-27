using MediatR;

namespace Blog.Services.Identity.API.Extensions;

public static class LoggerExtensions
{
    private static readonly Action<ILogger, string, DateTime, string?, Exception?> _sendingCommand
        = LoggerMessage.Define<string, DateTime, string?>(
            LogLevel.Information,
            new EventId(1, "Sending command"),
            "----- Sending command {CommandType} at {UtcNow} - ({Command})");

    private static readonly Action<ILogger, string, Exception?> _handlingRequest
        = LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(2, "Handling request"),
            "----- Handling request: {RequestName}");

    private static readonly Action<ILogger, string, string?, Exception?> _receivedRequestResponse
        = LoggerMessage.Define<string, string?>(
            LogLevel.Information,
            new EventId(3, "Received request response"),
            "----- Received response for request {RequestName}: {Response}");

    public static void LogSendingCommand<TCommand>(this ILogger logger, IRequest<TCommand> command)
        => _sendingCommand(logger, command.GetType().Name, DateTime.UtcNow, command.ToString(), null);

    public static void LogHandlingRequest<TRequest>(this ILogger logger, IRequest<TRequest> request)
        => _handlingRequest(logger, request.GetType().Name, null);

    public static void LogReceivedRequestResponse<TRequest, TResponse>(this ILogger logger, IRequest<TRequest> request, TResponse response)
        => _receivedRequestResponse(logger, request.GetType().Name, response?.ToString(), null);
}