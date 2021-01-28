
const fs = require('fs');

const execute = (message, args) => {
    let input = args.join(' ');
    let splits = input.split('|');

    if (!(splits.length == 4)) { // Error checking
        message.channel.send("Use the right format, baka!");
        message.channel.send(this.syntax);
        return;
    }

    let fullname = splits[0].trim();
    let nickname = splits[1].trim().toLowerCase();
    let originalNick = splits[1].trim();
    let episodecount = parseInt(splits[2].trim());
    let tasks = splits[3].trim().toUpperCase().split(/ +/);

    // Make directory
    fs.mkdirSync(`./data/${nickname}`, { recursive: true }, err => {if (err) console.error(err);});
    for (i = 1; i < episodecount+1; i++) {
        let episode = {
            title: fullname,
            number: i,
            complete: false
        };
        for (t = 0; t < tasks.length; t++) {
            episode[tasks[t]] = false;
        }

        const jsonString = JSON.stringify(episode);

        fs.writeFileSync(`./data/${nickname}/${nickname}_${i}.json`, jsonString, err => {if (err) console.error(err);});
    }
    message.channel.send(`I went ahead and made "${originalNick}" for you...`);
}

module.exports = {
    name: 'add',
    description: 'Add a title',
    syntax: '`.nino add FullName | Nickname | EpisodeCount | Task1 Task2 ...`',
    execute
};
