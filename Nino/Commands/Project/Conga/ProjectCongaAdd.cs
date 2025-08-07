using System.Text;
using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class Conga
        {
            [SlashCommand("add", "Add a link to the Conga line")]
            public async Task<RuntimeResult> Add(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(CongaCurrentAutocompleteHandler))] string current,
                [Summary("next", "Position to ping"), Autocomplete(typeof(CongaNextAutocompleteHandler))] string next
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                current = current.Trim().ToUpperInvariant();
                next = next.Trim().ToUpperInvariant();

                // Verify project and user - Owner or Admin required
                var project = db.ResolveAlias(alias, interaction);
                if (project is null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Validate current task isn't already in the conga line
                if (project.CongaParticipants.Contains(current) 
                    && project.CongaParticipants.GetDependentsOf(current).Any(c => c.Abbreviation == next))
                    return await Response.Fail(T("error.conga.alreadyExists", lng, current, next), interaction);

                // Validate tasks

                var currentType = CongaNodeType.Unknown;
                var nextType = CongaNodeType.Unknown;

                if (current.StartsWith('$')) currentType = CongaNodeType.Special;
                else
                {
                    if (project.KeyStaff.Any(ks => ks.Role.Abbreviation == current))
                        currentType = CongaNodeType.KeyStaff;
                    else if (project.Episodes.SelectMany(e => e.AdditionalStaff).Any(ks => ks.Role.Abbreviation == current))
                        currentType = CongaNodeType.AdditionalStaff;
                    else
                        return await Response.Fail(T("error.noSuchTask", lng, current), interaction);
                }
                if (next.StartsWith('$')) nextType = CongaNodeType.Special;
                else
                {
                    if (project.KeyStaff.Any(ks => ks.Role.Abbreviation == next))
                        nextType = CongaNodeType.KeyStaff;
                    else if (project.Episodes.SelectMany(e => e.AdditionalStaff).Any(ks => ks.Role.Abbreviation == next))
                        nextType = CongaNodeType.AdditionalStaff;
                    else
                        return await Response.Fail(T("error.noSuchTask", lng, next), interaction);
                }

                // We are good to go!
                var originalPrereqCount = project.CongaParticipants.GetPrerequisitesFor(next).Count();
                var originalDepsCount = project.CongaParticipants.GetDependentsOf(current).Count();
                
                project.CongaParticipants.Add(current, next, currentType, nextType);
                
                var prereqs = project.CongaParticipants.GetPrerequisitesFor(next).ToList();
                var deps = project.CongaParticipants.GetDependentsOf(current).ToList();

                Log.Info($"Added {current} → {next} to the Conga line for {project}");

                var description = new StringBuilder();

                // New one-to-many union (ex: TL → (ED, TS))
                if (deps.Count > 1 && deps.Count > originalDepsCount)
                {
                    var unionStr = $"({string.Join(", ", deps.Select(p => p.Abbreviation))})";
                    description.AppendLine(T("project.conga.union.added.next", lng, current, next, current, unionStr));
                }
                // New many-to-one union (ex: (TLC, TS) → QC)
                else if (prereqs.Count > 1 && prereqs.Count > originalPrereqCount)
                {
                    var unionStr = $"({string.Join(", ", prereqs.Select(p => p.Abbreviation))})";
                    description.AppendLine(T("project.conga.union.added.current", lng, current, next, unionStr, next));
                }
                // Normal (ex: QC1 → QC2)
                else
                {
                    description.AppendLine(T("project.conga.added", lng, current, next));
                }
                
                // Add reminder information if this is the first conga added
                if (project.CongaParticipants.Nodes.Count == 1)
                    description.AppendLine(T("project.conga.firstHelp", lng));

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(description.ToString())
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await db.TrySaveChangesAsync(interaction);
                return ExecutionResult.Success;
            }
        }
    }
}
