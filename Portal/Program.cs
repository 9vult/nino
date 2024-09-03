using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nino.Records;
using Nino.Records.Enums;
using Portal;

using StreamReader sr = new StreamReader("projects.json");
var json = sr.ReadToEnd();
var FB = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, FBProject>>>(json);

Dictionary<ulong, List<Project>> projects = [];
Dictionary<string, List<Episode>> episodes = [];

foreach (var gid in FB!.Keys)
{
    ulong guildId = ulong.Parse(gid);
    foreach (var nickname in FB![gid].Keys)
    {
        var project = FB![gid][nickname]!;
        var projectId = $"{guildId}-{nickname}";

        ulong? airRole = !string.IsNullOrEmpty(project.airReminderRole) ? project?.airReminderRole == "@everyone" ? guildId : ulong.Parse(project!.airReminderRole!) : null;

        if (!projects.TryGetValue(guildId, out var guildList))
        {
            guildList = [];
            projects[guildId] = guildList;
        }

        guildList.Add(new Project()
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

        List<Episode> episodeList = [];
        foreach (var epid in project.episodes.Keys)
        {
            var episode = project.episodes[epid]!;
            episodeList.Add(new Episode()
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
        episodes[projectId] = episodeList;
    }
}
Console.WriteLine($"Ready! Processed {projects.Keys.Count} guilds, {projects.Values.Sum(c => c.Count)} projects ({episodes.Values.Sum(c => c.Count)})");
Console.WriteLine("Press any key to continue to database setup...");
Console.ReadKey(); // PAUSE

// Set up database
var endpoint = "";
var secret = "";
var dbname = "";

CosmosClient _client = new(
    endpoint,
    secret,
    new CosmosClientOptions
    {
        ApplicationName = "Nino",
        SerializerOptions = new CosmosSerializationOptions
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    }
);

Database _database = await _client.CreateDatabaseIfNotExistsAsync(dbname);
Container _projectsContainer = await _database.CreateContainerIfNotExistsAsync("Projects", "/guildId");
Container _episodesContainer = await _database.CreateContainerIfNotExistsAsync("Episodes", "/projectId");
Container _configurationContainer = await _database.CreateContainerIfNotExistsAsync("Configuration", "/guildId");

Console.WriteLine("Database is ready. Press any key to begin writing projects...");
Console.ReadKey(); // PAUSE

foreach (var pair in projects)
{
    var guildId = pair.Key;
    var list = pair.Value;

    TransactionalBatch pBatch = _projectsContainer.CreateTransactionalBatch(partitionKey: new PartitionKey(guildId.ToString()));
    foreach (var project in list)
    {
        pBatch.UpsertItem(project);
    }
    await pBatch.ExecuteAsync();
}

Console.WriteLine("Projects are ready. Press any key to begin writing episodes...");
Console.ReadKey(); // PAUSE

foreach (var pair in episodes)
{
    var projectId = pair.Key;
    var list = pair.Value;

    TransactionalBatch eBatch = _episodesContainer.CreateTransactionalBatch(partitionKey: new PartitionKey(projectId));
    foreach (var episode in list)
    {
        eBatch.UpsertItem(episode);
    }
    await eBatch.ExecuteAsync();
}

Console.WriteLine("Done!");
