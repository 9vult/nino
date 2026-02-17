// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Actions.Project.Create;
using Nino.Core.Actions.Project.Delete;
using Nino.Core.Actions.Project.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Project;

[Group("project", "Project management")]
public partial class ProjectModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IDataService dataService,
    IInteractionIdentityService interactionIdService,
    IUserVerificationService verificationService,
    ProjectResolveHandler projectResolver,
    ProjectCreateHandler createHandler,
    ProjectDeleteHandler deleteHandler,
    ILogger<ProjectModule> logger
) : InteractionModuleBase<IInteractionContext> { }
