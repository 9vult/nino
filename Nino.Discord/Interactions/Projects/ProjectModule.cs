// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Commands.Projects.Create;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Projects;

[Group("project", "Project management")]
public partial class ProjectModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    IBotPermissionsService botPermissionsService,
    GetGenericProjectDataHandler getProjectDataHandler,
    CreateProjectHandler createHandler,
    ILogger<ProjectModule> logger
) : InteractionModuleBase<IInteractionContext> { }
