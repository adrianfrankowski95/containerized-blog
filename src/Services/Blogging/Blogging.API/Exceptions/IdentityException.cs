namespace Blog.Services.Blogging.API.Exceptions;

public class IdentityException : Exception
{
    public IdentityException(string message) : base(message) { }
}
