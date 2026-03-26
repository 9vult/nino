// SPDX-License-Identifier: MPL-2.0

namespace Nino.Localization;

/// <summary>
/// Attribute for assigning a localization field, especially enum members
/// </summary>
/// <param name="key">Localization key</param>
public sealed class LocalizationKeyAttribute(string key) : Attribute
{
    /// <summary>
    /// Localization key
    /// </summary>
    public string Key { get; } = key;
}
