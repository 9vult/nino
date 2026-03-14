// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.KeyStaff.Add;
using Nino.Core.Features.Queries.Project.Resolve;
using Nino.Core.Features.Queries.Project.Status;
using Nino.Domain;
using Nino.Domain.Enums;
using Nino.Localization;

namespace Nino.Discord.Interactions.KeyStaff;

public partial class KeyStaffModule
{
    [SlashCommand("add", "Add a Key Staff to the project")]
    public async Task<RuntimeResult> AddAsync(
        [MaxLength(Length.Alias)] string alias,
        SocketUser member,
        [MaxLength(Length.Abbreviation)] string abbreviation,
        [MaxLength(Length.RoleName)] string fullName,
        bool isPseudo = false
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        abbreviation = abbreviation.Trim().ToUpperInvariant();
        fullName = fullName.Trim();

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);
        var resolved = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .BindAsync(id => projectStatusHandler.HandleAsync(new ProjectStatusQuery(id)));
        if (!resolved.IsSuccess)
            return await interaction.FailAsync(T("project.resolution.failed", locale, alias));

        var (projectId, projectTitle, projectType, _, completedEpisodeCount) = resolved.Value;

        var header = $"{projectTitle} ({projectType.ToFriendlyString(locale)})";

        var memberId = await identityService.GetOrCreateUserByDiscordIdAsync(
            member.Id,
            member.Username
        );

        var command = new AddKeyStaffCommand(
            ProjectId: projectId,
            RequestedBy: requestedBy,
            Abbreviation: abbreviation,
            Name: fullName,
            MemberId: memberId,
            IsPseudo: isPseudo,
            MarkDoneForDoneEpisodes: false
        );

        // Some episodes are completed, so we need direction from the user
        if (completedEpisodeCount > 0)
        {
            // Verify the user
            if (
                !await userVerificationService.VerifyProjectPermissionsAsync(
                    projectId,
                    requestedBy,
                    PermissionsLevel.Administrator
                )
            )
                return await interaction.FailAsync("error.permissions");

            logger.LogTrace(
                "Displaying 'Mark Done if Episode is Done' prompt to user {UserId} for project {ProjectId}",
                requestedBy,
                projectId
            );

            // Save command to state
            var stateId = await stateService.SaveStateAsync(command);

            // Create question embed
            var questionEmbed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle(T("action.question", locale))
                .WithDescription(T("keyStaff.creation.markDone.question", locale))
                .WithCurrentTimestamp()
                .Build();

            var noId = $"nino.keyStaff.create.markDone.no:{stateId}";
            var yesId = $"nino.keyStaff.create.markDone.yes:{stateId}";

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

        // No completed episodes, proceed normally
        var result = await addHandler.HandleAsync(command);

        if (!result.IsSuccess)
        {
            Dictionary<string, object> errorParams = new() { ["abbreviation"] = abbreviation };
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => "error.permissions",
                    ResultStatus.Conflict => "keyStaff.creation.conflict",
                    _ => "error.generic",
                },
                locale,
                errorParams
            );
        }

        // Success!
        var staffMention = $"<@{member.Id}>";
        var successEmbed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T("keyStaff.creation.success", locale, staffMention, abbreviation))
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
