using Discord.Interactions;
using Nino.Handlers;
using Nino.Services;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("export", "Export a project to JSON")]
        public async Task<RuntimeResult> Export(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            bool prettyPrint = true
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Verify project and user - Owner required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            Log.Info($"Exporting project {project}");

            // Get stream
            var file = ExportService.ExportProject(project, prettyPrint);

            // Respond
            await interaction.FollowupWithFileAsync(
                file,
                $"{project.Id}.json",
                T("project.exported", lng, project.Nickname)
            );

            return ExecutionResult.Success;
        }
    }
}
