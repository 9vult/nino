using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class KeyStaff
    {
        public partial class PinchHitterManagement
        {
            [SlashCommand("set", "Set a pinch hitter for an episode")]
            public async Task<RuntimeResult> Set(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] decimal episodeNumber,
                [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string abbreviation,
                [Summary("member", "Staff member")] SocketUser member
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                var memberId = member.Id;
                alias = alias.Trim();
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
                
                // Check if position exists
                if (project.KeyStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                    return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

                // All good!
                var hitter = new PinchHitter
                {
                    UserId = memberId,
                    Abbreviation = abbreviation
                };

                // Add to database
                TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(episode));
                
                var phIndex = Array.IndexOf(episode.PinchHitters, episode.PinchHitters.SingleOrDefault(k => k.Abbreviation == abbreviation));
                batch.PatchItem(id: episode.Id, [
                    PatchOperation.Set($"/pinchHitters/{(phIndex != -1 ? phIndex : "-")}", hitter)
                ]);
                
                await batch.ExecuteAsync();

                log.Info($"Set {memberId} as pinch hitter for {abbreviation} for {episode.Id}");

                // Send success embed
                var staffMention = $"<@{memberId}>";
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("keyStaff.pinchHitter.set", lng, staffMention, episode.Number, abbreviation))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await Cache.RebuildCacheForProject(episode.ProjectId);
                return ExecutionResult.Success;
            }
        }
    }
}
