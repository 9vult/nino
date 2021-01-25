
module.exports = {
    name: 'help',
    description: 'Prints Help info',
    syntax: '`.nino help`',
    execute(message, args, Discord, fs, moment) {
        // .nino help        
        const updateEmbed = new Discord.MessageEmbed()
            .setTitle("Nino Help")
            .setDescription("Command List: about, add, complete, done, help")
            .setFooter("More help coming soon.");
        
        message.channel.send(updateEmbed);
    }
}
