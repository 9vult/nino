using Discord;
using Discord.Interactions;
using Nino.Utilities;
using System.Text;
using Tababular;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("list", "List the projects in this server")]
        public async Task<RuntimeResult> List()
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var guildId = interaction.GuildId ?? 0;

            // Check for guild administrator status
            var guild = Nino.Client.GetGuild(guildId);
            var member = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(db, member, guild)) return await Response.Fail(T("error.notPrivileged", lng), interaction);
            
            Log.Trace($"Listing projects for {guildId}");

            // Get projects
            var projects = db.Projects.Where(p => p.GuildId == guildId).ToList();

            if (projects.Count == 0)
                return await Response.Fail(T("error.noProjects", lng), interaction);

            var tblData = projects.Select(p => new Dictionary<string, string>
            {
                [T("project.list.nickname", lng)] = p.Nickname,
                [T("project.list.owner", lng)] = Nino.Client.GetUser(p.OwnerId)?.Username ?? $"<@{p.OwnerId}>",
                [T("project.list.isPrivate", lng)] = p.IsPrivate ? T("observer.list.yes", lng) : T("observer.list.no", lng),
                [T("project.list.isArchived", lng)] = p.IsArchived ? T("observer.list.yes", lng) : T("observer.list.no", lng),
                [T("project.list.episodeCount", lng)] = p.Episodes.Count.ToString()
            }).Chunk(10).ToList();

            foreach (var chunk in tblData)
            {
                var sb = new StringBuilder();
                sb.AppendLine("```");
                sb.AppendLine(new TableFormatter().FormatDictionaries(chunk));
                sb.AppendLine("```");

                // Send table
                await interaction.FollowupAsync(text: sb.ToString(), allowedMentions: new AllowedMentions(AllowedMentionTypes.None));
            }

            return ExecutionResult.Success;
        }
    }
}
