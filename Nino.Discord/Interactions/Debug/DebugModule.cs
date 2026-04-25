// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Queries.Projects.GetDebugData;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.Debug;

[Group("debug", "Debug commands")]
public partial class DebugModule(
    GetDebugDataHandler dataHandler,
    IBotPermissionsService botPermissionsService
) : InteractionModuleBase<IInteractionContext>;
