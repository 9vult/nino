using Discord.Interactions;
using Newtonsoft.Json;
using Nino.Handlers;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Debug
    {
        public class DebugInfo(DataContext db) : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("info", "Debugging Information")]
            public async Task<RuntimeResult> Handle(
                [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                alias = alias.Trim();

                // Verify project and user - Owner or Admin required
                var project = await db.ResolveAlias(alias, interaction);
                if (project == null)
                    return await Response.Fail(
                        T("error.alias.resolutionFailed", lng, alias),
                        interaction
                    );

                if (!project.VerifyUser(db, interaction.User.Id))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                Log.Trace(
                    $"Generating debug information for {project} for M[{interaction.User.Id} (@{interaction.User.Username})]"
                );

                if (project.KeyStaff.Count == 0)
                    return await Response.Fail(T("error.noRoster", lng), interaction);

                var debugData = new
                {
                    project.Id,
                    project.GuildId,
                    project.OwnerId,
                    project.IsArchived,
                    project.IsPrivate,
                    AniListId = project.AniListId ?? -1,
                };

                await interaction.FollowupAsync(
                    $"```json\n{JsonConvert.SerializeObject(debugData, Formatting.Indented)}\n```"
                );

                return ExecutionResult.Success;
            }
        }
    }
}
