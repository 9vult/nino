// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Commands.TemplateStaff.Add;
using Nino.Core.Features.Commands.TemplateStaff.Edit;
using Nino.Core.Features.Commands.TemplateStaff.Remove;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Core.Features.Queries.Projects.Resolve;
using Nino.Core.Features.Queries.TemplateStaff.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.TemplateStaff;

[Group("template-staff", "Manage Template Staff")]
public partial class TemplateStaffModule(
    IIdentityService identityService,
    IInteractionIdentityService interactionIdService,
    GetGenericProjectDataHandler getProjectDataHandler,
    ResolveProjectHandler projectResolver,
    ResolveTemplateStaffHandler staffResolver,
    AddTemplateStaffHandler addHandler,
    RemoveTemplateStaffHandler removeHandler,
    EditTemplateStaffHandler editHandler
) : InteractionModuleBase<IInteractionContext>;
