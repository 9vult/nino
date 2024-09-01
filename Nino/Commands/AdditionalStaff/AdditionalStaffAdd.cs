using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class AdditionalStaff
    {
        [SlashCommand("add", "Add additional staff to an episode")]
        public async Task<RuntimeResult> Add(
            [Summary("project", "Project nickname")] string alias,
            [Summary("episode", "Episode number")] decimal episodeNumber,
            [Summary("member", "Staff member")] SocketUser member,
            [Summary("abbreviation", "Position shorthand")] string abbreviation,
            [Summary("name", "Full position name")] string taskName
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize imputs
            var memberId = member.Id;
            alias = alias.Trim();
            taskName = taskName.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            var episode = await Getters.GetEpisode(project, episodeNumber);
            if (episode == null)
                return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

            // Check if position already exists
            if (episode.AdditionalStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.positionExists", lng), interaction);

            // All good!
            var newStaff = new Staff
            {
                UserId = memberId,
                Role = new Role
                {
                    Abbreviation = abbreviation,
                    Name = taskName
                }
            };

            var newTask = new Records.Task
            {
                Abbreviation = abbreviation,
                Done = false
            };

            // Add to database
            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(episode));
            batch.PatchItem(id: episode.Id, [
                PatchOperation.Add("/additionalStaff/-", newStaff),
                PatchOperation.Add("/tasks/-", newTask)
            ]);
            await batch.ExecuteAsync();

            log.Info($"Added {memberId} to {episode.Id} for {abbreviation}");

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("additionalStaff.added", lng, staffMention, abbreviation, episode.Number))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(episode.ProjectId);
            return ExecutionResult.Success;
        }
    }
}
