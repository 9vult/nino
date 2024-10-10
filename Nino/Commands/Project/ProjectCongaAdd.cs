using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
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
            [SlashCommand("add", "Add a link to the Conga line")]
            public async Task<RuntimeResult> Add(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string current,
                [Summary("next", "Position to ping"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string next
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                current = current.Trim().ToUpperInvariant();
                next = next.Trim().ToUpperInvariant();

                List<string> manyToOne = [];
                List<string> oneToMany = [];
                int manyToOneCount = 0;
                int oneToManyCount = 0;

                // Verify project and user - Owner or Admin required
                var project = Utils.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Validate current task isn't already in the conga line
                if (project.CongaParticipants.Any(c => c.Current == current && c.Next == next))
                    return await Response.Fail(T("error.conga.alreadyExists", lng, current, next), interaction);

                // Find current union members (one→many relationship)
                oneToMany.AddRange(project.CongaParticipants.Where(c => c.Current == current).Select(p => p.Next));
                oneToManyCount = oneToMany.Count;
                // Find next union members (many→one relationship)
                manyToOne.AddRange(project.CongaParticipants.Where(c => c.Next == next).Select(p => p.Current));
                manyToOneCount = manyToOne.Count;

                // Validate tasks exist
                if (!project.KeyStaff.Any(ks => ks.Role.Abbreviation == current))
                    return await Response.Fail(T("error.noSuchTask", lng, current), interaction);
                if (!project.KeyStaff.Any(ks => ks.Role.Abbreviation == next))
                    return await Response.Fail(T("error.noSuchTask", lng, next), interaction);

                // We good!
                var participant = new CongaParticipant
                {
                    Current = current,
                    Next = next
                };

                oneToMany.Add(next);
                manyToOne.Add(current);

                // Add to database
                await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                    patchOperations: [
                        PatchOperation.Add("/congaParticipants/-", participant)
                ]);

                log.Info($"Added {current} → {next} to the Conga line for {project.Id}");

                string description;

                // New one-to-many union (ex: TL → (ED, TS))
                if (oneToMany.Count > 1 && oneToMany.Count > oneToManyCount)
                {
                    var unionStr = $"({string.Join(", ", oneToMany)})";
                    description = T("project.conga.union.added.next", lng, current, next, current, unionStr);
                }
                // New many-to-one union (ex: (TLC, TS) → QC)
                else if (manyToOne.Count > 1 && manyToOne.Count > manyToOneCount)
                {
                    var unionStr = $"({string.Join(", ", manyToOne)})";
                    description = T("project.conga.union.added.current", lng, current, next, unionStr, next);
                }
                // Normal (ex: QC1 → QC2)
                else
                {
                    description = T("project.conga.added", lng, current, next);
                }

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(description)
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await Cache.RebuildCacheForProject(project.Id);
                return ExecutionResult.Success;
            }
        }
    }
}
