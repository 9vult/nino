// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Project.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.KeyStaff;

[Group("keystaff", "Manage project Key Staff")]
public partial class KeyStaffModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IDataService dataService,
    IInteractionIdentityService interactionIdService,
    IUserVerificationService verificationService,
    ResolveProjectHandler projectResolver,
    ILogger<KeyStaffModule> logger
) : InteractionModuleBase<IInteractionContext> { }
