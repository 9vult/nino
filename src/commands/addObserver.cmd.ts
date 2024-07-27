import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField, Role } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, ObservedProject } from "../misc/types";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { fail } from "../actions/fail.action";
import { t } from "i18next";
import { getAllPrivilegedIds } from "../actions/getters";

export const AddObserverCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, member, guildId: observingGuildId, user, locale: lng } = interaction;
  if (observingGuildId == null) return;

  await interaction.deferReply();

  if (!(member as GuildMember)?.permissions.has(PermissionsBitField.Flags.Administrator)) {
    return fail(t('error.notPrivileged', { lng }), interaction);
  }

  const originGuildId = options.getString('guild')!;
  const blame = options.getBoolean('blame')!;
  const updatesWH: string = options.getString('updates') ?? '';
  const relesesWH: string = options.getString('releases') ?? '';
  const releaseRole: string = options.getRole('role') ? (options.getRole('role')!.name == '@everyone' ? "@everyone" : options.getRole('role')!.id) : '';
  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!, originGuildId);

  if (!blame && updatesWH == '' && relesesWH == '') {
    // no-op condition
    return await fail(t('error.observerNoOp', { lng }), interaction);
  }

  if (originGuildId == null || !(originGuildId in dbdata.guilds))
    return await fail(t('error.noSuchGuild', { lng, guildId: originGuildId }), interaction);
 
  let projects = dbdata.guilds[originGuildId];
  if (!alias || !(alias in projects))
    return await fail(t('error.noSuchProject', { lng }), interaction);

  if (projects[alias].isPrivate) {
    let privilegedIds = getAllPrivilegedIds(projects[alias], dbdata.configuration[originGuildId!]?.administrators ?? []);
    if (!privilegedIds.includes(user.id))
      return await fail(t('error.noSuchProject', { lng }), interaction);
  }

  const projectName = alias;

  db.ref(`/Projects/`).child(`${originGuildId}`).child(`${projectName}`).child('observers')
    .push({ guildId: observingGuildId, updatesWebhook: updatesWH, releasesWebhook: relesesWH, managerid: user.id, releaseRole });

  const nameAndBlame: ObservedProject = { name: projectName, blame };
  const ref = db.ref(`/Observers`).child(`${observingGuildId}`);
  if (dbdata.observers 
      && dbdata.observers[observingGuildId] 
      && dbdata.observers[observingGuildId][originGuildId]
    ) {
      let data: {[key:string]:ObservedProject[]} = {};
      data[originGuildId] = [...dbdata.observers[observingGuildId][originGuildId], nameAndBlame];
      ref.update(data);
    }
  else {
    let data: {[key:string]:ObservedProject[]} = {};
    data[originGuildId] = [nameAndBlame];
    ref.update(data)
  }

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(t('observer.added', { lng, originGuildId, project: projectName }))
    .setColor(0xd797ff);
  
  (await interaction.editReply("OK")).delete(); // Remove any webhook URLs from the log
  await interaction.channel?.send({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}