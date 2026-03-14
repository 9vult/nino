// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Commands.AdditionalStaff.Add;
using Nino.Core.Features.Commands.AdditionalStaff.Rename;
using Nino.Core.Features.Queries.Episode.Resolve;
using Nino.Core.Features.Queries.Project.Resolve;
using Nino.Core.Features.Queries.Staff.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.AdditionalStaff;

[Group("additionalstaff", "Manage episode Additional Staff")]
public partial class AdditionalStaffModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    IStateService stateService,
    IUserVerificationService userVerificationService,
    ResolveProjectHandler projectResolver,
    ResolveEpisodeHandler episodeResolver,
    ResolveStaffHandler staffResolver,
    AddAdditionalStaffHandler addHandler,
    RenameAdditionalStaffHandler renameHandler,
    ILogger<AdditionalStaffModule> logger
) : InteractionModuleBase<IInteractionContext> { }
