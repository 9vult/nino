import appRootPath from "app-root-path";
import { existsSync, mkdirSync, readFileSync, statSync, writeFileSync } from "fs";
import { t } from "i18next";
import path from "path";

const CACHE = path.join(appRootPath.path, ".cache");
const MILLIS_PER_DAY = 86400000;

export const AniDBCache = async (anidbId: string, lng: string) => {
  const cid = process.env.ANIDB_API_CLIENT_NAME;
  const baseUrl = `http://api.anidb.net:9001/httpapi?client=${cid}&clientver=1&protover=1&request=anime&aid=${anidbId}`;

  const filename = `${anidbId}.xml`;
  const filenamefull = path.join(CACHE, filename);

  // Set up cache dir
  try {
    if (!existsSync(CACHE)) {
      mkdirSync(CACHE);
    }
  } catch (err) {
    console.error(`Failed to read/create directory ${CACHE}`);
  }

  // Check existance & age
  try {
    if (existsSync(filenamefull)) {
      let mtime = statSync(filenamefull).mtime;
      if (Date.now() - mtime.getTime() < MILLIS_PER_DAY) {
        // Use cached version
        return readFileSync(filenamefull, {encoding: 'utf-8'});
      }
    }
  } catch (err) {
    console.error(`Failed to read ${filenamefull}`);
  }

  // Acquire info
  let response: Response = new Response();

  try {
    response = await fetch(baseUrl);
  } catch (err) {
    return t('anidbApiError', { lng });
  }

  if (!response.ok) return t('anidbApiError', { lng });
  let resptext = await response.text();
  if (!resptext) return t('anidbResponseEmpty', { lng });

  // Write
  try {
    writeFileSync(filenamefull, resptext);
  } catch (err) {
    console.error(`Failed to write ${filenamefull}`);
  }

  return resptext;
};
