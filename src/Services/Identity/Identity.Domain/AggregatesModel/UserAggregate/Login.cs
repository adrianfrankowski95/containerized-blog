using Blog.Services.Identity.Domain.SeedWork;
using NodaTime;

namespace Blog.Services.Identity.Domain.AggregatesModel.UserAggregate;

public class Login : ValueObject<Login>
{
    public LoginAttempts FailedAttempts { get; private set; }
    public Instant? LastSuccessfulAt { get; private set; }

    public Login()
    {
        FailedAttempts = LoginAttempts.None();
    }

    private Login AddFailedAttempt()
        => new() { LastSuccessfulAt = LastSuccessfulAt, FailedAttempts = FailedAttempts.Increment() };
    private Login ClearFailedAttempts()
        => new() { LastSuccessfulAt = LastSuccessfulAt };
    public Login FailedAttempt(User user)
    {
        if (FailedAttempts == LoginAttempts.MaxAllowed)
        {
            user.LockOutUntil(SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromMinutes(5)));
            return ClearFailedAttempts();
        }

        return AddFailedAttempt();
    }
    public static Login SuccessfulAttempt() => new()
    {
        LastSuccessfulAt = SystemClock.Instance.GetCurrentInstant()
    };

    protected override IEnumerable<object> GetEqualityCheckAttributes()
    {
        yield return FailedAttempts;
        yield return LastSuccessfulAt;
    }
}