using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nino.Records
{
    internal record Project
    {
        public required string Id;
        public required string GuildId;
        public required string Title;
        public required string Nickname;
        public required string OwnerId;
        public required string[] AdministratorIds;
        public required Staff[] KeyStaff;
        public required string Type;
        public required string PosterUri;
        public required string UpdateChannelId;
        public required string ReleaseChannelId;
        public required bool IsPrivate;
        public CongaParticipant[]? CongaParticipants;
        public string[]? Aliases;
        public string? Motd;
        public string? AniDBId;
        public string? AirTime;
        public string? AirReminderChannelId;
        public string? AirReminderRoleId;
        public bool? AirReminderEnabled;
    }
}
