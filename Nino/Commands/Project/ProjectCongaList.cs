using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records;
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
                        .Select(g => new { Current = g.Select(x => WC(x.Current)).OrderBy(x => x.Weight).ToList(), Next = g.Key })
                        .ToList();

                    var alreadyGroupedNexts = new HashSet<string>(groupedByNext.Select(g => g.Next));

                    var groupedByCurrent = project.CongaParticipants
                        .Where(p => !alreadyGroupedNexts.Contains(p.Next))
                        .GroupBy(p => p.Current)
                        .Select(g => new { Current = WC(g.Key), Next = g.Select(x => x.Next).ToList() })
                        .ToList();

                    description = string.Join(Environment.NewLine, groupedByCurrent.Select(g =>
                        {
                            if (g.Next.Count == 1)
                                return new { g.Current.Weight, Text = $"{g.Current.Value} → {g.Next.First()}" };
                            else
                                return new { g.Current.Weight, Text = $"{g.Current.Value} → ({string.Join(", ", g.Next)})" };
                        })
                        .Concat(groupedByNext.Select(g =>
                        {
                            if (g.Current.Count == 1)
                                return new { g.Current.Last().Weight, Text = $"{g.Current.Last().Value} → {g.Next}" };
                            else
                                return new { g.Current.Last().Weight, Text = $"({string.Join(", ", g.Current.Select(x => x.Value))}) → {g.Next}" };
                        }))
                        .OrderBy(x => x.Weight)
                        .Select(x => x.Text)
                    );
                }

                // Local function for creating a weighted current
                WeightedCurrent WC(string abbreviation) => new()
                {
                    Value = abbreviation,
                    Weight = project!.KeyStaff.FirstOrDefault(k => k.Role.Abbreviation == abbreviation)?.Role.Weight ?? 0
                };

                // Send embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.congaList", lng))
                    .WithDescription(description)
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                return ExecutionResult.Success;
            }
        }

        private class WeightedCurrent
        {
            public required string Value;
            public required decimal Weight;
        }
    }
}
