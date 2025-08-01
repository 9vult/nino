﻿using System.Globalization;
using System.Net.Http.Json;
using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Localizer.Localizer;

namespace Nino.Commands;

public partial class ProjectManagement
{
    [SlashCommand("create-from-json", "Create a project using a json file")]
    public async Task<RuntimeResult> CreateFromJson (
        [Summary("file", "Project Template")] IAttachment file
    )
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;

        var guildId = interaction.GuildId ?? 0;
        var guild = Nino.Client.GetGuild(guildId);
        var member = guild.GetUser(interaction.User.Id);
        if (!Utils.VerifyAdministrator(member, guild)) return await Response.Fail(T("error.notPrivileged", lng), interaction);

        Log.Info($"Project creation from json file requested by M[{interaction.User.Id} (@{interaction.User.Username})]");
            
        // Parse the json
        ProjectTemplate? template;
        try
        {
            Log.Trace($"Attempting to get and parse JSON...");
            using var client = new HttpClient();
            template = await client.GetFromJsonAsync<ProjectTemplate>(file.Url, new JsonSerializerOptions
            {
                IncludeFields = true,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            if (template is null)
            {
                Log.Trace($"Project creation from json file failed (null)");
                return await Response.Fail(T("error.generic", lng), interaction);
            }

            template.FirstEpisode = template.FirstEpisode ?? 1;
            Log.Trace($"Getting and parsing JSON successful!");
        }
        catch (Exception e)
        {
            Log.Error(e);
            Log.Trace($"Project creation from json file failed");
            return await Response.Fail(e.Message, interaction);
        }
            
        var ownerId = interaction.User.Id;

        // Sanitize input
        template.Nickname = template.Nickname.Trim().ToLowerInvariant().Replace(" ", string.Empty); // remove spaces

        // Verify data
        if (Cache.GetProjects(guildId).Any(p => p.Nickname == template.Nickname))
            return await Response.Fail(T("error.project.nameInUse", lng, template.Nickname), interaction);

        if (!Uri.TryCreate(template.PosterUri, UriKind.Absolute, out var _))
            return await Response.Fail(T("error.project.invalidPosterUrl", lng), interaction);

        // Configure weights
        var idxWeight = 1;
        foreach (var ks in template.KeyStaff)
        {
            ks.Role.Weight ??= idxWeight++;
        }
        // Go through the additional staff dictionary
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
            ks.Role.Abbreviation = ks.Role.Abbreviation.Trim().ToUpperInvariant().Replace("$", string.Empty);
            ks.Role.Name = ks.Role.Name.Trim();
        }

        foreach (var ks in template.AdditionalStaff.Values.SelectMany(x => x))
        {
            ks.Role.Abbreviation = ks.Role.Abbreviation.Trim().ToUpperInvariant().Replace("$", string.Empty);
            ks.Role.Name = ks.Role.Name.Trim();
        }
            
        // Populate data
        var projectData = new Project
        {
            Id = AzureHelper.CreateProjectId(),
            GuildId = guildId,
            Nickname = template.Nickname,
            Title = template.Title,
            OwnerId = ownerId,
            Type = template.Type,
            PosterUri = template.PosterUri,
            UpdateChannelId = template.UpdateChannelId,
            ReleaseChannelId = template.ReleaseChannelId,
            IsPrivate = template.IsPrivate,
            IsArchived = false,
            AirReminderEnabled = false,
            CongaReminderEnabled = false,
            AdministratorIds = template.AdministratorIds ?? [],
            KeyStaff = template.KeyStaff,
            CongaParticipants = CongaGraph.Deserialize(template.CongaParticipants ?? []) ,
            Aliases = template.Aliases ?? [],
            AniListId = template.AniListId,
            Created = DateTimeOffset.UtcNow
        };
            
        var episodes = new List<Episode>();
        for (var i = template.FirstEpisode; i < template.FirstEpisode + template.Length; i++)
        {
            var stringNumber = i.Value.ToString(CultureInfo.InvariantCulture);
            template.AdditionalStaff.TryGetValue(stringNumber, out var additionalStaff);
            episodes.Add(new Episode
            {
                Id = AzureHelper.CreateEpisodeId(),
                GuildId = guildId,
                ProjectId = projectData.Id,
                Number = stringNumber,
                Done = false,
                ReminderPosted = false,
                AdditionalStaff = additionalStaff ?? [],
                PinchHitters = [],
                Tasks = template.KeyStaff.Concat(additionalStaff ?? [])
                    .Select(ks => new Records.Task { Abbreviation = ks.Role.Abbreviation, Done = false }).ToArray(),
            });
        }

        Log.Info($"Creating project {projectData} for M[{ownerId} (@{member.Username})] from JSON file '{file.Filename}' with {episodes.Count} episodes and {template.KeyStaff.Length} keystaff");

        // Add project and episodes to database
        await AzureHelper.Projects!.UpsertItemAsync(projectData);

        foreach (var chunk in episodes.Chunk(50))
        {
            var batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(projectData.Id.ToString()));
            foreach (var episode in chunk)
            {
                batch.UpsertItem(episode);
            }
            await batch.ExecuteAsync();
        }

        // Create configuration if the guild doesn't have one yet
        if (await Getters.GetConfiguration(guildId) == null)
        {
            Log.Info($"Creating default configuration for guild {guildId}");
            await AzureHelper.Configurations!.UpsertItemAsync(Configuration.CreateDefault(guildId));
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
            await Response.Info(T("error.missingChannelPerms", lng, $"<#{template.UpdateChannelId}>"), interaction);
        if (!PermissionChecker.CheckReleasePermissions(template.ReleaseChannelId))
            await Response.Info(T("error.missingChannelPermsRelease", lng, $"<#{template.ReleaseChannelId}>"), interaction);

        await Cache.RebuildCacheForGuild(interaction.GuildId ?? 0);
        return ExecutionResult.Success;
    }
}