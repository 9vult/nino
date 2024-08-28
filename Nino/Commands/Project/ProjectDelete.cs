using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class ProjectManagement
    {
        public static async Task<bool> HandleDelete(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First();

            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();

            // Verify project and user - Owner required
            var project = await Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            log.Info($"Deleting project {project.Id}");

            // Remove from database
            await AzureHelper.Projects!.DeleteItemAsync<Project>(project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project));

            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: new PartitionKey(project.Id));
            foreach (Episode e in await Getters.GetEpisodes(project))
            {
                batch.DeleteItem(e.Id);
            }
            await batch.ExecuteAsync();

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectDeletion", lng))
                .WithDescription(T("project.deleted", lng, project.Title))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildCacheForGuild(interaction.GuildId ?? 0);
            return true;
        }
    }
}
