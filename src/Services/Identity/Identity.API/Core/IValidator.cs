namespace Blog.Services.Identity.API.Core;

public interface IValidator<TItem, TResult>
{
    public abstract ValueTask<TResult> ValidateAsync(TItem item);
}
