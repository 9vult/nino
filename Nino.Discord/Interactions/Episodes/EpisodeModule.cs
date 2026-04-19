// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Commands.Episodes.Add;
using Nino.Core.Features.Commands.Episodes.Remove;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Episodes;

[Group("episode", "Episode management")]
public partial class EpisodesModule(
    IInteractionIdentityService interactionIdService,
    GetGenericProjectDataHandler getProjectDataHandler,
    ResolveProjectHandler projectResolver,
    ResolveEpisodeHandler episodeResolver,
    AddEpisodeHandler addHandler,
    RemoveEpisodeHandler removeHandler
) : InteractionModuleBase<IInteractionContext> { }
