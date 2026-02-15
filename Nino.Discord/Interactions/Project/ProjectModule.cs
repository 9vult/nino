// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Actions.Project.Delete;
using Nino.Core.Services;

namespace Nino.Discord.Interactions.Project;

[Group("project", "Project management")]
public partial class ProjectModule(
    IUserIdentityService identityService,
    ProjectDeleteHandler deleteHandler,
    ILogger<ProjectModule> logger
) : InteractionModuleBase<IInteractionContext> { }
