// SPDX-License-Identifier: MPL-2.0

using Discord;
using Discord.Interactions;

namespace Nino.Discord.Interactions;

public class VogenTypeConverter<T> : TypeConverter<T>
    where T : struct, IVogen<T, string>
{
    public override ApplicationCommandOptionType GetDiscordType() =>
        ApplicationCommandOptionType.String;

    public override Task<TypeConverterResult> ReadAsync(
        IInteractionContext context,
        IApplicationCommandInteractionDataOption option,
        IServiceProvider services
    )
    {
        if (option.Value is not string raw)
            return Task.FromResult(
                TypeConverterResult.FromError(
                    InteractionCommandError.ParseFailed,
                    "Expected a string value"
                )
            );

        var validation = T.TryFrom(raw, out var value);

        return Task.FromResult(
            validation
                ? TypeConverterResult.FromSuccess(value)
                : TypeConverterResult.FromError(
                    InteractionCommandError.ParseFailed,
                    "Validation failed"
                )
        );
    }
}
