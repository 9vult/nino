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
    public class ProjectRoster(DataContext db) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("roster", "See who worked on a project")]
        public async Task<RuntimeResult> Handle(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            alias = alias.Trim();

            // Verify project and user - minimum Key Staff required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id, includeStaff: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            Log.Trace(
                $"Generating roster for {project} for M[{interaction.User.Id} (@{interaction.User.Username})]"
            );

            if (project.KeyStaff.Count == 0)
                return await Response.Fail(T("error.noRoster", lng), interaction);

            var roster = project.GenerateRoster();
            var title = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var embed = new EmbedBuilder()
                .WithAuthor(title, url: project.AniListUrl)
                .WithTitle(T("title.roster", lng))
                .WithThumbnailUrl(project.PosterUri)
                .WithDescription(roster)
                .WithCurrentTimestamp()
                .Build();

            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
