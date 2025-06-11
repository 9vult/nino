using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.Cosmos;
using Nino.Handlers;
using Nino.Records;
using Nino.Records.Enums;
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

        var newUndoneTask = new Records.Task
        {
            Abbreviation = abbreviation,
            Done = false
        };
        
        var newDoneTask = new Records.Task
        {
            Abbreviation = abbreviation,
            Done = true
        };

        var projectEpisodes = Cache.GetEpisodes(project.Id);
        var markDoneIfEpisodeIsDone = false;
        
        if (projectEpisodes.Any(e => e.Done))
        {
            var (response, finalBody, questionMessage) 
                = await Ask.AboutAction(interactive, interaction, project, lng,  Ask.InconsequentialAction.MarkTaskDoneIfEpisodeIsDone);
            
            markDoneIfEpisodeIsDone = response;
            
            // Update the question embed to reflect the choice
            if (questionMessage is not null)
            {
                var header = project.IsPrivate
                    ? $"🔒 {project.Title} ({project.Type.ToFriendlyString(lng)})"
                    : $"{project.Title} ({project.Type.ToFriendlyString(lng)})";
                var editedEmbed = new EmbedBuilder()
                    .WithAuthor(header)
                    .WithTitle($"❓ {T("progress.done.inTheDust.question", lng)}")
                    .WithDescription(finalBody)
                    .WithCurrentTimestamp()
                    .Build();
                await questionMessage.ModifyAsync(m => {
                    m.Components = null;
                    m.Embed = editedEmbed;
                });
            }
        }

        // Add to database
        await AzureHelper.PatchProjectAsync(project, [
            PatchOperation.Add("/keyStaff/-", newStaff)
        ]);

        var batch = AzureHelper.Episodes!.CreateTransactionalBatch(partitionKey: AzureHelper.EpisodePartitionKey(project));
        foreach (var episode in projectEpisodes)
        {
            var taskDone = markDoneIfEpisodeIsDone && episode.Done;
            
            batch.PatchItem(id: episode.Id.ToString(), [
                PatchOperation.Add("/tasks/-", taskDone ? newDoneTask : newUndoneTask),
                PatchOperation.Set("/done", episode.Done && taskDone)
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