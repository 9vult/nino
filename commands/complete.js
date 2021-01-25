
module.exports = {
    name: 'complete',
    description: 'Complete a task',
    syntax: '`.nino complete Nickname EpisodeNumber`',
    execute(message, args, Discord, fs, moment) {
        // .nino finish GGO 12
        if (!(args.length == 2)) { // Error checking
            message.channel.send("Use the right format, baka!");
            message.channel.send(this.syntax);
            return;
        }

        let nickname = args[0].trim().toLowerCase();
        let episodeNumber = parseInt(args[1].trim());

        fs.readFile(`./data/${nickname}/${nickname}_${episodeNumber}.json`, 'utf8', (err, jsonString) => {
            if (err) {
                console.log("Error reading file from disk:", err);
                return;
            }
            try {
                const episode = JSON.parse(jsonString);
                message.channel.send(`Wow, you completed a _whole episode_.`);
                episode["complete"] = true; // mark episode as complete
                
                // Mark all tasks as complete
                for (var key in episode) {
                    if (!(key == "title" || key == "number" || key == "complete")) { // then it's a task
                        episode[key] = true;
                    }
                }

                
                const jsonStringNew = JSON.stringify(episode);
                fs.writeFileSync(`./data/${nickname}/${nickname}_${episodeNumber}.json`, jsonStringNew, err => {if (err) console.error(err);});

                // Get the date
                let timestamp = moment().format('MMMM D, YYYY h:mm:ss a');
                // Create embed
                const updateEmbed = new Discord.MessageEmbed()
                    .setAuthor(episode.title)
                    .setTitle(`Episode ${episodeNumber}`)
                    .setDescription(`${episode.title} #${episodeNumber} is complete!`)
                    .setFooter(timestamp);

                // Get the #progress channel ID
                let progchan = message.guild.channels.cache.find(channel => channel.name === "progress");
                progchan.send(updateEmbed);

            } catch (err) {
                console.error(err);
            }
        });
        
    }
}
