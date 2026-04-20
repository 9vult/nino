// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.Groups.Admins.Remove;
using Nino.Core.Features.Queries.Groups.GetGenericData;
using Nino.Discord.Entities;

namespace Nino.Discord.Interactions.Groups;

public partial class GroupModule
{
    public partial class AdminModule
    {
        [SlashCommand("remove", "Remove an admin from a group")]
        public async Task<RuntimeResult> RemoveAsync(SocketUser user)
        {
            var interaction = Context.Interaction;
            var locale = interaction.UserLocale;

            var guild = client.GetGuild(interaction.GuildId!.Value);
            var requestee = guild.GetUser(interaction.User.Id);

            var userId = await identityService.GetOrCreateUserByDiscordIdAsync(
                user.Id,
                user.Username
            );

            var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(
                interaction
            );
            var isDiscordAdmin = requestee.GuildPermissions.Administrator;

            var command = new RemoveGroupAdminCommand(
                GroupId: groupId,
                UserId: userId,
                RequestedBy: requestedBy,
                OverrideVerification: isDiscordAdmin
            );

            var result = await removeAdminHandler
                .HandleAsync(command)
                .BindAsync(() =>
                    getGenericDataHandler.HandleAsync(new GetGenericGroupDataQuery(groupId))
                );

            if (!result.IsSuccess)
            {
                return await interaction.FailAsync(
                    result.Status,
                    locale,
                    new FailureContext
                    {
                        Overrides = new Dictionary<ResultStatus, string>
                        {
                            [ResultStatus.BadRequest] = "admin.remove.badRequest",
                        },
                    }
                );
            }

            var gData = result.Value;

            // Success!
            var staffMention = $"<@{user.Id}>";
            var successEmbed = new EmbedBuilder()
                .WithAuthor(gData.GroupName)
                .WithTitle(T("group.configuration.title", locale))
                .WithDescription(T("admin.remove.success", locale, staffMention))
                .WithCurrentTimestamp()
                .Build();

            await interaction.FollowupAsync(embed: successEmbed);
            return ExecutionResult.Success;
        }
    }
}
