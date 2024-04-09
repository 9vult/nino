import { XMLParser } from "fast-xml-parser";

export const AirDate = async (anidb: string, airTime: string | undefined, episodeNumber: number) => {
  if (!airTime) airTime = '00:00';

  const cid = process.env.ANIDB_API_CLIENT_NAME;
  const baseUrl = `http://api.anidb.net:9001/httpapi?client=${cid}&clientver=1&protover=1&request=anime&aid=${anidb}`;

  const response = await fetch(baseUrl);
  if (!response.ok) return 'Air date unknown: AniDB API error';
  let reqtext = await response.text();
  if (!reqtext) return 'Air date unknown: AniDB request empty';
  let parser = new XMLParser();
  let xml = parser.parse(reqtext);
  if (Object.keys(xml)[0] == 'error') return 'Air date unknown: AniDB error';

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
      return `${future ? 'Airs' : 'Aired'} on <t:${utc}:D> (<t:${utc}:R>)`;
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
    return `Estimate: ${future ? 'Airs' : 'Aired'} on <t:${utc}:D> (<t:${utc}:R>)`;
  }

  return 'Air date unknown: Not specified on AniDB';
}