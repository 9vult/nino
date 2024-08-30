using System;
using System.Text;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Nino.Records.Enums;
using Nino.Utilities;
using NLog;
using static Localizer.Localizer;

namespace Nino.Commands
{
    internal static partial class Release
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public const string Name = "release";

        public static async Task<bool> Handle(SocketSlashCommand interaction)
        {
            var lng = interaction.UserLocale;

            var alias = ((string)interaction.Data.Options.FirstOrDefault(o => o.Name == "project")!.Value).Trim();

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);

            // Get inputs
            var releaseType = (ReleaseType)Convert.ToInt32(interaction.Data.Options.FirstOrDefault(o => o.Name == "type")!.Value);
            var releaseNumber = (string)interaction.Data.Options.FirstOrDefault(o => o.Name == "number")!.Value;
            var releaseUrl = (string)interaction.Data.Options.FirstOrDefault(o => o.Name == "url")!.Value;
            var roleId = ((SocketRole?)interaction.Data.Options.FirstOrDefault(o => o.Name == "role")?.Value)?.Id;

            var roleStr = roleId != null
                ? roleId == project.GuildId ? "@everyone " : $"<@&{roleId}> "
                : "";

            var publishBody = releaseType != ReleaseType.Custom
                ? $"**{project.Title} - {releaseType.ToFriendlyString()} {releaseNumber}**\n{roleStr}{releaseUrl}"
                : $"**{project.Title} - {releaseNumber}**\n{roleStr}{releaseUrl}";
            
            // Add prefix if needed
            if (!string.IsNullOrEmpty(Cache.GetConfig(project.GuildId)?.ReleasePrefix))
                publishBody = $"{Cache.GetConfig(project.GuildId)!.ReleasePrefix!} {publishBody}";

            // Publish to local releases channel
            try
            {
                var publishChannel = (SocketTextChannel)Nino.Client.GetChannel(project.ReleaseChannelId);
                var msg = await publishChannel.SendMessageAsync(text: publishBody);
                if (msg.Channel.GetChannelType() == ChannelType.News) // Guild announcement channel
                    await msg.CrosspostAsync(); // Publish announcement
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                return await Response.Fail(T("error.release.failed", lng, e.Message), interaction);
            }

            // Publish to observers
            await ObserverPublisher.PublishRelease(project, releaseType, releaseNumber, releaseUrl);
            
            // Send success embed
            var replyEmbed = new EmbedBuilder()
                .WithAuthor(name: $"{project.Title} ({project.Type.ToFriendlyString()})")
                .WithTitle(T("title.released", lng))
                .WithDescription(T("progress.released", lng, project.Title, releaseType.ToFriendlyString(), releaseNumber))
                .WithCurrentTimestamp()
                .Build();
            await interaction.FollowupAsync(embed: replyEmbed);

            return true;
        }

        public static SlashCommandBuilder Builder =>
            new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription("Release!")
            .WithNameLocalizations(GetCommandNames(Name))
            .WithDescriptionLocalizations(GetCommandDescriptions(Name))
            .AddOption(CommonOptions.Project())
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("type")
                .WithDescription("Type of release")
                .WithNameLocalizations(GetOptionNames("release.type"))
                .WithDescriptionLocalizations(GetOptionDescriptions("release.type"))
                .WithRequired(true)
                .AddChoice("Episode", 0, GetChoiceNames("release.type.episode"))
                .AddChoice("Volume", 1, GetChoiceNames("release.type.volume"))
                .AddChoice("Batch", 2, GetChoiceNames("release.type.batch"))
                .AddChoice("Custom", 3, GetChoiceNames("release.type.custom"))
                .WithType(ApplicationCommandOptionType.Number)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("number")
                .WithDescription("What is being released?")
                .WithNameLocalizations(GetOptionNames("release.number"))
                .WithDescriptionLocalizations(GetOptionDescriptions("release.number"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("url")
                .WithDescription("Release URL(s)")
                .WithNameLocalizations(GetOptionNames("release.url"))
                .WithDescriptionLocalizations(GetOptionDescriptions("release.url"))
                .WithRequired(true)
                .WithType(ApplicationCommandOptionType.String)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("role")
                .WithDescription("Role to ping")
                .WithNameLocalizations(GetOptionNames("release.role"))
                .WithDescriptionLocalizations(GetOptionDescriptions("release.role"))
                .WithRequired(false)
                .WithType(ApplicationCommandOptionType.Role)
            );
    }
}
