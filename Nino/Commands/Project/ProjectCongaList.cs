using Discord;
using Discord.WebSocket;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class ProjectManagement
    {
        public static async Task<bool> HandleCongaList(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;
            var subcommand = interaction.Data.Options.First().Options.First();

            var alias = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();

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
