using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands;

public partial class ProjectManagement
{
    public partial class Conga
    {
        [SlashCommand("import", "Import Conga nodes from a file")]
        public async Task<RuntimeResult> Import(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            IAttachment file
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();

            // Verify project and user - Owner or Admin required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            List<(string, string, CongaNodeType, CongaNodeType)> importedNodes = [];
            string fileContent;
            try
            {
                Log.Trace("Attempting to get and parse conga file...");
                using var client = new HttpClient();
                fileContent = await client.GetStringAsync(file.Url);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Trace("Conga file could not be read");
                return await Response.Fail(e.Message, interaction);
            }

            if (string.IsNullOrWhiteSpace(fileContent))
                return await Response.Fail(T("error.conga.invalidFile", lng), interaction);

            foreach (var entry in fileContent.Split(Environment.NewLine))
            {
                if (string.IsNullOrWhiteSpace(entry))
                    continue;

                var match = CongaEntryRegex().Match(entry.Trim());
                if (!match.Success)
                    return await Response.Fail(T("error.conga.invalidFile", lng), interaction);

                var current = match.Groups[1].Value.Trim().ToUpperInvariant();
                var next = match.Groups[2].Value.Trim().ToUpperInvariant();

                // Validate current task isn't already in the conga line
                if (
                    project.CongaParticipants.Contains(current)
                    && project
                        .CongaParticipants.GetDependentsOf(current)
                        .Any(c => c.Abbreviation == next)
                )
                    continue;

                // Validate tasks
                CongaNodeType currentType;
                CongaNodeType nextType;

                if (current.StartsWith('$'))
                    currentType = CongaNodeType.Special;
                else if (current.StartsWith('@'))
                    currentType = CongaNodeType.Group;
                else
                {
                    if (project.KeyStaff.Any(ks => ks.Role.Abbreviation == current))
                        currentType = CongaNodeType.KeyStaff;
                    else if (
                        project
                            .Episodes.SelectMany(e => e.AdditionalStaff)
                            .Any(ks => ks.Role.Abbreviation == current)
                    )
                        currentType = CongaNodeType.AdditionalStaff;
                    else
                        return await Response.Fail(
                            T("error.noSuchTask", lng, current),
                            interaction
                        );
                }

                if (next.StartsWith('$'))
                    nextType = CongaNodeType.Special;
                else if (next.StartsWith('@'))
                    nextType = CongaNodeType.Group;
                else
                {
                    if (project.KeyStaff.Any(ks => ks.Role.Abbreviation == next))
                        nextType = CongaNodeType.KeyStaff;
                    else if (
                        project
                            .Episodes.SelectMany(e => e.AdditionalStaff)
                            .Any(ks => ks.Role.Abbreviation == next)
                    )
                        nextType = CongaNodeType.AdditionalStaff;
                    else
                        return await Response.Fail(T("error.noSuchTask", lng, next), interaction);
                }

                importedNodes.Add((current, next, currentType, nextType));
            }

            foreach (var importedNode in importedNodes)
            {
                // Comedy
                project.CongaParticipants.Add(
                    importedNode.Item1,
                    importedNode.Item2,
                    importedNode.Item3,
                    importedNode.Item4
                );
            }

            Log.Info($"Imported {importedNodes.Count} conga entries to {project}.");

            var body = T(
                "project.conga.imported",
                lng,
                new Dictionary<string, object>
                {
                    ["number"] = importedNodes.Count,
                    ["project"] = project.Nickname,
                }
            );

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(body)
                .Build();
            await interaction.FollowupAsync(embed: embed);

            db.Entry(project).Property(p => p.CongaParticipants).IsModified = true;
            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }

        [GeneratedRegex("^(.+)->(.+)$")]
        private static partial Regex CongaEntryRegex();
    }
}
