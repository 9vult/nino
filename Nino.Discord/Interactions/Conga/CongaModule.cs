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
    GetGenericProjectDataHandler getProjectDataHandler,
    AddCongaEdgeHandler addEdgeHandler,
    RemoveCongaEdgeHandler removeEdgeHandler,
    ImportCongaHandler importHandler
) : InteractionModuleBase<IInteractionContext>
{
    [Group("group", "Group management")]
    public partial class AdminModule(
        IIdentityService identityService,
        IInteractionIdentityService interactionIdService,
        ResolveProjectHandler projectResolver,
        AddCongaGroupHandler createGroupHandler,
        RemoveCongaGroupHandler deleteGroupHandler,
        AddCongaGroupMemberHandler addMemberHandler,
        RemoveCongaGroupMemberHandler removeMemberHandler,
        GetGenericProjectDataHandler getGenericDataHandler
    ) : InteractionModuleBase<IInteractionContext>;

    [Group("reminders", "Reminder management")]
    public partial class AliasModule(
        IInteractionIdentityService interactionIdService,
        ResolveProjectHandler projectResolver,
        EnableCongaRemindersHandler enableHandler,
        DisableCongaRemindersHandler disableHandler,
        GetGenericProjectDataHandler getGenericDataHandler
    ) : InteractionModuleBase<IInteractionContext>;
}
