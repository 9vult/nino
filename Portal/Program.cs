using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Nino.Records;
using Nino.Records.Enums;
using Portal;

using StreamReader projectsSr = new StreamReader("projects.json");
var projectJson = projectsSr.ReadToEnd();
var projectsFB = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, FBProject>>>(projectJson);

Dictionary<ulong, List<Project>> projects = [];
Dictionary<string, List<Episode>> episodes = [];
List<Configuration> configurations = [];
#if false
foreach (var gid in projectsFB!.Keys)
{
    ulong guildId = ulong.Parse(gid);
    foreach (var nickname in projectsFB![gid].Keys)
    {
        var project = projectsFB![gid][nickname]!;
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
            AdministratorIds = project.administrators?.Select(ulong.Parse).ToArray() ?? [],
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

using StreamReader configSr = new StreamReader("configuration.json");
var configJson = configSr.ReadToEnd();
var configFB = JsonConvert.DeserializeObject<Dictionary<string, FBConfig>>(configJson);

foreach (var pair in configFB!)
{
    var guildId = pair.Key;
    var config = pair.Value;

    configurations.Add(new Configuration()
    {
        Id = $"{guildId}-conf",
        GuildId = ulong.Parse(guildId),
        UpdateDisplay = config.progressDisplay switch
        {
            "Normal" => UpdatesDisplayType.Normal,
            "Extended" => UpdatesDisplayType.Extended,
            _ => UpdatesDisplayType.Normal
        },
        ProgressDisplay = config.doneDisplay switch
        {
            "Succinct" => ProgressDisplayType.Succinct,
            "Verbose" => ProgressDisplayType.Verbose,
            _ => ProgressDisplayType.Succinct
        },
        AdministratorIds = config.administrators?.Select(ulong.Parse).ToArray() ?? [],
        ReleasePrefix = config.releasePrefix
    });
}

Console.WriteLine($"Ready! Processed {projects.Keys.Count} guilds ({configurations.Count} configs), {projects.Values.Sum(c => c.Count)} projects ({episodes.Values.Sum(c => c.Count)} episodes)");
Console.WriteLine("Press any key to continue to database setup...");
Console.ReadKey(); // PAUSE

#endif

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

var projectSql = new QueryDefinition("SELECT * FROM c");
List<Project> rawProjects = [];
using FeedIterator<Project> feed = _projectsContainer!.GetItemQueryIterator<Project>(queryDefinition: projectSql);
while (feed.HasMoreResults)
{
    FeedResponse<Project> response = await feed.ReadNextAsync();
    foreach (Project p in response)
    {
        rawProjects.Add(p);
    }
}

foreach (Project p in rawProjects)
{
    int i = 1;
    foreach (var ks in p.KeyStaff)
    {
        if (ks.Role.Weight is null)
        {
            ks.Role.Weight = i;
        }
        else
        {
            ks.Role.Weight++;
        }
        i++;
    }
    await _projectsContainer.UpsertItemAsync(p);
}

Console.WriteLine();

#if false

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

Console.WriteLine("Episodes are ready. Press any key to begin writing configs...");
Console.ReadKey(); // PAUSE

foreach (var config in configurations)
{
    await _configurationContainer.UpsertItemAsync(config);
}

#endif

Console.WriteLine("Done!");
