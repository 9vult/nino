// SPDX-License-Identifier: MPL-2.0

using System.Reflection;
using static Nino.Localization.Localizer;

namespace Nino.Localization;

public static class LocalizedEnumExtensions
{
    public static string ToFriendlyString<TEnum>(this TEnum value, string lng)
        where TEnum : struct, Enum
    {
        var key = typeof(TEnum)
            .GetField(value.ToString())
            ?.GetCustomAttribute<LocalizationKeyAttribute>()
            ?.Key;

        return key is not null ? T(key, lng) : value.ToString();
    }
}
