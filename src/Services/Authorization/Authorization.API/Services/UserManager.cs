using Blog.Services.Authorization.API.Models;
;
using Microsoft.Extensions.Options;

namespace Blog.Services.Authorization.API.Services;

public class UserManager : UserManager<User>
{
    public UserManager(IUserStore<User> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<User> passwordHasher,
        IEnumerable<IUserValidator<User>> userValidators,
        IEnumerable<IPasswordValidator<User>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<User>> logger)
        : base(store,
            optionsAccessor,
            passwordHasher,
            userValidators,
            passwordValidators,
            keyNormalizer,
            errors,
            services,
            logger)
    { }

    public override async Task<IdentityResult> AddToRoleAsync(User user, string role)
    {

        ThrowIfDisposed();
        var userRoleStore = GetUserRoleStore();
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var normalizedRole = NormalizeKey(role);
        if (await userRoleStore.IsInRoleAsync(user, normalizedRole, CancellationToken))
        {
            return await UserAlreadyInRoleError(user, role);
        }
        await userRoleStore.AddToRoleAsync(user, normalizedRole, CancellationToken);
        return await UpdateUserAsync(user);
    }

    private IUserRoleStore<User> GetUserRoleStore()
    {
        if (Store is not IUserRoleStore<User> cast)
        {
            throw new NotSupportedException(Resources.StoreNotIUserRoleStore);
        }
        return cast;
    }

    public virtual string NormalizeKey(string key)
    {
        return (KeyNormalizer == null) ? key : KeyNormalizer.Normalize(key);
    }
}