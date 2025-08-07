using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Handlers;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class Admin
        {
            [SlashCommand("remove", "Remove an administrator from this project")]
            public async Task<RuntimeResult> Remove(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("member", "Staff member")] SocketUser member
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                var memberId = member.Id;
                var staffMention = $"<@{memberId}>";

                // Verify project and user - Owner required
                var project = db.ResolveAlias(alias, interaction);
                if (project is null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Validate user is an admin
                var admin = project.Administrators.FirstOrDefault(a => a.UserId == memberId);
                if (admin is null)
                    return await Response.Fail(T("error.noSuchAdmin", lng, staffMention), interaction);

                project.Administrators.Remove(admin);

                Log.Info($"Removed M[{memberId} (@{member.Username})] as an admin from {project}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.admin.removed", lng, staffMention, project.Nickname))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await db.TrySaveChangesAsync(interaction);
                return ExecutionResult.Success;
            }
        }
    }
}
