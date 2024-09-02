
using Newtonsoft.Json;
using Nino.Records;
using Nino.Records.Enums;
using Portal;

using StreamReader sr = new StreamReader("projects.json");
var json = sr.ReadToEnd();
var FB = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, FBProject>>>(json);

List<Project> projects = [];
List<Episode> episodes = [];

foreach (var gid in FB!.Keys)
{
    ulong guildId = ulong.Parse(gid);
    foreach (var nickname in FB![gid].Keys)
    {
        var project = FB![gid][nickname]!;
        var projectId = $"{guildId}-{nickname}";

        ulong? airRole = !string.IsNullOrEmpty(project.airReminderRole) ? project?.airReminderRole == "@everyone" ? guildId : ulong.Parse(project!.airReminderRole!) : null;

        projects.Add(new Project()
        {
            Id = projectId,
            GuildId = guildId,
            Nickname = nickname,
            Title = project.title,
            OwnerId = ulong.Parse(project.owner),
            AdministratorIds = project.administrators?.Select(a => ulong.Parse(a)).ToArray() ?? [],
            KeyStaff = project.keyStaff?.Select(ks => new Staff
            {
                UserId = ulong.Parse(ks.Value.id),
                Role = new Role
                {
                    Abbreviation = ks.Value.role.abbreviation,
                    Name = ks.Value.role.title,
                    Weight = ks.Value.role.weight
                }
            }).ToArray() ?? [],
            Type = project.type switch
            {
                "TV" => ProjectType.TV,
                "Movie" => ProjectType.Movie,
                "BD" => ProjectType.BD,
                _ => ProjectType.TV
            },
            PosterUri = project.poster,
            UpdateChannelId = ulong.Parse(project.updateChannel),
            ReleaseChannelId = ulong.Parse(project.releaseChannel),
            IsPrivate = project.isPrivate ?? false,
            CongaParticipants = project.conga?.Select(c => new CongaParticipant()
            {
                Current = c.Value.current,
                Next = c.Value.next
            }).ToArray() ?? [],
            Aliases = project?.aliases ?? [],
            Motd = project?.motd,
            AniDBId = project?.anidb,
            AirTime = project?.airTime,
            AirReminderEnabled = project?.airReminderEnabled ?? false,
            SerializationAirReminderChannelId = project?.airReminderChannel,
            AirReminderRoleId = airRole,
        });

        if (project?.episodes == null)
            continue;
        foreach (var epid in project.episodes.Keys)
        {
            var episode = project.episodes[epid]!;
            episodes.Add(new Episode()
            {
                Id = $"{projectId}-{episode.number}",
                ProjectId = projectId,
                GuildId = guildId,
                Number = episode.number,
                Done = episode.done,
                ReminderPosted = episode.airReminderPosted ?? false,
                AdditionalStaff = episode.additionalStaff?.Select(ks => new Staff()
                {
                    UserId = ulong.Parse(ks.Value.id),
                    Role = new Role
                    {
                        Abbreviation = ks.Value.role.abbreviation,
                        Name = ks.Value.role.title,
                        Weight = null
                    }
                }).ToArray() ?? [],
                Tasks = episode.tasks?.Select(t => new Nino.Records.Task
                {
                    Abbreviation = t.Value.abbreviation,
                    Done = t.Value.done
                }).ToArray() ?? [],
                Updated = episode.updated != null ? DateTimeOffset.FromUnixTimeSeconds((long)episode.updated) : null
            });
        }
    }
}

Console.WriteLine();
