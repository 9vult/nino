// SPDX-License-Identifier: MPL-2.0

namespace Nino.Localization;

internal sealed class LocalizationException : Exception
{
    public LocalizationException() { }

    public LocalizationException(string message)
        : base(message) { }

    public LocalizationException(string message, Exception inner)
        : base(message, inner) { }
}
