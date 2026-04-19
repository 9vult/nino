// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Commands.Tasks.Add;
using Nino.Core.Features.Commands.Tasks.Edit;
using Nino.Core.Features.Commands.Tasks.Remove;
using Nino.Core.Features.Queries.Episodes.Resolve;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Features.Queries.Tasks.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Tasks;

[Group("task", "Manage Tasks")]
public partial class TaskModule(
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    GetGenericProjectDataHandler getProjectDataHandler,
    ResolveProjectHandler projectResolver,
    ResolveEpisodeHandler episodeResolver,
    ResolveTaskHandler taskResolver,
    AddTaskHandler addHandler,
    RemoveTaskHandler removeHandler,
    EditTaskHandler swapHandler
) : InteractionModuleBase<IInteractionContext> { }
