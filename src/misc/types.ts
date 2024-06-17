
export interface Project {
  nickname: string,
  aliases: string[],
  title: string,
  owner: string
  length: number,
  poster: string,
  type: 'TV' | 'Movie' | 'BD' | string,
  keyStaff: Staff[],
  episodes: Episode[],
  done: boolean,
  anidb: string,
  airTime: string, // '14:30'
  updateChannel: string,
  releaseChannel: string,
  observers: Observer[],
  administrators: string[]
};

export interface Episode {
  number: number,
  done: boolean,
  additionalStaff: Staff[],
  tasks: Task[],
  updated: number | undefined
};

export interface Staff {
  id: string,
  role: Role
};

export interface Role {
  abbreviation: string,
  title: string,
  weight: number
};

export type Task = {
  abbreviation: string,
  done: boolean
};

export type Observer = {
  guildId: string,
  updatesWebhook: string | undefined,
  releasesWebhook: string | undefined
};

export type ObserverAliasResult = {
  guildId: string | undefined,
  project: string | undefined
};

export type WeightedStatusEntry = {
  status: string,
  weight: number
};

export type Configuration = {
  progressDisplay: string | undefined
};

export type DatabaseData = {
  guilds: {[key:string]: {[key:string]: Project}},
  observers: {[key:string]: {[key:string]: string[]}},
  configuration: {[key:string]: Configuration},
  i18n: any
};
