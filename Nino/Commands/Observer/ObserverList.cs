using Discord;
using Discord.Interactions;
using Nino.Utilities;
using System.Text;
using Tababular;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Observer
    {
        [SlashCommand("list", "List the projects being observed by this server")]
        public async Task<RuntimeResult> List()
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var guildId = interaction.GuildId ?? 0;

            // Check for guild administrator status
            var guild = Nino.Client.GetGuild(guildId);
            var member = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(member, guild)) return await Response.Fail(T("error.notPrivileged", lng), interaction);
            
            Log.Trace($"Listing observers for {guildId}");

            // Get observers
            var observers = Cache.GetObservers().Where(o => o.GuildId == guildId).ToList();

            if (observers.Count == 0)
                return await Response.Fail(T("error.noObservers", lng), interaction);

            // End the interaction
            if (!PermissionChecker.CheckPermissions(interaction.Channel.Id))
                await Response.Info(T("error.missingChannelPerms", lng, $"<#{interaction.Channel.Id}>"), interaction);
            await interaction.FollowupAsync(T("observer.list.response", lng));
            
            var projects = Cache.GetProjects();

            var tblData = observers.Select(o => new Dictionary<string, string>
            {
                [T("observer.list.server", lng)] = o.OriginGuildId.ToString(),
                [T("observer.list.project", lng)] = projects.FirstOrDefault(p => p.Id == o.ProjectId)?.Nickname ?? "Unknown",
                [T("observer.list.blame", lng)] = o.Blame ? T("observer.list.yes", lng) : T("observer.list.no", lng),
                [T("observer.list.updates", lng)] = !string.IsNullOrEmpty(o.ProgressWebhook) ? T("observer.list.yes", lng) : T("observer.list.no", lng),
                [T("observer.list.releases", lng)] = !string.IsNullOrEmpty(o.ReleasesWebhook) ? T("observer.list.yes", lng) : T("observer.list.no", lng),
                [T("observer.list.role", lng)] = o.RoleId is not null ? T("observer.list.yes", lng) : T("observer.list.no", lng),
            }).Chunk(10).ToList();

            foreach (var chunk in tblData)
            {
                var sb = new StringBuilder();
                sb.AppendLine("```");
                sb.AppendLine(new TableFormatter().FormatDictionaries(chunk));
                sb.AppendLine("```");

                // Send table
                await interaction.Channel.SendMessageAsync(text: sb.ToString(), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
            }

            return ExecutionResult.Success;
        }
    }
}
