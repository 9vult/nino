// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Nino.Core.Features;
using Nino.Core.Features.Commands.AdditionalStaff.Add;
using Nino.Core.Features.Queries.Episode.Resolve;
using Nino.Core.Features.Queries.Project.Resolve;
using Nino.Domain;
using Nino.Localization;

namespace Nino.Discord.Interactions.AdditionalStaff;

public partial class AdditionalStaffModule
{
    [SlashCommand("add", "Add an Additional Staff to an episode")]
    public async Task<RuntimeResult> AddAsync(
        [MaxLength(Length.Alias)] string alias,
        [MaxLength(Length.EpisodeNumber)] string episodeNumber,
        SocketUser member,
        [MaxLength(Length.Abbreviation)] string abbreviation,
        [MaxLength(Length.RoleName)] string fullName,
        bool isPseudo = false
    )
    {
        var interaction = Context.Interaction;
        var locale = interaction.UserLocale;

        // Cleanup
        alias = alias.Trim();
        episodeNumber = episodeNumber.Trim();
        abbreviation = abbreviation.Trim().ToUpperInvariant();
        fullName = fullName.Trim();

        var (requestedBy, groupId) = await interactionIdService.GetUserAndGroupAsync(interaction);

        var resolve = await projectResolver
            .HandleAsync(new ResolveProjectQuery(alias, groupId, requestedBy))
            .ThenAsync(prjId =>
                episodeResolver.HandleAsync(new ResolveEpisodeQuery(prjId, episodeNumber))
            );

        if (!resolve.IsSuccess)
        {
            Dictionary<string, object> errorParams = new()
            {
                ["alias"] = alias,
                ["number"] = episodeNumber,
            };
            return await interaction.FailAsync(
                resolve.Status switch
                {
                    ResultStatus.ProjectNotFound => "project.resolution.failed",
                    ResultStatus.EpisodeNotFound => "episode.resolution.failed",
                    _ => "error.generic",
                },
                locale,
                errorParams
            );
        }

        var (projectId, episodeId) = resolve.Value;

        var memberId = await identityService.GetOrCreateUserByDiscordIdAsync(
            member.Id,
            member.Username
        );

        var command = new AddAdditionalStaffCommand(
            ProjectId: projectId,
            EpisodeId: episodeId,
            RequestedBy: requestedBy,
            Abbreviation: abbreviation,
            Name: fullName,
            MemberId: memberId,
            IsPseudo: isPseudo
        );

        var result = await addHandler.HandleAsync(command);

        if (!result.IsSuccess)
        {
            Dictionary<string, object> errorParams = new() { ["abbreviation"] = abbreviation };
            return await interaction.FailAsync(
                result.Status switch
                {
                    ResultStatus.Unauthorized => "error.permissions",
                    ResultStatus.Conflict => "additionalStaff.creation.conflict",
                    _ => "error.generic",
                },
                locale,
                errorParams
            );
        }

        var (projectTitle, projectType, _) = result.Value;
        var header = $"{projectTitle} ({projectType.ToFriendlyString(locale)})";

        // Success!
        var staffMention = $"<@{member.Id}>";
        var successEmbed = new EmbedBuilder()
            .WithAuthor(header)
            .WithTitle(T("project.modification.title", locale))
            .WithDescription(
                T(
                    "additionalStaff.creation.success",
                    locale,
                    staffMention,
                    abbreviation,
                    episodeNumber
                )
            )
            .Build();

        await interaction.FollowupAsync(embed: successEmbed);
        return ExecutionResult.Success;
    }
}
