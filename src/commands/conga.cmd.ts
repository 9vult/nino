import { ChatInputCommandInteraction, Client, EmbedBuilder } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { GetAlias } from "../actions/getalias.action";
import { InteractionData, VerifyInteraction } from "../actions/verify.action";
import { t } from "i18next";
import { getKeyStaff, isCongaParticipant } from "../actions/getters";

export const CongaCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, guildId, member, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  const alias = await GetAlias(db, dbdata, interaction, options.getString('project')!);
  const subcommand = options.getSubcommand()!;

  let verification = await VerifyInteraction(dbdata, interaction, alias);
  if (!verification) return;
  const { projects, project: projectName } = InteractionData(dbdata, interaction, alias);
  const project = projects[projectName];

  let message = '';

  switch (subcommand) {
    case 'add':
      const current = options.getString('abbreviation')!.toUpperCase();
      const next = options.getString('next')!.toUpperCase();
      
      if (isCongaParticipant(project, current)) return fail(t('error.conga.alreadyExists', { lng }), interaction);
      if (!getKeyStaff(project, current).id) return fail(t('error.noSuchTask', { lng, abbreviation: current }), interaction);
      if (!getKeyStaff(project, next).id) return fail(t('error.noSuchTask', { lng, abbreviation: next }), interaction);

      db.ref(`/Projects/`).child(`${guildId}`).child(`${projectName}`).child('conga').push({ current, next });

      message = t('conga.added', { lng, current, next });
      break;

    case 'remove':
      const abbreviation = options.getString('abbreviation')!.toUpperCase();

      let success = false;

      if (project.conga) {
        for (let id in project.conga) {
          const participant = project.conga[id];
          if (participant.current == abbreviation) {
            success = true;
            db.ref(`/Projects/`).child(`${guildId}`).child(`${projectName}`).child('conga').child(id).remove();
          }
        }
      }

      if (!success) return fail(t('error.noSuchConga', { lng }), interaction);

      message = t('conga.removed', { lng, current: abbreviation });
      break;
  }

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectModification', { lng }))
    .setDescription(message)
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}