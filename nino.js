
/**
 *
 * Nino.js Discord bot
 * (c) 2021 9volt
 *  
 */

const Discord = require('discord.js');
const fs = require('fs');
const moment = require('moment');

const client = new Discord.Client();
const prefix = '.nino';

// Load command files
let commands = new Discord.Collection();
const commandFiles = fs.readdirSync('./commands/').filter(file => file.endsWith('.js'));
for (const file of commandFiles) {
    let command = require(`./commands/${file}`);
    commands.set(command.name, command);
}

// Startup
client.once('ready', () => { console.log("Nino is now online!"); });

// Read messages
client.on('message', message => {
    if (!message.content.startsWith(prefix) || message.author.bot) return;

    const args = message.content.slice(prefix.length).trim().split(/ +/);
    const command = args.shift().toLowerCase();

    // Execute the command if the user has the Quintuplet role
    if (message.member.roles.cache.find(r => r.name === "Quintuplet"))
    {
        let cmd = commands.get(command)
        if (cmd) cmd.execute(message, args);
    }
});





// Log in with client token
fs.readFile('token.txt', 'utf8', (err, data) => {
    if (err) {console.error(err); return; }
    client.login(data);
})
