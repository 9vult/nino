using System.Text;
using System.Text.Json;
using Discord;
using Discord.Interactions;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        public partial class Conga
        {
            [SlashCommand("list", "List all the Conga line participants")]
            public async Task<RuntimeResult> List(
                [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
                [Autocomplete(typeof(EpisodeAutocompleteHandler))] string? episodeNumber = null,
                bool forceAdditional = false
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

                // Verify episode
                Episode? episode = null;
                if (episodeNumber is not null && !project.TryGetEpisode(episodeNumber, out episode))
                    return await Response.Fail(
                        T("error.noSuchEpisode", lng, episodeNumber),
                        interaction
                    );

                Log.Trace(
                    $"Listing Conga graph for {project} (episode={episodeNumber ?? "null"},forced={forceAdditional}) for M[{interaction.User.Id} (@{interaction.User.Username})]"
                );

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

                var dot = episode is null
                    ? CongaHelper.GetDot(project, forceAdditional)
                    : CongaHelper.GetDot(project, episode, forceAdditional);

                var data = new StringContent(
                    JsonSerializer.Serialize(
                        new
                        {
                            graph = dot,
                            layout = "dot",
                            format = "png",
                        }
                    ),
                    Encoding.UTF8,
                    "application/json"
                );

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("https://quickchart.io/graphviz", data);

                if (!response.IsSuccessStatusCode)
                {
                    await interaction.FollowupAsync(
                        $"{response.StatusCode}: {response.ReasonPhrase}"
                    );
                    return ExecutionResult.Success;
                }

                using var stream = new MemoryStream(await response.Content.ReadAsByteArrayAsync());

                // Send embed
                var embed = new EmbedBuilder()
                    .WithTitle(T("title.congaList", lng))
                    .WithImageUrl("attachment://congo.png")
                    .Build();
                await interaction.FollowupWithFileAsync(stream, "congo.png", embed: embed);

                return ExecutionResult.Success;
            }
        }
    }
}
