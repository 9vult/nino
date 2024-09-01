using Discord;
using Discord.Interactions;
using Microsoft.Azure.Cosmos;
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
                [Summary("project", "Project nickname")] string alias,
                [Summary("abbreviation", "Position shorthand")] string current,
                [Summary("next", "Position to ping")] string next
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                current = current.Trim().ToUpperInvariant();
                next = next.Trim().ToUpperInvariant();

                // Verify project and user - Owner or Admin required
                var project = Utils.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Validate tasks aren't already in the conga line
                if (project.CongaParticipants.Any(c => c.Current == current))
                    return await Response.Fail(T("error.conga.alreadyExists", lng, current), interaction);
                if (project.CongaParticipants.Any(c => c.Next == next))
                    return await Response.Fail(T("error.conga.alreadyExists", lng, next), interaction);

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

                // Add to database
                await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                    patchOperations: [
                        PatchOperation.Add("/congaParticipants/-", participant)
                ]);

                log.Info($"Added {current} → {next} to the Conga line for {project.Id}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.conga.added", lng, current, next))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                return ExecutionResult.Success;
            }
        }
    }
}
