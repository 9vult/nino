// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;
using Nino.Core.Features;
using Nino.Discord.Entities;

namespace Nino.Discord.Interactions;

public static class InteractionExtensions
{
    extension(IDiscordInteraction interaction)
    {
        public async Task<RuntimeResult> FailAsync(string message, bool ephemeral = false)
        {
            var embed = new EmbedBuilder()
                .WithTitle("Baka.")
                .WithDescription(message)
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed, ephemeral: ephemeral);
            return ExecutionResult.Failure;
        }

        public async Task<RuntimeResult> FailAsync(
            string localizationKey,
            string locale,
            Dictionary<string, object> localizationArgs,
            bool ephemeral = false
        )
        {
            var embed = new EmbedBuilder()
                .WithTitle("Baka.")
                .WithDescription(T(localizationKey, locale, localizationArgs))
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed, ephemeral: ephemeral);
            return ExecutionResult.Failure;
        }

        public async Task<RuntimeResult> FailAsync(
            ResultStatus status,
            string locale,
            FailureContext? context = null,
            bool ephemeral = false
        )
        {
            var key = status switch
            {
                ResultStatus.Unauthorized => "error.permissions",
                ResultStatus.Archived => "project.archived",
                ResultStatus.ProjectConflict => "project.conflict",
                ResultStatus.EpisodeConflict => "episode.conflict",
                ResultStatus.TaskConflict => "task.conflict",
                ResultStatus.ProjectNotFound => "project.notFound",
                ResultStatus.EpisodeNotFound => "episode.notFound",
                ResultStatus.TaskNotFound => "task.notFound",
                ResultStatus.TemplateStaffNotFound => "templateStaff.notFound",
                ResultStatus.NotFound => "error.notFound",
                ResultStatus.AniListError => "error.aniListError",
                ResultStatus.BadRequest => "error.badRequest",
                ResultStatus.ProjectResolutionFailed => "project.resolutionFailed",
                ResultStatus.EpisodeResolutionFailed => "episode.resolutionFailed",
                ResultStatus.TaskResolutionFailed => "task.resolutionFailed",
                ResultStatus.TemplateStaffResolutionFailed => "templateStaff.resolutionFailed",
                ResultStatus.MissingProjectChannel => "error.missingProjectChannel",
                ResultStatus.ObserverNotFound => "observer.notFound",
                _ => "error.generic",
            };

            if (context?.Overrides?.TryGetValue(status, out var value) is true)
                key = value;

            var args = (context ?? new FailureContext()).ToLocalizationArgs();
            args["errorCode"] = status.ToString();

            var embed = new EmbedBuilder()
                .WithTitle("Baka.")
                .WithDescription(T(key, locale, args))
                .WithColor(0xd797ff)
                .Build();
            await interaction.FollowupAsync(embed: embed, ephemeral: ephemeral);
            return ExecutionResult.Failure;
        }
    }
}
