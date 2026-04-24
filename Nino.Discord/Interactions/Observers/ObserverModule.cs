// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Commands.Observers.Add;
using Nino.Core.Features.Commands.Observers.Remove;
using Nino.Core.Features.Commands.Projects.DelegateObserver.Remove;
using Nino.Core.Features.Commands.Projects.DelegateObserver.Set;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Observers;

[Group("observer", "Manage observers")]
public partial class ObserverModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    GetGenericProjectDataHandler getProjectDataHandler,
    IBotPermissionsService botPermissionsService,
    ResolveProjectHandler projectResolver,
    AddObserverHandler addHandler,
    RemoveObserverHandler removeHandler
) : InteractionModuleBase<IInteractionContext>
{
    [Group("delegate", "Manage delegate observers")]
    public partial class DelegateModule(
        DiscordSocketClient client,
        IIdentityService identityService,
        IInteractionIdentityService interactionIdService,
        GetGenericProjectDataHandler getProjectDataHandler,
        ResolveProjectHandler projectResolver,
        SetDelegateObserverHandler setHandler,
        RemoveDelegateObserverHandler removeHandler
    ) : InteractionModuleBase<IInteractionContext>;
}
