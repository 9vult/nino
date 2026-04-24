// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Commands.Tasks.MarkDone;
using Nino.Core.Features.Queries.Episodes.GetProgressResponseData;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Features.Queries.Tasks.GetTaskInfo;
using Nino.Core.Features.Queries.Tasks.GetWorkingTaskEpisode;
using Nino.Core.Features.Queries.Tasks.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Done;

public partial class DoneModule(
    IInteractionIdentityService interactionIdService,
    IIdentityService identityService,
    IStateService stateService,
    IAniListService aniListService,
    GetGenericProjectDataHandler getProjectDataHandler,
    GetWorkingTaskEpisodeHandler getWorkingTaskEpisodeHandler,
    GetProgressResponseDataHandler getProgressResponseDataHandler,
    GetTaskInfoHandler getTaskInfoHandler,
    ResolveProjectHandler projectResolver,
    ResolveEpisodeHandler episodeResolver,
    ResolveTaskHandler taskResolver,
    MarkTaskDoneHandler doneHandler,
    ILogger<DoneModule> logger
) : InteractionModuleBase<IInteractionContext>;
