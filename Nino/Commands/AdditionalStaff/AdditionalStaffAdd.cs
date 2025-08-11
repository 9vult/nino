using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;
using Task = Nino.Records.Task;

namespace Nino.Commands
{
    public partial class AdditionalStaff
    {
        [SlashCommand("add", "Add additional staff to an episode")]
        public async Task<RuntimeResult> Add(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
            [Autocomplete(typeof(EpisodeAutocompleteHandler))] string episodeNumber,
            SocketUser member,
            [MaxLength(16)] string abbreviation,
            [MaxLength(32)] string fullName,
            bool isPseudo = false
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize inputs
            var memberId = member.Id;
            alias = alias.Trim();
            fullName = fullName.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant().Replace("$", string.Empty);
            episodeNumber = Episode.CanonicalizeEpisodeNumber(episodeNumber);

            // Verify project and user - Owner or Admin required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Verify episode
            if (!project.TryGetEpisode(episodeNumber, out var episode))
                return await Response.Fail(
                    T("error.noSuchEpisode", lng, episodeNumber),
                    interaction
                );

            // Check if position already exists
            if (
                project
                    .KeyStaff.Concat(episode.AdditionalStaff)
                    .Any(ks => ks.Role.Abbreviation == abbreviation)
            )
                return await Response.Fail(T("error.positionExists", lng), interaction);

            // All good!
            var newStaff = new Staff
            {
                UserId = memberId,
                Role = new Role
                {
                    Abbreviation = abbreviation,
                    Name = fullName,
                    Weight = 1000000,
                },
                IsPseudo = isPseudo,
            };

            var newTask = new Task { Abbreviation = abbreviation, Done = false };

            // Add to database

            episode.AdditionalStaff.Add(newStaff);
            episode.Tasks.Add(newTask);
            episode.Done = false;

            Log.Info(
                $"Added M[{memberId} (@{member.Username})] to {episode} for {abbreviation} (IsPseudo={isPseudo})"
            );

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(
                    T("additionalStaff.added", lng, staffMention, abbreviation, episode.Number)
                )
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
