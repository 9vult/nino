// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Discord.Interactions;

namespace Nino.Discord.Interactions.Debug;

public partial class DebugModule
{
    [SlashCommand("validate-channel", "Validate that Nino can use a Channel")]
    public async Task<RuntimeResult> ValidateChannelAsync(
        [ChannelTypes(ChannelType.Text, ChannelType.News)] IMessageChannel channel
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        var perms = botPermissionsService.GetChannelPermissions(channel.Id);

        var body = new StringBuilder();

        body.AppendLine($"<#{channel.Id}>");

        if (perms is null)
        {
            body.AppendLine(T("nino.debug.invalidChanel", locale));
        }
        else
        {
            var passFail = new Dictionary<bool, string> { [true] = "✅ ", [false] = "❌ " };
            var passWarn = new Dictionary<bool, string> { [true] = "✅ ", [false] = "⚠️ " };
            var p = perms.Value;

            body.AppendLine(passFail[p.ViewChannel] + T("nino.debug.channel.view", locale));
            body.AppendLine(passFail[p.SendMessages] + T("nino.debug.channel.send", locale));
            body.AppendLine(passFail[p.EmbedLinks] + T("nino.debug.channel.embed", locale));
            body.AppendLine(passFail[p.MentionEveryone] + T("nino.debug.channel.mention", locale));
            body.AppendLine(
                passWarn[channel.ChannelType is ChannelType.News]
                    + T("nino.debug.channel.crosspost", locale)
            );
        }

        await interaction.FollowupAsync(
            embed: new EmbedBuilder()
                .WithAuthor(name: "Nino", url: "https://github.com/9vult/nino")
                .WithThumbnailUrl("https://files.catbox.moe/j3qizm.png")
                .WithTitle(T("nino.debug.channel.title", locale))
                .WithDescription(body.ToString())
                .WithCurrentTimestamp()
                .Build()
        );
        return ExecutionResult.Success;
    }
}
