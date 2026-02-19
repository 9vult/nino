// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Project.Create;
using Nino.Core.Features.Project.Delete;
using Nino.Core.Features.Project.Export;
using Nino.Core.Features.Project.Resolve;
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
    ResolveProjectHandler projectResolver,
    CreateProjectHandler handler,
    ExportProjectHandler exportHandler,
    DeleteProjectHandler deleteHandler,
    ILogger<ProjectModule> logger
) : InteractionModuleBase<IInteractionContext> { }
