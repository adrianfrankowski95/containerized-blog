namespace Blog.Services.Blogging.Domain.Exceptions;

public class BloggingDomainException : Exception
{
    public BloggingDomainException(string message)
        : base(message)
    { }
}