import { readdirSync } from "fs"
import path from "path";

export const GetNames = (dict: {[key:string]:any}, cat: string, key: string) => {
  let results: {[key:string]:string} = {};
  for (let definitions of Object.values(dict)) {
    if (!definitions[cat]) continue;
    if (!definitions[cat][key]) continue;
    results[definitions['lang']] = definitions[cat][key]['name'];
  }
  return results;
};

export const GetDescriptions = (dict: {[key:string]:any}, cat: string, key: string) => {
  let results: {[key:string]:string} = {};
  for (let definitions of Object.values(dict)) {
    if (!definitions[cat]) continue;
    if (!definitions[cat][key]) continue;
    results[definitions['lang']] = definitions[cat][key]['description'];
  }
  return results;
};

export const GetStr = (dict: {[key:string]:any}, key: string, locale: string) => {
  let definitions = dict[locale];
  if (!definitions) definitions = dict['en-US'];
  if (!definitions['strings']) definitions = dict['en-US'];
  return (definitions['strings'][key]) 
    ? definitions['strings'][key] 
    : (dict['en-US']['strings'][key]) 
      ? dict['en-US']['strings'][key]
      : `i18n string ${key} not found`;
};

export const LoadI18Ns = () => {
  let results: {[key:string]:any} = {};
  const files = readdirSync(path.resolve(__dirname, '../i18n'));

  for (let file of files) {
    if (!file.endsWith('.json')) continue;
    let imp = require(`../i18n/${file}`);
    results[imp['lang']] = imp;
  }
  return results;
};