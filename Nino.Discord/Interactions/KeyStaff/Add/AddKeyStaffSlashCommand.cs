// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Enums;
using Nino.Core.Features.KeyStaff.Add;
using Nino.Core.Features.Project.Resolve;

namespace Nino.Discord.Interactions.KeyStaff;

public partial class KeyStaffModule
{
    [SlashCommand("add", "Add a new Key Staff to the whole project")]
    public async Task<RuntimeResult> AddAsync(
        string alias,
        SocketUser member,
        [MaxLength(16)] string abbreviation,
        [MaxLength(32)] string fullName,
        bool isPseudo = false
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Resolve user, group, and project
        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var (resolveStatus, projectId) = await projectResolver.HandleAsync(
            new ResolveProjectQuery(alias, groupId, requestedBy)
        );

        if (resolveStatus is not ResultStatus.Success)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var memberId = await identityService.GetOrCreateUserByDiscordIdAsync(
            member.Id,
            member.Username
        );

        var commandDto = new AddKeyStaffCommand(
            projectId,
            memberId,
            abbreviation,
            fullName,
            isPseudo,
            MarkDoneForDoneEpisodes: false,
            requestedBy
        );

        var data = await dataService.GetProjectBasicInfoAsync(projectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        var (completedEpisodeCount, _) = await dataService.GetProjectCompletionStatusAsync(
            projectId
        );

        // There's some completed episodes, so we need direction from the user
        if (completedEpisodeCount > 0)
        {
            // Verify user
            var isVerified = await verificationService.VerifyProjectPermissionsAsync(
                projectId,
                requestedBy,
                PermissionsLevel.Administrator
            );
            if (!isVerified)
                return await interaction.FailAsync(T("error.permissions", locale));

            // Save the command state
            var stateId = await stateService.SaveStateAsync(commandDto);

            // Send question embed
            logger.LogTrace(
                "Displaying Mark Done if Episode is Done prompt to {UserId}",
                requestedBy
            );

            var questionEmbed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle(T("project.modification.title", locale))
                .WithDescription(T("keyStaff.creation.markDone.question", locale))
                .WithCurrentTimestamp()
                .Build();

            var noId = $"nino:keyStaff:create:markDone:no:{stateId}";
            var yesId = $"nino:keyStaff:create:markDone:yes:{stateId}";

            var component = new ComponentBuilder()
                .WithButton(T("button.no", locale), noId, ButtonStyle.Secondary)
                .WithButton(T("button.yes", locale), yesId, ButtonStyle.Secondary)
                .Build();

            await interaction.ModifyOriginalResponseAsync(m =>
            {
                m.Embed = questionEmbed;
                m.Components = component;
            });

            return ExecutionResult.Success;
        }

        // No completed episodes, go forward
        var result = await addHandler.HandleAsync(commandDto);

        if (result.Status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("error.projectNotFound", locale),
                    ResultStatus.Conflict => T("keyStaff.creation.conflict", locale, abbreviation),
                    _ => T("error.generic", locale),
                }
            );
        }

        var staffMention = $"<@{member.Id}>";
        var embed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T("keyStaff.creation.success", locale, staffMention, abbreviation))
            .Build();

        await interaction.FollowupAsync(embed: embed);
        return ExecutionResult.Success;
    }
}
