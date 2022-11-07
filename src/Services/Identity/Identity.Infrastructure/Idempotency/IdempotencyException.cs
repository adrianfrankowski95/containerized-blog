namespace Blog.Services.Identity.Infrastructure.Idempotency;

public class IdempotencyException : Exception
{
    public IdempotencyException(string message)
        : base(message)
    { }
}