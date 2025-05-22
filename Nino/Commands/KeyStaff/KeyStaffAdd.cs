using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Utilities;
using static Localizer.Localizer;

namespace Nino.Commands;

public partial class KeyStaff
{
    [SlashCommand("add", "Add a new Key Staff to the whole project")]
    public async Task<RuntimeResult> Add(
        [Summary("project", "Project nickname"), Autocomplete(typeof(ProjectAutocompleteHandler))] string alias,
        [Summary("member", "Staff member")] SocketUser member,
        [Summary("abbreviation", "Position shorthand")] string abbreviation,
        [Summary("fullName", "Full position name")] string taskName,
        [Summary("isPseudo", "Position is a Pseudo-task")]bool isPseudo = false
    )
    {
        var interaction = Context.Interaction;
        var lng = interaction.UserLocale;

        // Sanitize inputs
        var memberId = member.Id;
        alias = alias.Trim();
        taskName = taskName.Trim();
        abbreviation = abbreviation.Trim().ToUpperInvariant().Replace("$", string.Empty);

        // Verify project and user - Owner or Admin required
        var project = Utils.ResolveAlias(alias, interaction);
        if (project == null)
            return await Response.Fail(T("error.alias.resolutionFailed", lng, alias), interaction);

        if (!Utils.VerifyUser(interaction.User.Id, project))
            return await Response.Fail(T("error.permissionDenied", lng), interaction);

        // Check if position already exists
        var additionalStaffs = Cache.GetEpisodes(project.Id).SelectMany(e => e.AdditionalStaff).ToHashSet();
        if (project.KeyStaff.Concat(additionalStaffs).Any(ks => ks.Role.Abbreviation == abbreviation))
            return await Response.Fail(T("error.positionExists", lng), interaction);

        // All good!
        var newStaff = new Staff
        {
            UserId = memberId,
            Role = new Role
            {
                Abbreviation = abbreviation,
                Name = taskName,
                Weight = (project.KeyStaff.Max(ks => ks.Role.Weight) ?? 0) + 1
            },
            IsPseudo = isPseudo
        };

        var newTask = new Records.Task
        {
            Abbreviation = abbreviation,
            Done = false
        };

        // Add to database
        await AzureHelper.PatchProjectAsync(project, [
            PatchOperation.Add("/keyStaff/-", newStaff)
        ]);

        var batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(project));
        foreach (var e in Cache.GetEpisodes(project.Id))
        {
            batch.PatchItem(id: e.Id.ToString(), [
                PatchOperation.Add("/tasks/-", newTask),
                PatchOperation.Set("/done", false)
            ]);
        }
        await batch.ExecuteAsync();

        Log.Info($"Added M[{memberId} (@{member.Username})] to {project} for {abbreviation} (IsPseudo={isPseudo})");

        // Send success embed
        var staffMention = $"<@{memberId}>";
        var embed = new EmbedBuilder()
            .WithTitle(T("title.projectCreation", lng))
            .WithDescription(T("keyStaff.added", lng, staffMention, abbreviation))
            .Build();
        await interaction.FollowupAsync(embed: embed);

        await Cache.RebuildCacheForProject(project.Id);
        return ExecutionResult.Success;
    }
}