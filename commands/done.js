
const Discord = require('discord.js');
const fs = require('fs');
const moment = require('moment');

const execute = (message, args) => {
    if (!(args.length == 3)) { // Error checking
        message.channel.send("Use the right format, baka!");
        message.channel.send(this.syntax);
        return;
    }

    let nickname = args[0].trim().toLowerCase();
    let episodeNumber = parseInt(args[1].trim());
    let task = args[2].trim().toUpperCase();

    fs.readFile(`./data/${nickname}/${nickname}_${episodeNumber}.json`, 'utf8', (err, jsonString) => {
        if (err) {
            console.log("Error reading file from disk:", err);
            return;
        }
        try {
            const episode = JSON.parse(jsonString);
            episode[task] = true;
            const jsonStringNew = JSON.stringify(episode);
            
            // write changes
            fs.writeFileSync(`./data/${nickname}/${nickname}_${episodeNumber}.json`, jsonStringNew, err => {if (err) console.error(err);});
            message.channel.send(`Nice job getting the ${task} done.`);

            // Generate status string
            let statusString = '';
            for (var key in episode) {
                if (!(key == "title" || key == "number" || key == "complete")) { // then it's a task
                    if (episode[key] == false) {statusString = statusString + `**${key}** `;}
                    if (episode[key] == true) {
                        if (key == task) { // what we just did
                            statusString = statusString + `__~~${key}~~__ `;
                        } else {
                            statusString = statusString + `~~${key}~~ `;
                        }
                    }
                }
            }

            // Get the date
            let timestamp = moment().format('MMMM D, YYYY h:mm:ss a');
            // Create embed
            const updateEmbed = new Discord.MessageEmbed()
                .setAuthor(episode.title)
                .setTitle(`Episode ${episodeNumber}`)
                .setDescription(statusString)
                .setFooter(timestamp);
            
                // Get the #progress channel ID
            let progchan = message.guild.channels.cache.find(channel => channel.name === "progress");
            progchan.send(updateEmbed);

        } catch (err) {
            console.error(err);
        }
    });
}

module.exports = {
    name: 'done',
    description: 'Complete a task',
    syntax: '`.nino done Nickname EpisodeNumber Task`',
    execute
};
