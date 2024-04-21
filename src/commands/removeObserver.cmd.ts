import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project, Task } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";

export const RemoveObserverCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const observingGuild = options.getString('guild')!;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project } = InteractionData(dbdata, interaction, alias);

  let success = false;
  for (let observerid in projects[project].observers) {
    const observer = projects[project].observers[observerid];
    if (observer.guildId == observingGuild) {
      success = true;
      db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`).child('observers').child(observerid).remove();

      const ref = db.ref(`/Observers`).child(`${observingGuild}`);
      let data: {[key:string]:string[]} = {};
      data[guildId] = dbdata.observers[observingGuild][guildId].filter(o => o !== project);
      ref.update(data);
    }
  }

  if (!success) return fail(GetStr(dbdata.i18n, 'noSuchObserver', interaction.locale), interaction);

  const embed = new EmbedBuilder()
    .setTitle(GetStr(dbdata.i18n, 'projectModificationTitle', locale))
    .setDescription(interp(GetStr(dbdata.i18n, 'removeObserver', interaction.locale), { '$OBSERVINGGUILD': observingGuild, '$PROJECT': project }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}