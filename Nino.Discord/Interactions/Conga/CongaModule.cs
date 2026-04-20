// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Commands.Projects.Conga.AddEdge;
using Nino.Core.Features.Commands.Projects.Conga.AddGroup;
using Nino.Core.Features.Commands.Projects.Conga.AddGroupMember;
using Nino.Core.Features.Commands.Projects.Conga.Import;
using Nino.Core.Features.Commands.Projects.Conga.RemoveEdge;
using Nino.Core.Features.Commands.Projects.Conga.RemoveGroup;
using Nino.Core.Features.Commands.Projects.Conga.RemoveGroupMember;
using Nino.Core.Features.Commands.Projects.CongaReminders.Disable;
using Nino.Core.Features.Commands.Projects.CongaReminders.Enable;
using Nino.Core.Features.Queries.Projects.Conga.GetDot;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Conga;

[Group("conga", "Conga management")]
public partial class CongaModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    ResolveProjectHandler projectResolver,
    ImportCongaHandler importHandler,
    GetGenericProjectDataHandler getProjectDataHandler,
    GetCongaDotHandler getDotHandler,
    ILogger<CongaModule> logger
) : InteractionModuleBase<IInteractionContext>
{
    [Group("edge", "Group management")]
    public partial class EdgeModule(
        IIdentityService identityService,
        IInteractionIdentityService interactionIdService,
        ResolveProjectHandler projectResolver,
        AddCongaEdgeHandler addEdgeHandler,
        RemoveCongaEdgeHandler removeEdgeHandler,
        GetGenericProjectDataHandler getProjectDataHandler,
        GetCongaDotHandler getDotHandler,
        HttpClient httpClient,
        ILogger<EdgeModule> logger
    ) : InteractionModuleBase<IInteractionContext>;

    [Group("group", "Group management")]
    public partial class GroupModule(
        IIdentityService identityService,
        IInteractionIdentityService interactionIdService,
        ResolveProjectHandler projectResolver,
        AddCongaGroupHandler createGroupHandler,
        RemoveCongaGroupHandler deleteGroupHandler,
        AddCongaGroupMemberHandler addMemberHandler,
        RemoveCongaGroupMemberHandler removeMemberHandler,
        GetGenericProjectDataHandler getProjectDataHandler,
        GetCongaDotHandler getDotHandler,
        HttpClient httpClient,
        ILogger<GroupModule> logger
    ) : InteractionModuleBase<IInteractionContext>;

    [Group("reminders", "Reminder management")]
    public partial class RemindersModule(
        IInteractionIdentityService interactionIdService,
        ResolveProjectHandler projectResolver,
        EnableCongaRemindersHandler enableHandler,
        DisableCongaRemindersHandler disableHandler,
        GetGenericProjectDataHandler getProjectDataHandler
    ) : InteractionModuleBase<IInteractionContext>;
}
