// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Episodes.Add;
using Nino.Core.Features.Project.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Episodes;

[Group("episode", "Manage episodes")]
public partial class EpisodesModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IDataService dataService,
    IStateService stateService,
    IInteractionIdentityService interactionIdService,
    IUserVerificationService verificationService,
    ResolveProjectHandler projectResolver,
    AddEpisodeHandler addHandler,
    ILogger<EpisodesModule> logger
) : InteractionModuleBase<IInteractionContext> { }
