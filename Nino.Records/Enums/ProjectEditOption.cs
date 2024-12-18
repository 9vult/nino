using static Localizer.Localizer;

namespace Nino.Records.Enums
{
    public enum ProjectEditOption
    {
        Title = 0,
        Poster = 1,
        MOTD = 2,
        AniListId = 3,
        IsPrivate = 5,
        UpdateChannelID = 6,
        ReleaseChannelID = 7,
        Nickname = 8
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
                ProjectEditOption.AniListId => T("choice.project.edit.option.anilist", lng),
                ProjectEditOption.IsPrivate => T("choice.project.edit.option.private", lng),
                ProjectEditOption.UpdateChannelID => T("choice.project.edit.option.updatechannel", lng),
                ProjectEditOption.ReleaseChannelID => T("choice.project.edit.option.releasechannel", lng),
                ProjectEditOption.Nickname => T("choice.project.edit.option.nickname", lng),
                _ => type.ToString(),
            };
        }
    }
}
