using System.Data;
using Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;
using Blog.Services.Identity.Domain.SeedWork;
using Blog.Services.Identity.Infrastructure.EntityConfigurations;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;

namespace Blog.Services.Identity.Infrastructure;

public class IdentityDbContext : DbContext
{
    public const string DefaultSchema = "identity";
    private readonly IMediator _mediator;
    private IDbContextTransaction? _transaction;
    public DbSet<User> Users { get; set; }

    public bool HasActiveTransaction => _transaction is not null;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.IgnoreAny<IReadOnlyList<DomainEvent>>();
        configurationBuilder.Properties<NonEmptyString>().HaveConversion<string>();
        configurationBuilder.Properties<NonNegativeInt>().HaveConversion<int>();
        configurationBuilder.Properties<NonPastInstant>().HaveConversion<Instant>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new AvatarEntityConfiguration());
        modelBuilder.ApplyConfiguration(new IdentifiedRequestEntityConfiguration());

        // Mass transit EF Core outbox configurations
        modelBuilder.AddInboxStateEntity(x => x.ToTable("inbox_state", "outbox"));
        modelBuilder.AddOutboxMessageEntity(x => x.ToTable("message", "outbox"));
        modelBuilder.AddOutboxStateEntity(x => x.ToTable("outbox_state", "outbox"));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _mediator.DispatchDomainEventsAsync(this).ConfigureAwait(false);
        return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public override int SaveChanges()
    {
        _mediator.DispatchDomainEventsAsync(this).GetAwaiter().GetResult();
        return base.SaveChanges();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException($"Active transaction already exists.");

        _transaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

        return _transaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction));

        if (transaction != _transaction)
            throw new InvalidOperationException($"Provided transaction with ID {transaction.TransactionId} was different than existing one with ID {_transaction?.TransactionId}.");

        await _transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction is not null)
                await _transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
