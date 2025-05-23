﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Localizer;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ServerManagement
    {
        [SlashCommand("set-locale", "Set the locale for this server")]
        public async Task<RuntimeResult> SetLocale(
            [Summary("newValue", "New Value"), Autocomplete(typeof(LocaleAutocompleteHandler))] Locale locale
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;
            var guildId = interaction.GuildId ?? 0;
            var guild = Nino.Client.GetGuild(guildId);

            // Server administrator permissions required
            var runner = guild.GetUser(interaction.User.Id);
            if (!Utils.VerifyAdministrator(runner, guild, excludeServerAdmins: true))
                return await Response.Fail(T("error.notPrivileged", lng), interaction);

            var config = await Getters.GetConfiguration(guildId);
            if (config == null)
                return await Response.Fail(T("error.noSuchConfig", lng), interaction);

            // Apply change and upsert to database
            config.Locale = locale;

            await AzureHelper.Configurations!.UpsertItemAsync(config);
            log.Info($"Updated configuration for guild {config.GuildId}, set Locale to {locale.ToDiscordLocale() ?? "null"}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.serverConfiguration", lng))
                .WithDescription($"{T("server.configuration.saved", lng)}")
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildConfigCache();
            return ExecutionResult.Success;
        }
    }
}
