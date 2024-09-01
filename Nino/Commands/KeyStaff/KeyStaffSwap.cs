using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Records;
using Nino.Utilities;

using static Localizer.Localizer;

namespace Nino.Commands
{
    public partial class KeyStaff
    {
        [SlashCommand("swap", "Swap a Key Staff into the whole project")]
        public async Task<RuntimeResult> Swap(
            [Summary("project", "Project nickname")] string alias,
            [Summary("abbreviation", "Position shorthand")] string abbreviation,
            [Summary("member", "Staff member")] SocketUser member
        )
        {
            var interaction = Context.Interaction;
            var lng = interaction.UserLocale;

            // Sanitize imputs
            var memberId = member.Id;
            alias = alias.Trim();
            abbreviation = abbreviation.Trim().ToUpperInvariant();

            // Verify project and user - Owner or Admin required
            var project = Utils.ResolveAlias(alias, interaction);
            if (project == null)
                return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

            if (!Utils.VerifyUser(interaction.User.Id, project))
                return await Response.Fail(T("error.permissionDenied", lng), interaction);
                
            // Check if position exists
            if (!project.KeyStaff.Any(ks => ks.Role.Abbreviation == abbreviation))
                return await Response.Fail(T("error.noSuchTask", lng, abbreviation), interaction);

            // Update user
            var updatedStaff = project.KeyStaff.Single(k => k.Role.Abbreviation == abbreviation);
            var ksIndex = Array.IndexOf(project.KeyStaff, updatedStaff);

            updatedStaff.UserId = memberId;

            // Swap in database
            await AzureHelper.Projects!.PatchItemAsync<Project>(id: project.Id, partitionKey: AzureHelper.ProjectPartitionKey(project),
                patchOperations: [
                PatchOperation.Replace($"/keyStaff/{ksIndex}", updatedStaff)
            ]);

            log.Info($"Swapped {memberId} in to {project.Id} for {abbreviation}");

            // Send success embed
            var staffMention = $"<@{memberId}>";
            var embed = new EmbedBuilder()
                .WithTitle(T("title.projectModification", lng))
                .WithDescription(T("keyStaff.swapped", lng, staffMention, abbreviation))
                .Build();
            await interaction.FollowupAsync(embed: embed);

            return ExecutionResult.Success;
        }
    }
}
