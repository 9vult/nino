import { XMLParser } from "fast-xml-parser";
import { DatabaseData } from "../misc/types";
import { t } from "i18next";

export const AirDate = async (anidb: string, airTime: string | undefined, episodeNumber: number, dbdata: DatabaseData, lng: string) => {
  if (!airTime) airTime = '00:00';

  const cid = process.env.ANIDB_API_CLIENT_NAME;
  const baseUrl = `http://api.anidb.net:9001/httpapi?client=${cid}&clientver=1&protover=1&request=anime&aid=${anidb}`;

  let response: Response = new Response();

  try {
    response = await fetch(baseUrl);
  } catch (err) {
    return t('anidbApiError', { lng });
  }

  if (!response.ok) return t('anidbApiError', { lng });
  let resptext = await response.text();
  if (!resptext) return t('anidbResponseEmpty', { lng });
  let parser = new XMLParser();
  let xml = parser.parse(resptext);
  if (Object.keys(xml)[0] == 'error') return t('anidbError', { lng });

  const episodes = xml.anime.episodes.episode;
  const desiredEpisode = episodes.find((ep: any) => ep.epno == episodeNumber);
  if (desiredEpisode) {
    let airDate = desiredEpisode.airdate;
    // Airdate is in YYYY-MM-DD form, so add timezone info
    airDate = `${airDate}T${airTime}+09:00`; // Japan
    if (airDate) {
      let date = new Date(airDate);
      let future = (date > new Date());
      const utc = Math.floor(date.getTime() / 1000);
      // return `${future ? 'Airs' : 'Aired'} on <t:${utc}:D> (<t:${utc}:R>)`;
      return future 
        ? t('airdateFuture', { lng, date: `<t:${utc}:D>`, rel: `<t:${utc}:R>` })
        : t('airdatePast', { lng, date: `<t:${utc}:D>`, rel: `<t:${utc}:R>` });
    }
  }

  let startdate = xml.anime.startdate;
  // Startdate is in YYYY-MM-DD form, so add timezone info
  startdate = `${startdate}T${airTime}+09:00`; // Japan

  if (startdate) {
    let date = new Date(startdate);
    date.setDate(date.getDate() + (7 * (episodeNumber - 1)));
    let future = (date > new Date());
    const utc = Math.floor(date.getTime() / 1000);
    return `${t('estimate', { lng })}: `
      + future 
        ? t('airdateFuture', { lng, date: `<t:${utc}:D>`, rel: `<t:${utc}:R>` })
        : t('airdatePast', { lng, date: `<t:${utc}:D>`, rel: `<t:${utc}:R>` });
  }

  return t('anidbNotSpecified', { lng });
}