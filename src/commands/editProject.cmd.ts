import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { interp } from "../actions/interp.action";
import { GetStr } from "../actions/i18n.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";

export const EditProjectCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, locale } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const option = options.getString('option')!;
  const newValue = options.getString('newvalue')!;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { project } = InteractionData(dbdata, interaction, alias);

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${project}`);

  switch (option) {
    case 'Title':
      ref.update({ title: newValue });
      break;
    case 'Poster':
      ref.update({ poster: newValue });
      break;
    case 'UpdateChannel':
      ref.update({ updateChannel: newValue });
      break;
    case 'ReleaseChannel':
      ref.update({ releaseChannel: newValue });
      break;
    case 'AniDB':
      ref.update({ anidb: newValue });
      break;
    case 'AirTime24h':
      const isTime = /^([0-9]{2}):([0-9]{2})$/.test(newValue);
      if (!isTime) return fail(GetStr(dbdata.i18n, 'airTimeFail', locale), interaction);
      ref.update({ airTime: newValue });
      break;
  }

  const embed = new EmbedBuilder()
    .setTitle(GetStr(dbdata.i18n, 'projectModificationTitle', locale))
    .setDescription(interp(GetStr(dbdata.i18n, 'projectEdited', interaction.locale), { '$PROJECT': project }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}