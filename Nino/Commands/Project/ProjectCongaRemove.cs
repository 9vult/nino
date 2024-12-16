using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class Conga
        {
            [SlashCommand("remove", "Remove a link from the Conga line")]
            public async Task<RuntimeResult> Remove(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string current,
                [Summary("next", "Position to ping"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string next
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                current = current.Trim().ToUpperInvariant();
                next = next.Trim().ToUpperInvariant();

                // Verify project and user - Owner or Admin required
                var project = Utils.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Validate participant is in the conga line
                if (!project.CongaParticipants.Any(c => c.Current == current && c.Next == next))
                    return await Response.Fail(T("error.noSuchConga", lng, current), interaction);

                // Remove from database
                var cIndex = Array.IndexOf(project.CongaParticipants, project.CongaParticipants.Single(c => c.Current == current && c.Next == next));
                await AzureHelper.PatchProjectAsync(project, [
                    PatchOperation.Remove($"/congaParticipants/{cIndex}")
                ]);

                Log.Info($"Removed {current} → {next} from the Conga line for {project}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.conga.removed", lng, current, next))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await Cache.RebuildCacheForProject(project.Id);
                return ExecutionResult.Success;
            }
        }
    }
}
