// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Core.Features.Commands.KeyStaff.Add;
using Nino.Domain.ValueObjects;
using Nino.Localization;

namespace Nino.Discord.Interactions.KeyStaff;

public partial class KeyStaffModule
{
    [ComponentInteraction("nino.keyStaff.create.markDone.yes:*", ignoreGroupNames: true)]
    public async Task<RuntimeResult> CreateAndMarkDoneAsync(string rawId)
    {
        var stateId = StateId.From(rawId);
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var command = await stateService.LoadStateAsync<AddKeyStaffCommand>(stateId);
        if (command is null)
            return await interaction.FailAsync(T("error.db", locale));

        // Verify button is not being hijacked
        if (
            await identityService.GetOrCreateUserByDiscordIdAsync(
                interaction.User.Id,
                interaction.User.Username
            ) != command.RequestedBy
        )
            return await interaction.FailAsync(T("error.hijack", locale), ephemeral: true);

        // Delete state, is no longer needed
        await stateService.DeleteStateAsync(stateId);

        // Remove the buttons
        await interaction.ModifyOriginalResponseAsync(m => m.Components = null);

        // Make the request
        var result = await addHandler.HandleAsync(command with { MarkDoneForDoneEpisodes = true });

        if (!result.IsSuccess)
        {
            Dictionary<string, object> errorParams = new()
            {
                ["abbreviation"] = command.Abbreviation,
            };
            return await interaction.FailAsync(
                T(
                    result.Status switch
                    {
                        ResultStatus.Unauthorized => "error.permissions",
                        ResultStatus.Conflict => "keyStaff.creation.conflict",
                        _ => "error.generic",
                    },
                    locale,
                    errorParams
                )
            );
        }

        // Success!
        var header =
            $"{result.Value.ProjectTitle} ({result.Value.ProjectType.ToFriendlyString(locale)})";
        var memberId = await identityService.GetDiscordUserIdAsync(command.MemberId);
        var staffMention = $"<@{memberId}>";

        var successEmbed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(
                T("keyStaff.creation.success", locale, staffMention, command.Abbreviation)
            )
            .Build();

        await interaction.ModifyOriginalResponseAsync(m => m.Embed = successEmbed);
        return ExecutionResult.Success;
    }
}
