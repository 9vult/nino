
const Discord = require('discord.js');

const execute = (message, args) => {
    const updateEmbed = new Discord.MessageEmbed()
    .setTitle("Nino Help")
    .setDescription("Command List: about, add, complete, done, help")
    .setFooter("More help coming soon.");

    message.channel.send(updateEmbed);
}
module.exports = {
    name: 'help',
    description: 'Prints Help info',
    syntax: '`.nino help`',
    execute
};
