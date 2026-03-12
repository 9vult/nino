// SPDX-License-Identifier: MPL-2.0

namespace Nino.Localization;

/// <summary>
/// Attribute for assigning a localization key to a field, especially enum members.
/// </summary>
/// <param name="key">Localization key</param>
[AttributeUsage(AttributeTargets.Field)]
public sealed class LocalizationKeyAttribute(string key) : Attribute
{
    /// <summary>
    /// Localization key
    /// </summary>
    public string Key { get; } = key;
}
