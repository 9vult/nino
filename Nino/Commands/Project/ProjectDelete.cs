using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Services;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("delete", "Delete a project")]
        public async Task<RuntimeResult> Delete(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Verify project and user - Owner required
            var project = db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);
            
            // Ask if the user is sure
            var (goOn, finalBody) = await Ask.AboutIrreversibleAction(interactive, interaction, project, lng,
                Ask.IrreversibleAction.Delete);

            if (!goOn)
            {
                var cancelEmbed = new EmbedBuilder()
                    .WithTitle(T("title.projectDeletion", lng))
                    .WithDescription(finalBody)
                    .Build();
                await interaction.ModifyOriginalResponseAsync(m => {
                    m.Embed = cancelEmbed;
                    m.Components = null;
                });
                return ExecutionResult.Success;
            }
            
            Log.Info($"Exporting project {project} before deletion");

            // Get stream
            var file = ExportService.ExportProject(project, false);

            // Respond
            await interaction.FollowupWithFileAsync(file, $"{project.Id}.json", T("project.exported", lng, project.Nickname));

            Log.Info($"Deleting project {project}");
            
            db.Projects.Remove(project); // Removes episodes and observers

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectDeletion", lng))
                .WithDescription(T("project.deleted", lng, project.Title))
                .Build();
            await interaction.ModifyOriginalResponseAsync(m => {
                m.Embed = embed;
                m.Components = null;
            });

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
