using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class AirReminder
        {
            [SlashCommand("disable", "Disable airing reminders")]
            public async Task<RuntimeResult> Disable(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();

                // Verify project and user - Owner or Admin required
                var project = db.ResolveAlias(alias, interaction);
                if (project is null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                project.AirReminderEnabled = false;
                project.AirReminderChannelId = null;
                project.AirReminderRoleId = null;
                project.AirReminderUserId = null;

                Log.Info($"Disabled air reminders for {project}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.airreminder.disabled", lng, project.Nickname))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await db.TrySaveChangesAsync(interaction);
                return ExecutionResult.Success;
            }
        }
    }
}
