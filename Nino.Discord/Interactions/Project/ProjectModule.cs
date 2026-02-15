// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Actions.Project.Delete;
using Nino.Core.Actions.Project.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Project;

[Group("project", "Project management")]
public partial class ProjectModule(
    IInteractionIdentityService identityService,
    IUserVerificationService verificationService,
    ProjectResolveHandler projectResolver,
    ProjectDeleteHandler deleteHandler,
    ILogger<ProjectModule> logger
) : InteractionModuleBase<IInteractionContext> { }
