﻿
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
        public static string ToFriendlyString(this ProjectEditOption type)
        {
            return type switch
            {
                ProjectEditOption.Title => "Title",
                ProjectEditOption.Poster => "Poster",
                ProjectEditOption.MOTD => "MOTD",
                ProjectEditOption.AniDBId => "AniDBId",
                ProjectEditOption.AirTime24h => "AirTime24h",
                ProjectEditOption.IsPrivate => "IsPrivate",
                ProjectEditOption.UpdateChannelID => "UpdateChannelID",
                ProjectEditOption.ReleaseChannelID => "ReleaseChannelID",
                _ => type.ToString(),
            };
        }
    }
}
