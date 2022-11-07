namespace Blog.Services.Blogging.Infrastructure.Idempotency;

public class IdempotencyException : Exception
{
    public IdempotencyException(string message)
        : base(message)
    { }
}