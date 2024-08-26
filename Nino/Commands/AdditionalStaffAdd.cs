using Discord;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Utilities;
using Nino.Records;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Localizer.Localizer;
using System.Collections;

namespace Nino.Commands
{
    internal static partial class AdditionalStaff
    {

        public static async Task<bool> HandleAdd(SocketSlashCommand interaction, Episode episode)
        {
            var lng = interaction.UserLocale;

            var subcommand = interaction.Data.Options.First();

            // Get inputs
            var memberId = ((SocketGuildUser)subcommand.Options.FirstOrDefault(o => o.Name == "member")!.Value).Id;
            var abbreviation = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "abbreviation")!.Value).Trim().ToUpperInvariant();
            var title = ((string)subcommand.Options.FirstOrDefault(o => o.Name == "name")).Trim();

            // Check if position already exists
            if (episode.AdditionalStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.positionExists", lng), interaction);

            // All good!
            var newStaff = new Staff
            {
                UserId = memberId,
                Role = new Role
                {
                    Abbreviation = abbreviation,
                    Name = title
                }
            };

            var newTask = new Records.Task
            {
                Abbreviation = abbreviation,
                Done = false
            };

            // Add to database
            TransactionalBatch batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(episode));
            batch.PatchItem(id: episode.Id, new[]
            {
                PatchOperation.Add("/additionalStaff/-", newStaff),
                PatchOperation.Add("/tasks/-", newTask)
            });
            await batch.ExecuteAsync();

            log.Info($"Added {memberId} to {episode.Id} for {abbreviation}");

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectCreation", lng))
                .WithDescription(T("additionalStaff.added", lng, staffMention, abbreviation, episode.Number))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return true;
        }
    }
}
