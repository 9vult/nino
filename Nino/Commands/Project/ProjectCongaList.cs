using Discord;
using Discord.Interactions;
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
            [SlashCommand("list", "List all the Conga line participants")]
            public async Task<RuntimeResult> Remove(
                [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Summary("episode", "Episode number"), Autocomplete(typeof(EpisodeAutocompleteHandler))] string? episodeNumber = null
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
                
                // Verify episode
                Episode? episode = null;
                if (episodeNumber is not null && !Getters.TryGetEpisode(project, episodeNumber, out episode))
                    return await Response.Fail(T("error.noSuchEpisode", lng, episodeNumber), interaction);

                Log.Trace($"Listing Conga line for {project}");

                // Process

                if (project.CongaParticipants.Nodes.Count == 0)
                {
                    // Send embed
                    var emptyEmbed = new EmbedBuilder()
                        .WithTitle(T("title.congaList", lng))
                        .WithDescription(T("project.conga.empty", lng))
                        .Build();
                    await interaction.FollowupAsync(embed: emptyEmbed);
                    return ExecutionResult.Success;
                }

                var encodedDot = episode is null ? CongaHelper.GetDot(project) : CongaHelper.GetDot(project, episode);
                var url = $"https://quickchart.io/graphviz?format=png&graph={encodedDot}";

                // Send embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.congaList", lng))
                    .WithImageUrl(url)
                    .Build();
                await interaction.FollowupAsync(embed: embed);

                return ExecutionResult.Success;
            }
        }
    }
}
