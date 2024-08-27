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
        public static async Task<bool> HandleRemove(SocketSlashCommand interaction, Project project)
        {
            var guild = Nino.Client.GetGuild(interaction.GuildId ?? 0);
            var lng = interaction.UserLocale;

            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var abbreviation = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();

            // Check if position exists
            if (!project.KeyStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Remove from database
            var ksIndex = Array.IndexOf(project.KeyStaff, project.KeyStaff.Single(k => k.Role.Abbreviation == abbreviation));
            await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: new[]
            {
                PatchOperation.Remove($"/keyStaff/{ksIndex}")
            });

            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(project));
            foreach (Episode e in await Getters.GetEpisodes(project))
            {
                var taskIndex = Array.IndexOf(e.Tasks, e.Tasks.Single(t => t.Abbreviation == abbreviation));
                batch.PatchItem(id: e.Id, new[]
                {
                    PatchOperation.Remove($"/tasks/{taskIndex}")
                });
            }
            await batch.ExecuteAsync();

            log.Info($"Removed {abbreviation} from {project.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.removed", lng, abbreviation))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);
            return true;
        }
    }
}
