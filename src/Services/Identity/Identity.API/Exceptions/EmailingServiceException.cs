namespace Blog.Services.Identity.Domain.Exceptions;

public class EmailingServiceException : Exception
{
    public EmailingServiceException(string message)
        : base(message)
    { }
}