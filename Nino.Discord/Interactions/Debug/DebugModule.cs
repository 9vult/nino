// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Queries.Projects.GetDebugData;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Debug;

[Group("debug", "Debug commands")]
public partial class DebugModule(
    IAniListService aniListService,
    IInteractionIdentityService interactionIdService,
    IUserVerificationService verificationService,
    GetProjectDebugDataHandler projectDataHandler,
    IBotPermissionsService botPermissionsService
) : InteractionModuleBase<IInteractionContext>;
