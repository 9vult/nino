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
        public partial class AirReminder
        {
            [SlashCommand("enable", "Enable airing reminders")]
            public async Task<RuntimeResult> Enable(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("channel", "Channel to post reminders in"), ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel channel,
                [Summary("role", "Role to ping for reminders")] SocketRole? role = null,
                [Summary("member", "Member to ping for reminders")] SocketUser? member = null
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                var channelId = channel.Id;
                var roleId = role?.Id;
                var memberId = member?.Id;

                // Verify project and user - Owner or Admin required
                var project = await db.ResolveAlias(alias, interaction);
                if (project is null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!project.VerifyUser(db, interaction.User.Id))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                project.AirReminderEnabled = true;
                project.AirReminderChannelId = channelId;
                project.AirReminderRoleId = roleId;
                project.AirReminderUserId = memberId;
                
                Log.Info($"Enabled air reminders for {project}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.airreminder.enabled", lng, project.Nickname))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                // Check reminder channel permissions
                if (!PermissionChecker.CheckPermissions(channelId))
                    await Response.Info(T("error.missingChannelPerms", lng, $"<#{channelId}>"), interaction);

                await db.TrySaveChangesAsync(interaction);
                return ExecutionResult.Success;
            }
        }
    }
}
