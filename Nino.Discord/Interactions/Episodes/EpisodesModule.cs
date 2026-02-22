// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Episodes.Add;
using Nino.Core.Features.Episodes.Remove;
using Nino.Core.Features.Episodes.Roster;
using Nino.Core.Features.Project.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Episodes;

[Group("episode", "Manage episodes")]
public partial class EpisodesModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IDataService dataService,
    IInteractionIdentityService interactionIdService,
    ResolveProjectHandler projectResolver,
    AddEpisodeHandler addHandler,
    RemoveEpisodeHandler removeHandler,
    EpisodeRosterHandler rosterHandler,
    ILogger<EpisodesModule> logger
) : InteractionModuleBase<IInteractionContext> { }
