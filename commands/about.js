
const Discord = require('discord.js');

const execute = (message, args) => {
    const updateEmbed = new Discord.MessageEmbed()
    .setTitle("Nino v1.0.0")
    .setDescription("by 9volt");

    message.channel.send(updateEmbed);
}

module.exports = {
    name: 'about',
    description: 'Prints About info',
    syntax: '`.nino about`',
    execute
};
