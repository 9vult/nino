// SPDX-License-Identifier: MPL-2.0

using System.Text;
using Discord;
using Nino.Core.Features.Queries.Projects.GetGenericData;
using Nino.Localization;

namespace Nino.Discord.Interactions;

public static class InteractionModuleBaseExtensions
{
    public static EmbedBuilder WithProjectInfo(
        this EmbedBuilder embed,
        GetGenericProjectDataResponse data,
        string locale
    )
    {
        var header = new StringBuilder();

        if (data.IsPrivate)
            header.Append("🔒 ");

        header.Append($"{data.ProjectTitle} ({data.ProjectType.ToFriendlyString(locale)})");

        return embed
            .WithAuthor(name: header.ToString(), url: data.AniListUrl)
            .WithThumbnailUrl(data.PosterUrl)
            .WithCurrentTimestamp();
    }
}
