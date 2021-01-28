
const fs = require('fs');

const execute = (message, args) => {
    if (!(args.length == 3 || args.length == 4)) { // Error checking
        message.channel.send("Use the right format, baka!");
        message.channel.send(this.syntax);
        return;
    }

    let nickname = args[0].trim().toLowerCase();
    let releasetype = args[1].trim().toLowerCase();
    let episodeNumber = '';
    let url = ''

    if (args.length == 4) { // Not a batch
        episodeNumber = args[2].trim();
        url = args[3].trim();
    } else { // A batch
        episodeNumber = '1';
        url = args[2].trim();
    }
    
    let releaseString = '';
    switch (releasetype) {
        case 'episode':
            releaseString = `Episode ${episodeNumber}`;
            break;
        case 'volume':
            releaseString = `Volume ${episodeNumber}`;
            break;
        case 'batch':
            releaseString = `Batch`;
            break;
        default:
            message.channel.send("Use the right format, baka!");
            message.channel.send(this.syntax);
            return;
    }

    fs.readFile(`./data/${nickname}/${nickname}_${episodeNumber}.json`, 'utf8', (err, jsonString) => {
        if (err) {
            console.log("Error reading file from disk:", err);
            return;
        }
        try {
            const episode = JSON.parse(jsonString);
            
            // Get the #releases channel ID
            let relchan = message.guild.channels.cache.find(channel => channel.name === "releases");
            relchan.send(`**${episode.title} - ${releaseString}**\n${url}`);

        } catch (err) {
            console.error(err);
        }
    });
}

module.exports = {
    name: 'release',
    description: 'Complete a task',
    syntax: '`.nino release Nickname <Episode/Volume/Batch> {EpisodeNumber/VolumeNumber} <ReleaseURL>` (number not needed for batch)',
    execute
};
