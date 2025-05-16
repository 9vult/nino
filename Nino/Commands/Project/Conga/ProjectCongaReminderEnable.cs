using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class CongaReminder
        {
            [SlashCommand("enable", "Enable conga reminders")]
            public async Task<RuntimeResult> Enable(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("days", "Reminder period"), MinValue(1), MaxValue(90)] int days,
                [Summary("channel", "Channel to post reminders in"), ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel channel
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                var channelId = channel.Id;
                var hours = days * 24;

                // Verify project and user - Owner or Admin required
                var project = Utils.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Set in database
                await AzureHelper.PatchProjectAsync(project, [
                    PatchOperation.Set($"/congaReminderEnabled", true),
                    PatchOperation.Set($"/congaReminderChannelId", channelId.ToString()),
                    PatchOperation.Set($"/congaReminderPeriod", TimeSpan.FromHours(hours))
                ]);

                Log.Info($"Enabled conga reminders for {project} with a period of {hours} hours.");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.congareminder.enabled", lng, project.Nickname, hours))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                // Check reminder channel permissions
                if (!PermissionChecker.CheckPermissions(channelId))
                    await Response.Info(T("error.missingChannelPerms", lng, $"<#{channelId}>"), interaction);

                await Cache.RebuildCacheForProject(project.Id);
                return ExecutionResult.Success;
            }
        }
    }
}
