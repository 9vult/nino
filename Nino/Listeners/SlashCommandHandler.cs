using Discord.WebSocket;
using Nino.Commands;

namespace Nino.Listeners
{
    internal static partial class Listener
    {
        public static async Task SlashCommandExecuted(SocketSlashCommand interaction)
        {
            await interaction.DeferAsync();

            var guildId = interaction.GuildId;
            if (guildId == null)
            {
                await interaction.FollowupAsync("Nino commands must be run in a server!");
                return;
            }

            switch (interaction.CommandName)
            {
                case ProjectManagement.Name:
                    await ProjectManagement.Handle(interaction);
                    break;
                case KeyStaff.Name:
                    await KeyStaff.Handle(interaction);
                    break;
                case AdditionalStaff.Name:
                    await AdditionalStaff.Handle(interaction);
                    break;
                case About.Name:
                    await About.Handle(interaction);
                    break;
                case Help.Name:
                    await Help.Handle(interaction);
                    break;
            }
        }
    }
}
