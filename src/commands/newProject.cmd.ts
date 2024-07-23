import { ChatInputCommandInteraction, Client, EmbedBuilder, GuildMember, PermissionsBitField } from "discord.js";
import { generateAllowedMentions } from "../actions/generateAllowedMentions.action";
import { DatabaseData, Project } from "../misc/types";
import { fail } from "../actions/fail.action";
import { Database } from "@firebase/database-types";
import { t } from "i18next";

export const NewProjectCmd = async (client: Client, db: Database, dbdata: DatabaseData, interaction: ChatInputCommandInteraction) => {
  if (!interaction.isCommand()) return;
  const { options, user, member, guildId, locale: lng } = interaction;
  if (guildId == null) return;

  await interaction.deferReply();

  if (!(member as GuildMember)?.permissions.has(PermissionsBitField.Flags.Administrator)) {
    return fail(t('error.notPrivileged', { lng }), interaction);
  }

  const nickname = options.getString('nickname')!;
  const title = options.getString('title')!;
  const owner = String(user!.id);
  const type = options.getString('projecttype')!;
  const length = options.getNumber('length')!;
  const poster = options.getString('poster')!;
  const isPrivate = options.getBoolean('private')!;
  const updateChannel = options.getChannel('updatechannel')!.id;
  const releaseChannel = options.getChannel('releasechannel')!.id;

  const ref = db.ref(`/Projects/`).child(`${guildId}`).child(`${nickname}`);
  const newProj: Project = {
    nickname,
    title,
    aliases: [],
    owner,
    length,
    poster,
    type,
    keyStaff: [],
    episodes: [],
    done: false,
    anidb: '', // TODO
    airTime: '00:00',
    updateChannel,
    releaseChannel,
    observers: [],
    administrators: [],
    airReminderEnabled: false,
    airReminderRole: '',
    airReminderChannel: '',
    isPrivate
  };
  ref.set(newProj);

  let epref = ref.child('episodes');
  for (let i = 1; i < length + 1; i++) {
    epref.push({
      number: i,
      done: false,
      additionalStaff: [],
      tasks: [],
      updated: 0,
      airReminderPosted: false
    });
  }

  const embed = new EmbedBuilder()
    .setTitle(t('title.projectCreation', { lng }))
    .setDescription(t('project.created', { lng, nickname }))
    .setColor(0xd797ff);
  await interaction.editReply({ embeds: [embed], allowedMentions: generateAllowedMentions([[], []]) });
}