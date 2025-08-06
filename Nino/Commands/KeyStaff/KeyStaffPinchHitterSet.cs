using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class KeyStaff
    {
        public partial class PinchHitterManagement
        {
            [SlashCommand("set", "Set a pinch hitter for an episode")]
            public async Task<RuntimeResult> Set(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
                [Summary("abbreviation", "Position shorthand"), Autocomplete(typeof(KeyStaffAutocompleteHandler))] string abbreviation,
                [Summary("member", "Staff member")] SocketUser member
            )
            {
                var interaction = Context.Interaction;
                var lng = interaction.UserLocale;

                // Sanitize inputs
                var memberId = member.Id;
                alias = alias.Trim();
                abbreviation = abbreviation.Trim().ToUpperInvariant();
                episodeNumber = Utils.CanonicalizeEpisodeNumber(episodeNumber);

                // Verify project and user - Owner or Admin required
                var project = db.ResolveAlias(alias, interaction);
                if (project is null)
                    return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

                if (!Utils.VerifyUser(interaction.User.Id, project))
                    return await Response.Fail(T("error.permissionDenied", lng), interaction);

                // Verify episode
                if (!project.TryGetEpisode(episodeNumber, out var episode))
                    return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);
                
                // Check if position exists
                if (project.KeyStaff.All(ks => ks.Role.Abbreviation != abbreviation))
                    return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

                // All good!
                var hitter = new PinchHitter
                {
                    Id = Guid.NewGuid(),
                    UserId = memberId,
                    Abbreviation = abbreviation
                };

                episode.PinchHitters.RemoveAll(p => p.Abbreviation == abbreviation);
                episode.PinchHitters.Add(hitter);

                Log.Info($"Set M[{memberId} (@{member.Username})] as pinch hitter for {abbreviation} for {episode}");

                // Send success embed
                var staffMention = $"<@{memberId}>";
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.projectModification", lng))
                    .WithDescription(T("keyStaff.pinchHitter.set", lng, staffMention, episode.Number, abbreviation))
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                await db.SaveChangesAsync();
                return ExecutionResult.Success;
            }
        }
    }
}
