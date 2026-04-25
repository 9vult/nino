// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Core.Services;

namespace Nino.Discord.Interactions.About;

public class AboutSlashCommand(VersionService versionService)
    : InteractionModuleBase<IInteractionContext>
{
    [SlashCommand("about", "About Nino")]
    public async Task<RuntimeResult> ShowAboutAsync()
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var body = new StringBuilder();
        body.AppendLine(T("nino.about.version", locale, versionService.FullLabel));
        body.AppendLine(T("nino.about.author", locale, "<@248600185423396866>"));

        await interaction.FollowupAsync(
            embed: new EmbedBuilder()
                .WithAuthor(name: "Nino", url: "https://github.com/9vult/nino")
                .WithThumbnailUrl("https://files.catbox.moe/j3qizm.png")
                .WithDescription(body.ToString())
                .WithCurrentTimestamp()
                .Build()
        );
        return ExecutionResult.Success;
    }
}
