// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Commands.Projects.Admins.Add;
using Nino.Core.Features.Commands.Projects.Admins.Remove;
using Nino.Core.Features.Commands.Projects.AirNotifications.Disable;
using Nino.Core.Features.Commands.Projects.AirNotifications.Enable;
using Nino.Core.Features.Commands.Projects.Aliases.Add;
using Nino.Core.Features.Commands.Projects.Aliases.Remove;
using Nino.Core.Features.Commands.Projects.Create;
using Nino.Core.Features.Commands.Projects.Edit;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Features.Queries.Projects.Roster;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Projects;

[Group("project", "Project management")]
public partial class ProjectModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    IBotPermissionsService botPermissionsService,
    ResolveProjectHandler projectResolver,
    GetGenericProjectDataHandler getProjectDataHandler,
    CreateProjectHandler createHandler,
    EditProjectHandler editHandler,
    ProjectRosterHandler rosterHandler
) : InteractionModuleBase<IInteractionContext>
{
    [Group("admin", "Admin management")]
    public partial class AdminModule(
        IIdentityService identityService,
        IInteractionIdentityService interactionIdService,
        ResolveProjectHandler projectResolver,
        AddProjectAdminHandler addAdminHandler,
        RemoveProjectAdminHandler removeAdminHandler,
        GetGenericProjectDataHandler getGenericDataHandler
    ) : InteractionModuleBase<IInteractionContext>;

    [Group("alias", "Alias management")]
    public partial class AliasModule(
        IInteractionIdentityService interactionIdService,
        ResolveProjectHandler projectResolver,
        AddAliasHandler addAliasHandler,
        RemoveAliasHandler removeAliasHandler,
        GetGenericProjectDataHandler getGenericDataHandler
    ) : InteractionModuleBase<IInteractionContext>;

    [Group("air-notifications", "Air Notification management")]
    public partial class AirNotificationsModule(
        IIdentityService identityService,
        IInteractionIdentityService interactionIdService,
        ResolveProjectHandler projectResolver,
        EnableAirNotificationsHandler enableHandler,
        DisableAirNotificationsHandler disableHandler,
        GetGenericProjectDataHandler getGenericDataHandler
    ) : InteractionModuleBase<IInteractionContext>;
}
