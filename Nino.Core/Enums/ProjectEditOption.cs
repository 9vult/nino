// SPDX-License-Identifier: MPL-2.0

namespace Nino.Core.Enums;

/// <summary>
/// Editable project options
/// </summary>
public enum ProjectEditOption
{
    Title = 0,
    Poster = 1,
    Motd = 2,
    AniListId = 3,
    IsPrivate = 5,
    UpdateChannelId = 6,
    ReleaseChannelId = 7,
    Nickname = 8,
    AniListOffset = 9,
    AirNotificationDelay = 10,
}

public static class ProjectEditOptionExtensions
{
    public static string ToFriendlyString(this ProjectEditOption type, string lng)
    {
        return type switch
        {
            ProjectEditOption.Title => T("choice.project.edit.option.title", lng),
            ProjectEditOption.Poster => T("choice.project.edit.option.poster", lng),
            ProjectEditOption.Motd => T("choice.project.edit.option.motd", lng),
            ProjectEditOption.AniListId => T("choice.project.edit.option.aniList", lng),
            ProjectEditOption.IsPrivate => T("choice.project.edit.option.private", lng),
            ProjectEditOption.UpdateChannelId => T("choice.project.edit.option.updateChannel", lng),
            ProjectEditOption.ReleaseChannelId => T(
                "choice.project.edit.option.releaseChannel",
                lng
            ),
            ProjectEditOption.Nickname => T("choice.project.edit.option.nickname", lng),
            ProjectEditOption.AniListOffset => T("choice.project.edit.option.aniListOffset", lng),
            ProjectEditOption.AirNotificationDelay => T(
                "choice.project.edit.option.airNotificationDelay",
                lng
            ),
            _ => type.ToString(),
        };
    }
}
