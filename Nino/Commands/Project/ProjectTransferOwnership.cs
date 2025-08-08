using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Handlers;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("transfer-ownership", "Transfer project ownership to someone else")]
        public async Task<RuntimeResult> Delete(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Summary("member", "Staff member")] SocketUser member
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Verify project and user - Owner required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!project.VerifyUser(db, interaction.User.Id, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Get inputs
            var memberId = member.Id;

            // Swap in database
            project.OwnerId = memberId;

            Log.Info($"Transfered project ownership of {project} to M[{memberId} (@{member.Username})]");

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("project.owner.transferred", lng, staffMention, project.Nickname))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
