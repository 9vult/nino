using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class Alias
        {
            [SlashCommand("add", "Add a new alias")]
            public async Task<RuntimeResult> Add(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("alias", "Alias")] string input
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();
                input = input.Trim();

                // Verify project and user - Owner or Admin required
                var project = await db.ResolveAlias(alias, interaction);
                if (project is null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (project.IsArchived)
                    return await Response.Fail(T("error.archived", lng), interaction);

                if (!project.VerifyUser(db, interaction.User.Id))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Validate alias doesn't exist
                var preexistingProject = await db.ResolveAlias(input, interaction);
                if (preexistingProject is not null)
                    return await Response.Fail(T("error.alias.inUse", lng, preexistingProject.Nickname), interaction);
                
                project.Aliases.Add(new Records.Alias { Value = input });

                Log.Info($"Added {input} as an alias for {project}");

                // Send success embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("project.alias.addedAlias", lng, input, project.Nickname))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await db.TrySaveChangesAsync(interaction);
                return ExecutionResult.Success;
            }
        }
    }
}
