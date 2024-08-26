using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class KeyStaff
    {
        public static async Task<bool> HandleSwap(SocketSlashCommand interaction, Project project)
        {
            var guild = Nino.Client.GetGuild(interaction.GuildId ?? 0);
            var lng = interaction.UserLocale;

            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var abbreviation = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();
            var memberId = ((SocketGuildUser)subcommand.Options.FirstOrDefault(o => o.Name == "member")!.Value).Id;

            // Check if position exists
            if (!project.KeyStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Update user
            var updatedStaff = project.KeyStaff.Single(k => k.Role.Abbreviation == abbreviation);
            var ksIndex = Array.IndexOf(project.KeyStaff, updatedStaff);

            updatedStaff.UserId = memberId;

            // Swap in database
            await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: new[]
            {
                PatchOperation.Replace($"/keyStaff/{ksIndex}", updatedStaff)
            });

            log.Info($"Swapped {memberId} in to {project.Id} for {abbreviation}");

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.swapped", lng, staffMention, abbreviation))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }
    }
}
