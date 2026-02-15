// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;

namespace Nino.Discord.Interactions.Project;

[Group("project", "Project management")]
public partial class ProjectModule(ILogger<ProjectModule> logger)
    : InteractionModuleBase<IInteractionContext> { }
