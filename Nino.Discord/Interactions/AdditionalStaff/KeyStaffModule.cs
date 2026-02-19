// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Actions.Project.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.AdditionalStaff;

[Group("additionalstaff", "Manage episode Additional Staff")]
public partial class AdditionalStaffModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IDataService dataService,
    IInteractionIdentityService interactionIdService,
    IUserVerificationService verificationService,
    ProjectResolveHandler projectResolver,
    ILogger<AdditionalStaffModule> logger
) : InteractionModuleBase<IInteractionContext> { }
