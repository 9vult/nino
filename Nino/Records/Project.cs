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
        public required ulong GuildId;
        public required string Nickname;
        public required string Title;
        public required ulong OwnerId;
        public required string[] AdministratorIds;
        public required Staff[] KeyStaff;
        public required ProjectType Type;
        public required string PosterUri;
        public required ulong UpdateChannelId;
        public required ulong ReleaseChannelId;
        public required bool IsPrivate;
        public required CongaParticipant[] CongaParticipants;
        public required string[] Aliases;
        public string? Motd;
        public string? AniDBId;
        public string? AirTime;
        public ulong? AirReminderChannelId;
        public ulong? AirReminderRoleId;
        public bool? AirReminderEnabled;
    }
}
