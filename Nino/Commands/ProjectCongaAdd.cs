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
        public static async Task<bool> HandleCongaAdd(SocketSlashCommand interaction)
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
            var current = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();
            var next = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "next")!.Value).Trim().ToUpperInvariant();

            // Validate tasks aren't already in the conga line
            if (project.CongaParticipants.Any(c => c.Current == current))
                return await Response.Fail(T("error.conga.alreadyExists", lng, current), interaction);
            if (project.CongaParticipants.Any(c => c.Next == next))
                return await Response.Fail(T("error.conga.alreadyExists", lng, next), interaction);

            // Validate tasks exist
            if (!project.KeyStaff.Any(ks => ks.Role.Abbreviation == current))
                return await Response.Fail(T("error.noSuchTask", lng, current), interaction);
            if (!project.KeyStaff.Any(ks => ks.Role.Abbreviation == next))
                return await Response.Fail(T("error.noSuchTask", lng, next), interaction);

            // We good!
            var participant = new CongaParticipant
            {
                Current = current,
                Next = next
            };

            // Add to database
            await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: new[]
            {
                PatchOperation.Add("/congaParticipants/-", participant)
            });

            log.Info($"Added {current} → {next} to the Conga line for {project.Id}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("project.conga.added", lng, current, next))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }
    }
}
