// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features.Commands.Observers.Remove;
using Nino.Discord.Entities;
using Nino.Discord.Handlers.AutocompleteHandlers;
using Nino.Domain.ValueObjects;

namespace Nino.Discord.Interactions.Observers;

public partial class ObserverModule
{
    [SlashCommand("remove", "Remove an observer from a project")]
    public async Task<RuntimeResult> RemoveAsync(
        [Summary(name: "observer"), Autocomplete(typeof(ObserverAutocompleteHandler))]
            string rawObserverId
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        rawObserverId = rawObserverId.Trim();

        if (!ObserverId.TryParse(rawObserverId, out var observerId))
            return await interaction.FailAsync(
                T("observer.unknownObserver", locale, rawObserverId)
            );

        var (requestedBy, _) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var guild = client.GetGuild(interaction.GuildId!.Value);
        var member = guild.GetUser(interaction.User.Id);
        var isDiscordAdmin = member.GuildPermissions.Administrator;

        var command = new RemoveObserverCommand(observerId, requestedBy, isDiscordAdmin);

        var result = await removeHandler.HandleAsync(command);

        if (!result.IsSuccess)
        {
            return await interaction.FailAsync(result.Status, locale, new FailureContext());
        }

        // Success!
        var successEmbed = new EmbedBuilder()
            .WithTitle(T("observer.title", locale))
            .WithDescription(T("observer.delete.success", locale))
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
