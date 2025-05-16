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
                [Summary("link", "Link in the Conga graph"), Autocomplete(typeof(CongaNodesAutocompleteHandler))] string nodeText
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                
                // Verify node
                CongaEdge edge;
                try
                {
                    edge = CongaEdge.FromString(nodeText.Trim());
                }
                catch (Exception)
                {
                    return await Response.Fail(T("error.noSuchConga", lng), interaction);
                }

                // Verify project and user - Owner or Admin required
                var project = Utils.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Validate participant is in the conga line
                if (!project.CongaParticipants.Contains(edge.Current) 
                    || project.CongaParticipants.GetDependentsOf(edge.Current).All(c => c.Abbreviation != edge.Next))
                    return await Response.Fail(T("error.noSuchConga", lng), interaction);

                // Update database
                project.CongaParticipants.Remove(edge.Current, edge.Next);
                await AzureHelper.PatchProjectAsync(project, [
                    PatchOperation.Set($"/congaParticipants", project.CongaParticipants.Serialize()),
                ]);

                Log.Info($"Removed {edge} from the Conga line for {project}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.conga.removed", lng, edge.Current, edge.Next))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await Cache.RebuildCacheForProject(project.Id);
                return ExecutionResult.Success;
            }
        }
    }
}
