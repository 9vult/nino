// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

/// <summary>
/// Type of Conga node
/// </summary>
public enum CongaNodeType
{
    /// <summary>
    /// Node represents a Key Staff position
    /// </summary>
    KeyStaff = 0,

    /// <summary>
    /// Node represents an Additional Staff position
    /// </summary>
    AdditionalStaff = 1,

    /// <summary>
    /// Node represents a special condition (e.g. Episode Aired)
    /// </summary>
    Special = 2,

    /// <summary>
    /// Node represents a group of nodes
    /// </summary>
    Group = 3,

    /// <summary>
    /// Node is unknown
    /// </summary>
    Unknown = 4,
}
