namespace Blog.Services.Identity.Domain.SeedWork;
public abstract class Entity<T>
{
    public T Id { get; protected set; }

}