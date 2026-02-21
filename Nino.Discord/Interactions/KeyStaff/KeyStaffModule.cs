// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features.KeyStaff.Add;
using Nino.Core.Features.KeyStaff.Remove;
using Nino.Core.Features.KeyStaff.Rename;
using Nino.Core.Features.KeyStaff.Swap;
using Nino.Core.Features.Project.Resolve;
using Nino.Core.Services;
using Nino.Discord.Services;

namespace Nino.Discord.Interactions.KeyStaff;

[Group("keystaff", "Manage project Key Staff")]
public partial class KeyStaffModule(
    DiscordSocketClient client,
    IIdentityService identityService,
    IDataService dataService,
    IStateService stateService,
    IInteractionIdentityService interactionIdService,
    IUserVerificationService verificationService,
    ResolveProjectHandler projectResolver,
    AddKeyStaffHandler addHandler,
    SwapKeyStaffHandler swapHandler,
    RenameKeyStaffHandler renameHandler,
    RemoveKeyStaffHandler removeHandler,
    ILogger<KeyStaffModule> logger
) : InteractionModuleBase<IInteractionContext> { }
