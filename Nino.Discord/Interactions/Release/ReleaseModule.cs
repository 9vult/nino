// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Commands.Projects.Release.Batch;
using Nino.Core.Features.Commands.Projects.Release.Episode;
using Nino.Core.Features.Commands.Projects.Release.Volume;
using Nino.Core.Features.Queries.Episodes.ValidateRelease;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Release;

[Group("release", "Release to the world!")]
public partial class ReleaseModule(
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    IStateService stateService,
    GetGenericProjectDataHandler getProjectDataHandler,
    ResolveProjectHandler projectResolver,
    ReleaseEpisodeHandler releaseEpisodeHandler,
    ReleaseVolumeHandler releaseVolumeHandler,
    ReleaseBatchHandler releaseBatchHandler,
    ValidateReleaseHandler validateReleaseHandler,
    ILogger<ReleaseModule> logger
) : InteractionModuleBase<IInteractionContext>;
