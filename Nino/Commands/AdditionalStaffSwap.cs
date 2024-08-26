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
        public static async Task<bool> HandleSwap(SocketSlashCommand interaction, Episode episode)
        {
            var lng = interaction.UserLocale;

            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var abbreviation = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();
            var memberId = ((SocketGuildUser)subcommand.Options.FirstOrDefault(o => o.Name == "member")!.Value).Id;

            // Check if position exists
            if (!episode.AdditionalStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng), interaction);

            // Update user
            var updatedStaff = episode.AdditionalStaff.Single(k => k.Role.Abbreviation == abbreviation);
            var asIndex = Array.IndexOf(episode.AdditionalStaff, updatedStaff);

            updatedStaff.UserId = memberId;

            // Swap in database
            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(episode));
            batch.PatchItem(id: episode.Id, new[]
            {
                PatchOperation.Replace($"/additionalStaff/{asIndex}", updatedStaff)
            });
            await batch.ExecuteAsync();

            log.Info($"Swapped {memberId} in to {episode.Id} for {abbreviation}");

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("additionalStaff.swapped", lng, staffMention, abbreviation, episode.Number))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }
    }
}
