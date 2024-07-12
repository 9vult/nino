import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";

export const AddObserverCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, user, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const observingGuild = options.getString('guild')!;
  const updatesWH: string | null = options.getString('updates');
  const relesesWH: string | null = options.getString('releases');

  let verification = await VerifyInteraction(dbdata, interaction, alias, false, false);
  if (!verification) return;

  const { project } = InteractionData(dbdata, interaction, alias);

  db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`).child('observers')
    .push({ guildId: observingGuild, updatesWebhook: updatesWH, releasesWebhook: relesesWH, managerid: `${user.id}` });

  const ref = db.ref(`/Observers`).child(`${observingGuild}`);
  if (dbdata.observers 
      && dbdata.observers[observingGuild] 
      && dbdata.observers[observingGuild][guildId]
    ) {
      let data: {[key:string]:string[]} = {};
      data[guildId] = [...dbdata.observers[observingGuild][guildId], project];
      ref.update(data);
    }
  else {
    let data: {[key:string]:string[]} = {};
    data[guildId] = [project];
    ref.update(data)
  }

  const embed = new EmbedBuilder()
    .setTitle(t('projectModificationTitle', { lng }))
    .setDescription(t('addObserver', { lng, observingGuild, project }))
    .setColor(0xd797ff);
  
  (await interaction.editReply("OK")).delete(); // Remove any webhook URLs from the log
  await interaction.channel?.send({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}