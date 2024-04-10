import { DatabaseData, WeightedStatusEntry } from "../misc/types";

export const GenerateEntries = (dbdata: DatabaseData, guildId: string, project: string, episode: number) => {
  let projects = dbdata.guilds[guildId];
  let entries: {[key:string]:WeightedStatusEntry} = {};

  if (projects[project].keyStaff) {
    Object.values(projects[project].keyStaff).forEach((ks, i) => { 
      entries[ks.role.abbreviation] = {
        status: '',
        weight: ks.role.weight ?? i
      };
    });
  }

  for (let ep in projects[project].episodes) {
    if (projects[project].episodes[ep].number == episode) {
      let projObj = projects[project].episodes[ep];
      if (projObj.additionalStaff) {
        Object.values(projObj.additionalStaff).forEach(as => { 
          entries[as.role.abbreviation] = {
            status: '',
            weight: Number.POSITIVE_INFINITY
          };
        });
      }
      break;
    }
  }
  return entries;
}

export const EntriesToStatusString = (entries: {[key:string]:WeightedStatusEntry}, delim: string = ' ') => {
  return Object.values(entries)
    .sort((a, b) => a.weight - b.weight)
    .reduce((acc, item) => { return acc += `${item.status}${delim}` }, "")
    .trim();
};