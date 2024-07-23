import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, ObservedProject } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { t } from "i18next";

export const RemoveObserverCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, member, guildId: observingGuildId, user, locale: lng } = interaction;
  if (observingGuildId == null) return;

  await interaction.deferReply();

  if (!(member as GuildMember)?.permissions.has(PermissionsBitField.Flags.Administrator)) {
    return fail(t('error.notPrivileged', { lng }), interaction);
  }

  const originGuildId = options.getString('guild')!;
  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!, originGuildId);

  if (originGuildId == null || !(originGuildId in dbdata.guilds))
    return await fail(t('error.noSuchGuild', { lng, guildId: originGuildId }), interaction);
 
  let projects = dbdata.guilds[originGuildId];
    if (!alias || !(alias in projects))
      return await fail(t('error.noSuchProject', { lng }), interaction);

  const project = alias;

  let success = false;
  for (let observerid in projects[project].observers) {
    const observer = projects[project].observers[observerid];
    if (observer.guildId == observingGuildId) {
      success = true;
      db.ref(`/Projects/`).child(`${originGuildId}`).child(`${project}`).child('observers').child(observerid).remove();

      const ref = db.ref(`/Observers`).child(`${observingGuildId}`);
      let data: {[key:string]:ObservedProject[]} = {};
      data[originGuildId] = dbdata.observers[observingGuildId][originGuildId].filter(o => o.name !== project);
      ref.update(data);
    }
  }

  if (!success) return fail(t('error.noSuchObserver', { lng }), interaction);

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(t('observer.removed', { lng, originGuildId, project }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}