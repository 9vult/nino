using Discord.Interactions;
using Nino.Handlers;
using Nino.Services;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("export", "Export a project to JSON")]
        public async Task<RuntimeResult> Export(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("prettyprint", "Pretty-print?")] bool prettyPrint = true
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Verify project and user - Owner required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            log.Info($"Exporting project {project.Id}");

            // Get stream
            var file = await ExportService.ExportProject(project, prettyPrint);

            // Respond
            await interaction.FollowupWithFileAsync(file, $"{project.Id}.json", T("project.exported", lng, project.Nickname));
            
            return ExecutionResult.Success;
        }
    }
}
