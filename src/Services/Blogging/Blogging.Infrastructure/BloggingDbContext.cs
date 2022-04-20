using System.Data;
using Blog.Services.Blogging.Domain.AggregatesModel.PostAggregate;
using Blog.Services.Blogging.Domain.AggregatesModel.TagAggregate;
using Blog.Services.Blogging.Infrastructure.EntityConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Blog.Services.Blogging.Infrastructure;

public class BloggingDbContext : DbContext
{
    public const string DefaultSchema = "blogging";
    public DbSet<PostBase> Posts { get; set; }
    public DbSet<Tag> Tags { get; set; }
    private IDbContextTransaction? _transaction;

    public BloggingDbContext(DbContextOptions<BloggingDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PostBaseEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PostTranslationBaseEntityConfiguration());

        modelBuilder.ApplyConfiguration(new RecipePostEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RecipePostTranslationEntityConfiguration());

        modelBuilder.ApplyConfiguration(new ReviewPostBaseEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewPostTranslationBaseEntityConfiguration());

        modelBuilder.ApplyConfiguration(new RestaurantReviewPostEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RestaurantReviewPostTranslationEntityConfiguration());

        modelBuilder.ApplyConfiguration(new ProductReviewPostEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ProductReviewPostTranslationEntityConfiguration());

        modelBuilder.ApplyConfiguration(new LifestylePostEntityConfiguration());
        modelBuilder.ApplyConfiguration(new LifestylePostTranslationEntityConfiguration());

        modelBuilder.ApplyConfiguration(new TagEntityConfiguration());

        modelBuilder.ApplyConfiguration(new IdentifiedRequestEntityConfiguration());
    }

    public bool HasActiveTransaction => _transaction is not null;

    public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            throw new InvalidOperationException($"Active transaction already exists");

        _transaction = await Database.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);

        return _transaction;
    }

    public async Task CommitTransactionAsync(IDbContextTransaction transaction, CancellationToken cancellationToken = default)
    {
        if (transaction is null)
            throw new ArgumentNullException(nameof(transaction));

        if (transaction != _transaction)
            throw new InvalidOperationException($"Provided transaction with ID {transaction.TransactionId} was different than existing one");

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
