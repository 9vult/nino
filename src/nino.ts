/**
 * Nino Fansub Management Bot
 * (c) 2020-2024 9volt
 */

import { Client, GatewayIntentBits } from "discord.js";
import interactionCreate from "./listeners/interactionCreate";
import ready from "./listeners/ready";
import { DatabaseData, Project } from "./misc/types";

require('dotenv').config();
var admin = require('firebase-admin');
var firebase = require('./firebase.json');

export const VERSION = "3.0.1";

admin.initializeApp({
  credential: admin.credential.cert(firebase),
  databaseURL: process.env.DATABASE_URL
});

var db = admin.database();

const client = new Client({
  intents: [
    GatewayIntentBits.Guilds,
    GatewayIntentBits.GuildMessages,
    GatewayIntentBits.MessageContent
  ]
});
export const CLIENT: Client = client;

let dbdata: DatabaseData = { guilds: {}};

db.ref('/Projects').on("value", function(data: {[key:string]:any}) {
  dbdata.guilds = data.val();
});

// Set up listeners
ready(client);
interactionCreate(client, db, dbdata);

client.login(process.env.TOKEN);
