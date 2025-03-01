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
        public class RebuildCache(InteractionHandler handler, InteractionService commands) : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("rebuild-cache", "Rebuild Nino's cache")]
            public async Task<RuntimeResult> Handle()
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;
                
                // Verify bot owner
                if (Nino.Config.OwnerId != interaction.User.Id)
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);
                
                Log.Info($"Cache rebuild requested by M[{interaction.User.Id} (@{interaction.User.Username})]");
                await Cache.BuildCache();
                
                await interaction.FollowupAsync(T("debug.cacheRebuild", lng));

                return ExecutionResult.Success;
            }
        }
    }
}
