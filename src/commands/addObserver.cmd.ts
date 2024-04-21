import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project, Task } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";

export const AddObserverCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, member, guildId } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();
  const locale = interaction.locale;

  const project = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const observingGuild = options.getString('guild')!;
  const updatesWH: string | null = options.getString('updates');
  const relesesWH: string | null = options.getString('releases');

  if (guildId == null || !(guildId in dbdata.guilds))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchGuild', locale), { '$GUILDID': guildId }), interaction);

  let projects = dbdata.guilds[guildId];

  if (!project || !(project in projects))
    return fail(interp(GetStr(dbdata.i18n, 'noSuchproject', interaction.locale), { '$PROJECT': project }), interaction);
  if (projects[project].owner !== user!.id)
    return fail(GetStr(dbdata.i18n, 'permissionDenied', locale), interaction);

  db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`).child('observers')
    .push({ guildId: observingGuild, updatesWebhook: updatesWH, releasesWebhook: relesesWH });

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
    .setTitle(GetStr(dbdata.i18n, 'projectModificationTitle', locale))
    .setDescription(interp(GetStr(dbdata.i18n, 'addObserver', interaction.locale), { '$OBSERVINGGUILD': observingGuild, '$PROJECT': project }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}