// SPDX-License-Identifier: MPL-2.0

namespace Nino.Domain.Enums;

public enum CongaModificationResult
{
    Success,
    MixedGroups,
    SelfLoop,
    Cycle,
    Duplicate,
    DuplicateMember,
    NoGroup,
    IllegalTree,
    NotFound,
    NoLink,
}
