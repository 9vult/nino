// SPDX-License-Identifier: MPL-2.0

namespace Nino.Localization;

/// <summary>
/// Part of a string for interpolation
/// </summary>
/// <param name="Name">Name of the value</param>
/// <param name="Index">Index of the value</param>
/// <param name="Match">Full match</param>
internal sealed record StringPart(string Name, int Index, string Match);
