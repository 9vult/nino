// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Commands.Project.Create;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Project;

[Group("project", "Project management")]
public partial class ProjectModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    IBotPermissionsService botPermissionsService,
    CreateProjectHandler createHandler,
    ILogger<ProjectModule> logger
) : InteractionModuleBase<IInteractionContext> { }
