using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class AdditionalStaff
    {
        public static async Task<bool> HandleRemove(SocketSlashCommand interaction, Episode episode)
        {
            var lng = interaction.UserLocale;

            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var abbreviation = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();

            // Check if position exists
            if (!episode.AdditionalStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            var asIndex = Array.IndexOf(episode.AdditionalStaff, episode.AdditionalStaff.Single(k => k.Role.Abbreviation == abbreviation));
            var taskIndex = Array.IndexOf(episode.Tasks, episode.Tasks.Single(t => t.Abbreviation == abbreviation));

            // Rewmove from database
            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(episode));
            batch.PatchItem(id: episode.Id, new[]
            {
                PatchOperation.Remove($"/additionalStaff/{asIndex}"),
                PatchOperation.Remove($"/tasks/{taskIndex}")
            });
            await batch.ExecuteAsync();

            log.Info($"Removed {abbreviation} from {episode.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("additionalStaff.removed", lng, abbreviation, episode.Number))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }
    }
}
