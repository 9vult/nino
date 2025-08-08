using Discord;

namespace Nino.Utilities.Extensions;

public static class EmbedExtensions
{
    /// <summary>
    /// Convert an Embed to a simple object for JSON serialization
    /// </summary>
    /// <param name="embed">Embed to convert</param>
    /// <returns>Simple object for JSON serialization</returns>
    public static object ToJsonObject(this Embed embed)
    {
        return new
        {
            author = new {
                name = embed.Author?.Name,
                url = embed.Author?.Url,
            },
            title = embed.Title,
            description = embed.Description,
            thumbnail = new {
                url = embed.Thumbnail?.Url
            },
            timestamp = embed.Timestamp?.ToString("yyyy-MM-ddTHH:mm:ss.ffffffzzz")
        };
    }
}