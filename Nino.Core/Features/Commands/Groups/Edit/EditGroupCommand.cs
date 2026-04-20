// SPDX-License-Identifier: MPL-2.0

using Nino.Domain.Enums;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Core.Features.Commands.Groups.Edit;

public sealed record EditGroupCommand(
    GroupId GroupId,
    UserId RequestedBy,
    bool OverrideVerification,
    Locale? Locale = null,
    bool? PublishPrivateProgress = null,
    ProgressResponseType? ProgressResponseType = null,
    ProgressPublishType? ProgressPublishType = null,
    CongaPrefixType? CongaPrefixType = null,
    string? ReleasePrefix = null
) : ICommand;
