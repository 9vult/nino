import { Database } from "@firebase/database-types";
import { DatabaseData } from "../misc/types";
import { AirDate } from "./airdate.action";
import { Client, EmbedBuilder, TextChannel } from "discord.js";

export const CheckReleases = async (client: Client, db: Database, dbdata: DatabaseData) => {
  for (let guildId in dbdata.guilds) {
    for (let projectid in dbdata.guilds[guildId]) {
      const projobj = dbdata.guilds[guildId][projectid];
      if (!projobj.airReminderEnabled) continue;
      if (!projobj.anidb) continue;
      if (projobj.done) continue;

      for (let episodeid in projobj.episodes) {
        const epobj = projobj.episodes[episodeid];
        if (epobj.airReminderPosted) continue;

        // Unmarked episode, check airing date
        const utcMillis = await AirDate(projobj.anidb, projobj.airTime, epobj.number, dbdata, 'en-US', true);
        // Aired
        if (Date.now() > Number(utcMillis)) {
          // Mark in db
          db.ref(`/Projects/${guildId}/${projectid}/episodes`).child(episodeid).update({ airReminderPosted: true });
          // Send notification
          const role = projobj.airReminderRole && projobj.airReminderRole !== '' ? `<@&${projobj.airReminderRole}>` : '';
          const embed = new EmbedBuilder()
            .setAuthor({ name: `${projobj.title} (${projobj.type})` })
            .setTitle(`Episode ${epobj.number} has aired!`)
            .setDescription(`${await AirDate(projobj.anidb, projobj.airTime, epobj.number, dbdata, 'en-US')}`)
            .setThumbnail(projobj.poster)
            .setTimestamp(Date.now());
          
          const publishChannel = client.channels.cache.get(projobj.airReminderChannel!);
          if (publishChannel?.isTextBased) {
            (publishChannel as TextChannel).send({ content: role, embeds: [embed] })
            .catch(err => console.error(`[CheckReleases]: "${err.message}" from guild ${guildId}, project ${projobj.nickname}`));
          }
        }
      }
    }
  }

};
