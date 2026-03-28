// SPDX-License-Identifier: MPL-2.0

using Imposter.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;
using Nino.Core.Events;
using Nino.Core.Services;

namespace Nino.Core.Tests;

public abstract class TestBase : IAsyncDisposable
{
    protected TestDatabase Db { get; private set; } = null!;

    protected IEventBus EventBus { get; private set; } = null!;
    protected IEventBusImposter BusImposter { get; private set; } = null!;

    protected IIdentityService IdentityService { get; private set; } = null!;

    protected IUserVerificationService UserVerificationService { get; private set; } = null!;

    [Before(Test)]
    public async Task SetUpAsync()
    {
        Db = await TestDatabase.CreateAsync();

        BusImposter = IEventBus.Imposter();
        EventBus = BusImposter.Instance();

        IdentityService = new IdentityService(
            Db.Context,
            EventBus,
            NullLogger<IdentityService>.Instance
        );

        UserVerificationService = new UserVerificationService(
            Db.ReadOnlyContext,
            NullLogger<UserVerificationService>.Instance
        );
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Db is not null)
            await Db.DisposeAsync();
    }
}
