using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class Debug
    {
        public class DebugInfo() : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("info", "Debugging Information")]
            public async Task<RuntimeResult> Handle(
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
                
                Log.Trace($"Generating debug information for {project} for M[{interaction.User.Id} (@{interaction.User.Username})]");

                if (project.KeyStaff.Length == 0)
                    return await Response.Fail(T("error.noRoster", lng), interaction);

                var debugData = new
                {
                    Id = project.Id,
                    GuildId = project.GuildId,
                    OwnerId = project.OwnerId,
                    IsArchived = project.IsArchived,
                    IsPrivate = project.IsPrivate,
                    AniListId = project.AniListId ?? -1,
                };
                
                await interaction.FollowupAsync($"```json\n{JsonConvert.SerializeObject(debugData, Formatting.Indented)}\n```");

                return ExecutionResult.Success;
            }
        }
    }
}
