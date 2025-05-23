﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Records;
using Nino.Records.Enums;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class ServerManagement
    {
        [SlashCommand("conga-prefix", "Set a prefix for Conga notifications")]
        public async Task<RuntimeResult> SetCongaPrefix(
            [Summary("type", "Prefix type")] CongaPrefixType type
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
            config.CongaPrefix = type;

            await AzureHelper.Configurations!.UpsertItemAsync(config);
            log.Info($"Updated configuration for guild {config.GuildId}, set Conga Prefix to {type.ToFriendlyString(lng)}");

            // Send success embed
            var embed = new EmbedBuilder()
                .WithTitle(T("title.serverConfiguration", lng))
                .WithDescription(T("server.configuration.saved", lng))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            await Cache.RebuildConfigCache();
            return ExecutionResult.Success;
        }
    }
}
