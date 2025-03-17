using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Localizer;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("archive", "Archive a project")]
        public async Task<RuntimeResult> Archive(
            [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var gLng = Cache.GetConfig(interaction.GuildId ?? 0)?.Locale?.ToDiscordLocale() ?? interaction.GuildLocale ?? "en-US";

            // Verify project and user - Owner required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            if (project.IsArchived)
                return await Response.Fail(T("error.archived", lng), interaction);

            // Ask if the user is sure
            var questionBody = T("project.archive.question", lng, project.Nickname);

            var header = project.IsPrivate
                ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";

            var component = new ComponentBuilder()
                .WithButton(T("project.archive.cancel.button", lng), "ninoarchivecancel", ButtonStyle.Success)
                .WithButton(T("project.archive.continue.button", lng), "ninoarchivecontinue", ButtonStyle.Danger)
                .Build();
            var questionEmbed = new EmbedBuilder()
                .WithAuthor(header)
                .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                .WithDescription(questionBody)
                .WithCurrentTimestamp()
                .Build();

            var questionResponse = await interaction.ModifyOriginalResponseAsync(m => {
                m.Embed = questionEmbed;
                m.Components = component;
            });

            // Wait for response
            var questionResult = await _interactiveService.NextMessageComponentAsync(
                m => m.Message.Id == questionResponse.Id, timeout: TimeSpan.FromSeconds(60));

            bool goOn = false;
            string finalBody = string.Empty;

            if (!questionResult.IsSuccess)
                finalBody = T("progress.done.inTheDust.timeout", lng);
            else
            {
                if (questionResult.Value.Data.CustomId == "ninoarchivecancel")
                    finalBody = T("progress.done.inTheDust.dontDoIt", lng);
                else
                {
                    goOn = true;
                    finalBody = T("project.archive.done", lng, project.Nickname);
                }
            }

            if (goOn)
            {
                Log.Info($"Archiving project {project}");

                // Update database
                List<string> emptyList = [];

                await AzureHelper.PatchProjectAsync(project, [
                    PatchOperation.Replace($"/aliases", emptyList), // Remove aliases
                    PatchOperation.Set<string?>($"/motd", null), // Remove MOTD
                    PatchOperation.Set($"/isArchived", true) // set as archived
                ]);

                // Announce archival
                var publishEmbed = new EmbedBuilder()
                    .WithAuthor($"{project.Title} ({project.Type.ToFriendlyString(gLng)})")
                    .WithTitle(T("title.archived", gLng))
                    .WithDescription(T("project.archive.publish", gLng))
                    .WithThumbnailUrl(project.PosterUri)
                    .WithCurrentTimestamp()
                    .Build();

                // Publish to local progress channel
                try
                {
                    var publishChannel = (SocketTextChannel)Nino.Client.GetChannel(project.UpdateChannelId);
                    await publishChannel.SendMessageAsync(embed: publishEmbed);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                    var guild = Nino.Client.GetGuild(interaction.GuildId ?? 0);
                    await Utils.AlertError(T("error.release.failed", lng, e.Message), guild, project.Nickname, project.OwnerId, "Release");
                }

                // Publish to observers
                await ObserverPublisher.PublishProgress(project, publishEmbed);
            }

            // Send embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(finalBody)
                .Build();
            await interaction.ModifyOriginalResponseAsync(m => {
                m.Embed = embed;
                m.Components = null;
            });

            await Cache.RebuildCacheForProject(project.Id);
            return ExecutionResult.Success;
        }
    }
}
