import { readdirSync } from "fs"
import i18next from "i18next";
import Backend, { FsBackendOptions } from "i18next-fs-backend";
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

export const LoadCmdI18Ns = () => {
  let results: {[key:string]:any} = {};
  const files = readdirSync(path.resolve(__dirname, '../i18n/cmd'));

  for (let file of files) {
    if (!file.endsWith('.json')) continue;
    let imp = require(`../i18n/cmd/${file}`);
    results[imp['lang']] = imp;
  }
  return results;
};

export const InitI18Next = () => {
  const langs = readdirSync(path.resolve(__dirname, '../i18n/str'))
    .filter(f => f.endsWith('.json'))
    .map(f => f.replace('.json', ''));

  i18next
    .use(Backend)
    .init<FsBackendOptions>({
      backend: {
        loadPath: path.join(__dirname, '../i18n/str/{{lng}}.json')
      },
      lng: 'en-US',
      fallbackLng: 'en-US',
      ns: 'common',
      defaultNS: 'common',
      preload: langs,
      debug: false
    });
}