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
        public static async Task<bool> HandleAdd(SocketSlashCommand interaction, Project project)
        {
            var guild = Nino.Client.GetGuild(interaction.GuildId ?? 0);
            var lng = interaction.UserLocale;

            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var memberId = ((SocketGuildUser)subcommand.Options.FirstOrDefault(o => o.Name == "member")!.Value).Id;
            var abbreviation = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();
            var title = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "name")).Trim();

            // Check if position already exists
            if (project.KeyStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.positionExists", lng), interaction);

            // All good!
            var newStaff = new Staff
            {
                UserId = memberId,
                Role = new Role
                {
                    Abbreviation = abbreviation,
                    Name = title,
                    Weight = project.KeyStaff.Max(ks => ks.Role.Weight) ?? 0 + 1
                }
            };

            var newTask = new Records.Task
            {
                Abbreviation = abbreviation,
                Done = false
            };

            // Add to database
            await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: new[]
            {
                PatchOperation.Add("/keyStaff/-", newStaff)
            });

            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(project));
            foreach (Episode e in await Getters.GetEpisodes(project))
            {
                batch.PatchItem(id: e.Id, new[]
                {
                    PatchOperation.Add("/tasks/-", newTask)
                });
            }
            await batch.ExecuteAsync();

            log.Info($"Added {memberId} to {project.Id} for {abbreviation}");

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("keyStaff.added", lng, staffMention, abbreviation))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForProject(project.Id);
            return true;
        }
    }
}
