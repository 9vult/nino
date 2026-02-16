// SPDX-License-Identifier: MPL-2.0

using System.Globalization;
using System.Text.Json.Serialization;

namespace Nino.Core.Entities;

/// <summary>
/// An episode in a <see cref="Project"/>
/// </summary>
public class Episode
{
    [Key]
    public Guid Id { get; set; }
    public required Guid ProjectId { get; set; }
    public required Guid GroupId { get; set; }

    [MaxLength(32)]
    public required string Number { get; set; }
    public required bool Done { get; set; }
    public required bool AirReminderPosted { get; set; }
    public DateTimeOffset? Updated { get; set; }

    public ICollection<Task> Tasks { get; set; } = [];
    public ICollection<Staff> AdditionalStaff { get; set; } = [];
    public ICollection<PinchHitter> PinchHitters { get; set; } = [];

    public Project Project { get; set; } = null!;
    public Group Group { get; set; } = null!;

    /// <summary>
    /// Canonicalize an episode number
    /// </summary>
    /// <param name="input">Raw episode number</param>
    /// <returns>Canonical episode number</returns>
    /// <remarks>If the episode number is not a decimal, it is returned as-is</remarks>
    public static string CanonicalizeEpisodeNumber(string input)
    {
        var trim = input.Trim();
        var replaced = input.Replace(',', '.');
        // If replacing commas with periods results in a decimal, use that. Otherwise, keep the commas.
        return decimal.TryParse(replaced, CultureInfo.InvariantCulture, out var decimalValue)
            ? decimalValue.ToString(CultureInfo.InvariantCulture)
            : trim;
    }

    /// <summary>
    /// Check if the episode number is a number
    /// </summary>
    /// <param name="input">Raw episode number</param>
    /// <param name="episodeNumber">Output number as a decimal</param>
    /// <returns><see langword="true"/> if the episode number is a number</returns>
    public static bool EpisodeNumberIsNumber(string input, out decimal episodeNumber)
    {
        return decimal.TryParse(input, out episodeNumber);
    }

    /// <summary>
    /// Check if the episode number is an integer
    /// </summary>
    /// <param name="input">Raw episode number</param>
    /// <param name="episodeNumber">Output number as an integer</param>
    /// <returns><see langword="true"/> if the episode number is an integer</returns>
    public static bool EpisodeNumberIsInteger(string input, out int episodeNumber)
    {
        return int.TryParse(input, out episodeNumber);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"E[{Id} ({Number})]";
    }
}
