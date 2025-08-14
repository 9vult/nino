using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Localizer;
using Nino.Handlers;
using Nino.Records.Enums;
using Nino.Utilities;
using Nino.Utilities.Extensions;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ProjectManagement
    {
        [SlashCommand("archive", "Archive a project")]
        public async Task<RuntimeResult> Archive(
            [Autocomplete(typeof(ProjectAutocompleteHandler))] string alias
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var gLng =
                db.GetConfig(interaction.GuildId ?? 0)?.Locale?.ToDiscordLocale()
                ?? interaction.GuildLocale
                ?? "en-US";

            // Verify project and user - Owner required
            var project = await db.ResolveAlias(alias, interaction);
            if (project is null)
                return await Response.Fail(
                    T("error.alias.resolutionFailed", lng, alias),
                    interaction
                );

            if (!project.VerifyUser(db, interaction.User.Id, excludeAdmins: true))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            if (project.IsArchived)
                return await Response.Fail(T("error.archived", lng), interaction);

            // Ask if the user is sure
            var (goOn, finalBody) = await Ask.AboutIrreversibleAction(
                interactive,
                interaction,
                project,
                lng,
                Ask.IrreversibleAction.Archive
            );

            if (goOn)
            {
                Log.Info($"Archiving project {project}");

                project.Aliases.Clear();
                project.Motd = null;
                project.IsArchived = true;

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
                    var publishChannel = (SocketTextChannel)
                        Nino.Client.GetChannel(project.UpdateChannelId);
                    await publishChannel.SendMessageAsync(embed: publishEmbed);
                }
                catch (Exception e)
                {
                    Log.Error(e.Message);
                    var guild = Nino.Client.GetGuild(interaction.GuildId ?? 0);
                    await Utils.AlertError(
                        T("error.release.failed", lng, e.Message),
                        guild,
                        project.Nickname,
                        project.OwnerId,
                        "Release"
                    );
                }

                // Publish to observers
                await ObserverPublisher.PublishProgress(project, publishEmbed, db);
            }

            // Send embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(finalBody)
                .Build();
            await interaction.ModifyOriginalResponseAsync(m =>
            {
                m.Embed = embed;
                m.Components = null;
            });

            await db.TrySaveChangesAsync(interaction);
            return ExecutionResult.Success;
        }
    }
}
