// SPDX-License-Identifier: MPL-2.0

using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Done;
using Nino.Core.Features.Episodes.Roster;
using Nino.Core.Features.Project.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IDataService dataService,
    IStateService stateService,
    IInteractionIdentityService interactionIdService,
    ResolveProjectHandler projectResolver,
    DoneHandler doneHandler,
    EpisodeRosterHandler rosterHandler,
    ILogger<DoneModule> logger
) : InteractionModuleBase<SocketInteractionContext> { }
