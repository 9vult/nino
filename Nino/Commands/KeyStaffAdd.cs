using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Utilities;
using Nino.Records;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Localizer.Localizer;
using System.Collections;

namespace Nino.Commands
{
    internal static partial class KeyStaff
    {

        public static async Task<bool> HandleAdd(SocketSlashCommand interaction)
        {
            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);
            var lng = interaction.UserLocale;

            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();
            var memberId = ((SocketGuildUser)subcommand.Options.FirstOrDefault(o => o.Name == "member")!.Value).Id;
            var abbreviation = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();
            var title = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "name")).Trim();

            // Verify project and user
            var project = await Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            if (project.KeyStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.positionExists", lng), interaction);

            // All good!
            var newStaff = new Staff
            {
                Id = $"{project.Id}-{abbreviation}",
                UserId = memberId,
                Role = new Role
                {
                    Abbreviation = abbreviation,
                    Name = title
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

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("keyStaff.added", lng, staffMention, abbreviation))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }
    }
}
