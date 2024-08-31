using Discord;
using Discord.Interactions;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class Conga
        {
            [SlashCommand("list", "List all the Conga line participants")]
            public async Task<bool> Remove(
                [Summary("project", "Project nickname")] string alias
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();

                // Verify project and user - Owner or Admin required
                var project = Utils.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Process
                var message = string.Empty;

                if (project.CongaParticipants.Length == 0)
                    message = T("project.conga.empty", lng);
                else
                    message = string.Join(Environment.NewLine, project.CongaParticipants.Select(c => $"{c.Current} → {c.Next}"));

                // Send embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.congaList", lng))
                    .WithDescription(message)
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                return true;
            }
        }
    }
}
