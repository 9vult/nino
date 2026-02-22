// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Enums;
using Nino.Core.Features.KeyStaff.Add;

namespace Nino.Discord.Interactions.KeyStaff;

public partial class KeyStaffModule
{
    [ComponentInteraction("nino.keyStaff.create.markDone.no:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> CreateAndDoNotMarkDoneAsync(Guid stateId)
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var commandDto = await stateService.LoadStateAsync<AddKeyStaffCommand>(stateId);
        if (commandDto is null)
            return await interaction.FailAsync(T("error.db", locale));

        // Verify button was clicked by initiator
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != commandDto.RequestedBy
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        // Delete state, won't be needed regardless of the final status
        await stateService.DeleteStateAsync(stateId);

        var data = await dataService.GetProjectBasicInfoAsync(commandDto.ProjectId);
        var header = $"{data.Title} ({data.Type.ToFriendlyString(locale)})";

        // Update embed
        var responseEmbed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(T("keyStaff.creation.markDone.response.no", locale))
            .WithCurrentTimestamp()
            .Build();

        await interaction.ModifyOriginalResponseAsync(m =>
        {
            m.Embed = responseEmbed;
            m.Components = null;
        });

        // Make request
        var result = await addHandler.HandleAsync(
            commandDto with
            {
                MarkDoneForDoneEpisodes = false,
            }
        );

        if (result.Status is not ResultStatus.Success)
        {
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => T("error.permissions", locale),
                    ResultStatus.NotFound => T("error.projectNotFound", locale),
                    ResultStatus.Conflict => T(
                        "keyStaff.creation.conflict",
                        locale,
                        commandDto.Abbreviation
                    ),
                    _ => T("error.generic", locale),
                }
            );
        }

        var mentionId = await identityService.GetDiscordUserIdAsync(commandDto.UserId);

        var staffMention = $"<@{mentionId}>";
        var resultEmbed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(
                T("keyStaff.creation.success", locale, staffMention, commandDto.Abbreviation)
            )
            .Build();

        await interaction.FollowupAsync(embed: resultEmbed);
        return ExecutionResult.Success;
    }
}
