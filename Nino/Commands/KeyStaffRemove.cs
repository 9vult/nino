using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class KeyStaff
    {

        public static async Task<bool> HandleRemove(SocketSlashCommand interaction)
        {
            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);
            var lng = interaction.UserLocale;

            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();
            var abbreviation = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();

            // Verify project and user
            var project = await Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            if (!project.KeyStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng), interaction);

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

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.removed", lng, abbreviation))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }
    }
}
