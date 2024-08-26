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
        public static async Task<bool> HandleCongaRemove(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First().Options.First();

            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();

            // Verify project and user - Owner or Admin required
            var project = await Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Get inputs
            var abbreviation = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();

            // Validate participant is in the conga line
            if (!project.CongaParticipants.Any(c => c.Current == abbreviation))
                return await Response.Fail(T("error.noSuchConga", lng, abbreviation), interaction);

            // Remove from database
            var cIndex = Array.IndexOf(project.CongaParticipants, project.CongaParticipants.Single(c => c.Current == abbreviation));
            await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: new[]
            {
                PatchOperation.Remove($"/congaParticipants/{cIndex}")
            });

            log.Info($"Removed {abbreviation} from the Conga line for {project.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("project.conga.removed", lng, abbreviation))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }
    }
}
