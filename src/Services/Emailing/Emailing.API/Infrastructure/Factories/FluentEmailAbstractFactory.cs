using FluentEmail.Core;

namespace Blog.Services.Emailing.API.Infrastructure.Factories;

public abstract class FluentEmailAbstractFactory
{
    protected abstract IFluentEmail CreateEmail();
}