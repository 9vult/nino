// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.Commands.Groups.Admins.Add;
using Nino.Core.Features.Commands.Groups.Admins.Remove;
using Nino.Core.Features.Commands.Groups.Edit;
using Nino.Core.Features.Queries.Groups.GetGenericData;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Groups;

[Group("group", "Group management")]
public partial class GroupModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    EditGroupHandler editHandler,
    AddGroupAdminHandler addAdminHandler,
    RemoveGroupAdminHandler removeAdminHandler,
    GetGenericGroupDataHandler getGenericDataHandler
) : InteractionModuleBase<IInteractionContext> { }
