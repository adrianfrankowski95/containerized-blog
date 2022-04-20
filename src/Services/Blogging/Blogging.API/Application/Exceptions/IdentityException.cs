namespace Blog.Services.Blogging.API.Application.Exceptions;

public class IdentityException : Exception
{
    public IdentityException(string message) : base(message) { }
}
