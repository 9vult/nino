// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.AdditionalStaff.Add;
using Nino.Core.Features.AdditionalStaff.Remove;
using Nino.Core.Features.Project.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.AdditionalStaff;

[Group("additionalstaff", "Manage episode Additional Staff")]
public partial class AdditionalStaffModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IDataService dataService,
    IInteractionIdentityService interactionIdService,
    ResolveProjectHandler projectResolver,
    AddAdditionalStaffHandler addHandler,
    RemoveAdditionalStaffHandler removeHandler,
    ILogger<AdditionalStaffModule> logger
) : InteractionModuleBase<IInteractionContext> { }
