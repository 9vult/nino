// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Commands.KeyStaff.Add;
using Nino.Core.Features.Queries.Project.Resolve;
using Nino.Core.Features.Queries.Project.Status;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.KeyStaff;

[Group("keystaff", "Manage project Key Staff")]
public partial class KeyStaffModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    IStateService stateService,
    IUserVerificationService userVerificationService,
    ResolveProjectHandler projectResolver,
    ProjectStatusHandler projectStatusHandler,
    AddKeyStaffHandler addHandler,
    ILogger<KeyStaffModule> logger
) : InteractionModuleBase<IInteractionContext> { }
