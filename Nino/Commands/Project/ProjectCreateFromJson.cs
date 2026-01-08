using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Discord;
using Discord.Interactions;
using Nino.Records;
using Nino.Records.Json;
using Nino.Services;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;
using Task = Nino.Records.Task;

namespace Nino.Commands;

public partial class ProjectManagement
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        IncludeFields = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    [SlashCommand("create-from-json", "Create a project using a json file")]
    public async Task<RuntimeResult> CreateFromJson(IAttachment file)
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;

        var guildId = interaction.GuildId ?? 0;
        var guild = Nino.Client.GetGuild(guildId);
        var member = guild.GetUser(interaction.User.Id);
        if (!Utils.VerifyAdministrator(db, member, guild))
            return await Response.Fail(T("error.notPrivileged", lng), interaction);

        Log.Info(
            $"Project creation from json file requested by M[{interaction.User.Id} (@{interaction.User.Username})]"
        );

        // Parse the json
        ProjectCreateDto? template;
        try
        {
            Log.Trace("Attempting to get and parse JSON...");
            using var client = new HttpClient();
            var content = await client.GetStringAsync(file.Url);
            using var json = JsonDocument.Parse(content);

            if (!json.RootElement.TryGetProperty("Episodes", out _))
            {
                Log.Trace("Input appears to be a Create-From-Json file, proceeding normally...");
                template = JsonSerializer.Deserialize<ProjectCreateDto>(content, JsonOptions);
                if (template is null)
                {
                    Log.Trace("Project creation from json file failed (null)");
                    return await Response.Fail(T("error.generic", lng), interaction);
                }
            }
            else
            {
                Log.Trace("Input appears to be an Export file, attempting to migrate...");
                var import = JsonSerializer.Deserialize<Export>(content, JsonOptions);
                if (import is null)
                {
                    Log.Trace("Project creation from json file failed (null)");
                    return await Response.Fail(T("error.generic", lng), interaction);
                }

                template = new ProjectCreateDto
                {
                    Nickname = import.Project.Nickname,
                    AniListId = import.Project.AniListId ?? 0,
                    IsPrivate = import.Project.IsPrivate,
                    UpdateChannelId = import.Project.UpdateChannelId,
                    ReleaseChannelId = import.Project.ReleaseChannelId,
                    Title = import.Project.Title,
                    Type = import.Project.Type,
                    Length = import.Episodes.Length,
                    PosterUri = import.Project.PosterUri,
                    FirstEpisode = 1,
                    AdministratorIds = import
                        .Project.Administrators.Select(a => a.UserId)
                        .ToArray(),
                    Aliases = import.Project.Aliases.Select(a => a.Value).ToArray(),
                    CongaParticipants = import.Project.CongaParticipants.Serialize(),
                    AdditionalStaff = [],
                    KeyStaff = import
                        .Project.KeyStaff.Select(s => new StaffCreateDto
                        {
                            UserId = s.UserId,
                            Role = s.Role,
                            IsPseudo = s.IsPseudo,
                        })
                        .ToArray(),
                };
                Log.Trace(
                    $"Migration successful! Assumption: {template.Length} episodes, starting at 1."
                );
            }

            template.FirstEpisode ??= 1;
            Log.Trace("Getting and parsing JSON successful!");
        }
        catch (Exception e)
        {
            Log.Error(e);
            Log.Trace("Project creation from json file failed");
            return await Response.Fail(e.Message, interaction);
        }

        var ownerId = interaction.User.Id;

        // Sanitize input
        template.Nickname = template.Nickname.Trim().ToLowerInvariant().Replace(" ", string.Empty); // remove spaces

        // Verify data
        if (db.Projects.Any(p => p.GuildId == guildId && p.Nickname == template.Nickname))
            return await Response.Fail(
                T("error.project.nameInUse", lng, template.Nickname),
                interaction
            );

        var defaultFieldNames = string.Join(
            ", ",
            new[]
            {
                nameof(template.Title),
                nameof(template.Length),
                nameof(template.Type),
                nameof(template.PosterUri),
            }
                .Zip(
                    new object?[]
                    {
                        template.Title,
                        template.Length,
                        template.Type,
                        template.PosterUri,
                    }
                )
                .Where(p => p.Second is null)
                .Select(p => p.First)
        );

        if (defaultFieldNames.Length > 0)
            Log.Info(
                $"AniList will be used in the construction of project '{template.Nickname}' for the following fields: {defaultFieldNames}"
            );

        if (defaultFieldNames.Length > 0 && template.AniListId > 0)
        {
            var apiResponse = await AniListService.Get(template.AniListId);
            if (apiResponse is not null && apiResponse.Error is null)
            {
                template.Title ??= apiResponse.Title;
                template.Length ??= apiResponse.EpisodeCount;
                template.Type ??= apiResponse.Type;

                if (template.Title is null || template.Length is null || template.Length < 1)
                {
                    return await Response.Fail(
                        T(apiResponse.Error ?? "error.anilist.create", lng),
                        interaction
                    );
                }

                if (
                    template.PosterUri is null
                    || !Uri.TryCreate(template.PosterUri, UriKind.Absolute, out _)
                )
                {
                    template.PosterUri = apiResponse.CoverImage ?? AniListService.FallbackPosterUri;
                }
            }
            else
            {
                return await Response.Fail(
                    T(apiResponse?.Error ?? "error.anilist.create", lng),
                    interaction
                );
            }
        }
        else
        {
            if (defaultFieldNames.Length > 0)
                Log.Warn(
                    $"AniList ID not specified! Skipping autofill for fields: {defaultFieldNames}..."
                );
        }

        // Configure weights
        var idxWeight = 1;
        foreach (var ks in template.KeyStaff)
        {
            ks.Role.Weight ??= idxWeight++;
        }
        // Go through the additional staff dictionary
        template.AdditionalStaff ??= [];
        foreach (var ep in template.AdditionalStaff)
        {
            idxWeight = 10000;
            foreach (var ks in ep.Value)
            {
                ks.Role.Weight ??= idxWeight++;
            }
        }

        // Sanitization
        foreach (var ks in template.KeyStaff)
        {
            ks.Role.Abbreviation = ks
                .Role.Abbreviation.Trim()
                .ToUpperInvariant()
                .Replace("$", string.Empty);
            ks.Role.Name = ks.Role.Name.Trim();
        }

        foreach (var ks in template.AdditionalStaff.Values.SelectMany(x => x))
        {
            ks.Role.Abbreviation = ks
                .Role.Abbreviation.Trim()
                .ToUpperInvariant()
                .Replace("$", string.Empty);
            ks.Role.Name = ks.Role.Name.Trim();
        }

        // Populate data
        var projectData = new Project
        {
            Id = Guid.NewGuid(),
            GuildId = guildId,
            Nickname = template.Nickname,
            Title = template.Title!,
            OwnerId = ownerId,
            Type = template.Type!.Value,
            PosterUri = template.PosterUri,
            UpdateChannelId = template.UpdateChannelId,
            ReleaseChannelId = template.ReleaseChannelId,
            IsPrivate = template.IsPrivate,
            IsArchived = false,
            AirReminderEnabled = false,
            CongaReminderEnabled = false,
            Administrators =
                template.AdministratorIds?.Select(i => new Administrator { UserId = i }).ToList()
                ?? [],
            KeyStaff = template
                .KeyStaff.Select(s => new Staff
                {
                    UserId = s.UserId,
                    IsPseudo = s.IsPseudo,
                    Role = s.Role,
                })
                .ToList(),
            CongaParticipants = CongaGraph.Deserialize(template.CongaParticipants ?? []),
            Aliases = template.Aliases?.Select(a => new Records.Alias { Value = a }).ToList() ?? [],
            AniListId = template.AniListId,
            Created = DateTimeOffset.UtcNow,
        };

        var episodes = new List<Episode>();
        for (var i = template.FirstEpisode; i < template.FirstEpisode + template.Length; i++)
        {
            var stringNumber = i.Value.ToString(CultureInfo.InvariantCulture);
            template.AdditionalStaff.TryGetValue(stringNumber, out var additionalStaff);
            episodes.Add(
                new Episode
                {
                    GuildId = guildId,
                    ProjectId = projectData.Id,
                    Number = stringNumber,
                    Done = false,
                    ReminderPosted = false,
                    AdditionalStaff =
                        additionalStaff
                            ?.Select(s => new Staff
                            {
                                UserId = s.UserId,
                                IsPseudo = s.IsPseudo,
                                Role = s.Role,
                            })
                            .ToList() ?? [],
                    PinchHitters = [],
                    Tasks = template
                        .KeyStaff.Concat(additionalStaff ?? [])
                        .Select(ks => new Task
                        {
                            Abbreviation = ks.Role.Abbreviation,
                            Done = false,
                        })
                        .ToList(),
                }
            );
        }

        Log.Info(
            $"Creating project {projectData} for M[{ownerId} (@{member.Username})] from JSON file '{file.Filename}' with {episodes.Count} episodes and {template.KeyStaff.Length} keystaff"
        );

        // Add project and episodes to database
        await db.Projects.AddAsync(projectData);
        await db.Episodes.AddRangeAsync(episodes);

        // Create configuration if the guild doesn't have one yet
        if (db.GetConfig(guildId) == null)
        {
            Log.Info($"Creating default configuration for guild {guildId}");
            await db.Configurations.AddAsync(Configuration.CreateDefault(guildId));
        }

        var builder = new StringBuilder();
        builder.AppendLine(T("project.created", lng, template.Nickname));

        if (template.FirstEpisode != 1)
        {
            builder.AppendLine();
            builder.AppendLine(T("project.created.firstEpisode", lng, template.FirstEpisode));
        }

        // Send success embed
        var embed = new EmbedBuilder()
            .WithTitle(T("title.projectCreation", lng))
            .WithDescription(builder.ToString())
            .Build();
        await interaction.FollowupAsync(embed: embed);

        // Check progress channel permissions
        if (!PermissionChecker.CheckPermissions(template.UpdateChannelId))
            await Response.Info(
                T("error.missingChannelPerms", lng, $"<#{template.UpdateChannelId}>"),
                interaction
            );
        if (!PermissionChecker.CheckReleasePermissions(template.ReleaseChannelId))
            await Response.Info(
                T("error.missingChannelPermsRelease", lng, $"<#{template.ReleaseChannelId}>"),
                interaction
            );

        // Inform about private project behavior
        if (projectData.IsPrivate)
            await Response.Info(T("info.publishPrivateProgress", lng), interaction);

        await db.TrySaveChangesAsync(interaction);
        return ExecutionResult.Success;
    }
}
