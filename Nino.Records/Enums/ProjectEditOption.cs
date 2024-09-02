﻿using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum ProjectEditOption
    {
        Title = 0,
        Poster = 1,
        MOTD = 2,
        AniDBId = 3,
        AirTime24h = 4,
        IsPrivate = 5,
        UpdateChannelID = 6,
        ReleaseChannelID = 7
    }

    public static class ProjectEditOptionExtensions
    {
        public static string ToFriendlyString(this ProjectEditOption type, string lng)
        {
            return type switch
            {
                ProjectEditOption.Title => T("choice.project.edit.option.title", lng),
                ProjectEditOption.Poster => T("choice.project.edit.option.poster", lng),
                ProjectEditOption.MOTD => T("choice.project.edit.option.motd", lng),
                ProjectEditOption.AniDBId => T("choice.project.edit.option.anidb", lng),
                ProjectEditOption.AirTime24h => T("choice.project.edit.option.airtime", lng),
                ProjectEditOption.IsPrivate => T("choice.project.edit.option.private", lng),
                ProjectEditOption.UpdateChannelID => T("choice.project.edit.option.updatechannel", lng),
                ProjectEditOption.ReleaseChannelID => T("choice.project.edit.option.progresschannel", lng),
                _ => type.ToString(),
            };
        }
    }
}