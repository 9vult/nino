using System.Text.Json;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities.AzureDtos;
using NLog;
using CongaNodeDto = Nino.Records.CongaNodeDto;
using Task = System.Threading.Tasks.Task;

namespace Nino.Utilities;

public class Migrator(DataContext db)
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public async Task Migrate()
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        
        var projectsJson = await File.ReadAllTextAsync("mig/projects.json");
        var episodesJson = await File.ReadAllTextAsync("mig/episodes.json");
        var observersJson = await File.ReadAllTextAsync("mig/observers.json");
        var configJson = await File.ReadAllTextAsync("mig/configuration.json");
        
        var azureProjects = JsonSerializer.Deserialize<List<ProjectDto>>(projectsJson, options);
        var azureEpisodes = JsonSerializer.Deserialize<List<EpisodeDto>>(episodesJson, options);
        var azureObservers = JsonSerializer.Deserialize<List<ObserverDto>>(observersJson, options);
        var azureConfigs = JsonSerializer.Deserialize<List<ConfigurationDto>>(configJson, options);
        
        List<Project> projects = [];
        List<Configuration> configs = [];

        foreach (var azureProject in azureProjects!)
        {
            Log.Info($"Migrating project {azureProject.Nickname}...");
            Project project;
            try
            {
                project = new Project
                {
                    Id = Guid.NewGuid(),
                    GuildId = ulong.Parse(azureProject.GuildId),
                    Nickname = azureProject.Nickname,
                    Title = azureProject.Title,
                    OwnerId = ulong.Parse(azureProject.OwnerId),
                    Type = azureProject.Type != ProjectType.BD
                        ? azureProject.Type
                        : ProjectType.TV,
                    PosterUri = azureProject.PosterUri,
                    UpdateChannelId = ulong.Parse(azureProject.UpdateChannelId),
                    ReleaseChannelId = ulong.Parse(azureProject.ReleaseChannelId),
                    IsPrivate = azureProject.IsPrivate,
                    IsArchived = azureProject.IsArchived,
                    CongaParticipants = CongaGraph.Deserialize(azureProject.CongaParticipants.Select(p =>
                            new CongaNodeDto
                            {
                                Abbreviation = p.Abbreviation,
                                Dependents = p.Dependents.ToArray(),
                                Type = p.Type,
                            })
                        .ToArray()),
                    Motd = azureProject.Motd,
                    AniListId = azureProject.AniListId,
                    AniListOffset = azureProject.AniListOffset,
                    AirReminderEnabled = azureProject.AirReminderEnabled,
                    AirReminderChannelId = azureProject.AirReminderChannelId is not null
                        ? ulong.Parse(azureProject.AirReminderChannelId)
                        : null,
                    AirReminderRoleId = azureProject.AirReminderRoleId is not null
                        ? ulong.Parse(azureProject.AirReminderRoleId)
                        : null,
                    AirReminderUserId = azureProject.AirReminderUserId is not null
                        ? ulong.Parse(azureProject.AirReminderUserId)
                        : null,
                    CongaReminderEnabled = azureProject.CongaReminderEnabled,
                    CongaReminderPeriod = azureProject.CongaReminderPeriod,
                    CongaReminderChannelId = azureProject.CongaReminderChannelId is not null
                        ? ulong.Parse(azureProject.CongaReminderChannelId)
                        : null,
                    Created = azureProject.Created,
                    Aliases = azureProject.Aliases.Select(a => new Alias { Value = a }).ToList(),
                    KeyStaff = azureProject.KeyStaff.Select(s => new Staff
                    {
                        UserId = ulong.Parse(s.UserId),
                        Role = new Role
                        {
                            Abbreviation = s.Role.Abbreviation,
                            Name = s.Role.Name,
                            Weight = s.Role.Weight,
                        },
                        IsPseudo = s.IsPseudo ?? false
                    }).ToList(),
                    Administrators = azureProject.AdministratorIds
                        .Select(a => new Administrator { UserId = ulong.Parse(a) }).ToList(),
                    Episodes = [],
                    Observers = [],
                };
            }
            catch (Exception e)
            {
                var x = projects;
                Log.Error(e);
                return;
            }
            projects.Add(project);

            foreach (var azureEpisode in azureEpisodes!.Where(e => e.ProjectId == azureProject.Id))
            {
                var episode = new Episode
                {
                    Id = Guid.Empty,
                    ProjectId = project.Id,
                    GuildId = ulong.Parse(azureEpisode.GuildId),
                    Number = azureEpisode.Number,
                    Done = azureEpisode.Done,
                    ReminderPosted = azureEpisode.ReminderPosted,
                    Updated = azureEpisode.Updated,
                    Tasks = azureEpisode.Tasks.Select(t => new Records.Task
                    {
                        Id = Guid.Empty,
                        Abbreviation = t.Abbreviation,
                        Done = t.Done,
                        Updated = t.Updated,
                        LastReminded = t.LastReminded
                    }).ToList(),
                    AdditionalStaff = azureEpisode.AdditionalStaff.Select(s => new Staff
                    {
                        Id = Guid.Empty,
                        UserId = ulong.Parse(s.UserId),
                        Role = new Role
                        {
                            Abbreviation = s.Role.Abbreviation,
                            Name = s.Role.Name,
                            Weight = s.Role.Weight,
                        },
                        IsPseudo = s.IsPseudo ?? false
                    }).ToList(),
                    PinchHitters = azureEpisode.PinchHitters.Select(p => new PinchHitter
                    {
                        UserId = ulong.Parse(p.UserId),
                        Abbreviation = p.Abbreviation
                    }).ToList(),
                    Project = project,
                };
                
                project.Episodes.Add(episode);
            }

            foreach (var azureObserver in azureObservers!.Where(o => o.ProjectId == azureProject.Id))
            {
                var observer = new Observer
                {
                    Id = Guid.Empty,
                    GuildId = ulong.Parse(azureObserver.GuildId),
                    OriginGuildId = ulong.Parse(azureObserver.OriginGuildId),
                    OwnerId = ulong.Parse(azureObserver.OwnerId),
                    ProjectId = project.Id,
                    Blame = azureObserver.Blame,
                    RoleId = azureObserver.RoleId is not null ? ulong.Parse(azureObserver.RoleId) : null,
                    ProgressWebhook = azureObserver.ProgressWebhook,
                    ReleasesWebhook = azureObserver.ReleasesWebhook,
                    Project = project,
                };
                project.Observers.Add(observer);
            }
            
            Log.Info($"Project {project.Nickname} has been created with {project.Episodes.Count} episodes and {project.Observers.Count} observers");
        }
        
        await db.Projects.AddRangeAsync(projects);

        foreach (var azureConfig in azureConfigs!)
        {
            Log.Info($"Migrating configuration for  {azureConfig.GuildId}...");
            var config = new Configuration
            {
                Id = Guid.Empty,
                GuildId = ulong.Parse(azureConfig.GuildId),
                UpdateDisplay = azureConfig.UpdateDisplay,
                ProgressDisplay = azureConfig.ProgressDisplay,
                CongaPrefix = azureConfig.CongaPrefix,
                ReleasePrefix = azureConfig.ReleasePrefix,
                Locale = azureConfig.Locale,
                Administrators = azureConfig.AdministratorIds.Select(a => new Administrator { UserId = ulong.Parse(a) })
                    .ToList(),
            };
            await db.Configurations.AddAsync(config);
        }
        
        Log.Info("Writing to database...");
        
        await db.SaveChangesAsync();
        
        Log.Info("Done!");
    }
}