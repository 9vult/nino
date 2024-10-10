using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class Conga
        {
            [SlashCommand("list", "List all the Conga line participants")]
            public async Task<RuntimeResult> Remove(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias
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
                var description = string.Empty;

                if (project.CongaParticipants.Length == 0)
                    description = T("project.conga.empty", lng);
                else
                {
                    var groupedByNext = project.CongaParticipants
                        .GroupBy(p => p.Next)
                        .Where(g => g.Count() > 1)
                        .Select(g => new { Current = g.Select(x => x.Current).ToList(), Next = g.Key })
                        .OrderBy(g => g.Current.Count)
                        .ToList();

                    var alreadyGroupedNexts = new HashSet<string>(groupedByNext.Select(g => g.Next));

                    var groupedByCurrent = project.CongaParticipants
                        .Where(p => !alreadyGroupedNexts.Contains(p.Next))
                        .GroupBy(p => p.Current)
                        .Select(g => new { Current = g.Key, Next = g.Select(x => x.Next).ToList() })
                        .OrderByDescending(g => g.Next.Count)
                        .ToList();

                    description = string.Join(Environment.NewLine,
                        groupedByCurrent.Select(g => g.Next.Count == 1 ? $"{g.Current} → {g.Next.First()}" : $"{g.Current} → ({string.Join(", ", g.Next)})")
                        .Concat(groupedByNext.Select(g => g.Current.Count == 1 ? $"{g.Current.First()} → {g.Next}" : $"({string.Join(", ", g.Current)}) → {g.Next}"))
                    );
                }

                // Send embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.congaList", lng))
                    .WithDescription(description)
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                return ExecutionResult.Success;
            }
        }
    }
}
