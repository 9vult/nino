// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Queries.Projects.GetDebugData;

namespace Nino.Discord.Interactions.Debug;

[Group("debug", "Debug commands")]
public partial class DebugModule(GetDebugDataHandler dataHandler) : InteractionModuleBase<IInteractionContext>;
