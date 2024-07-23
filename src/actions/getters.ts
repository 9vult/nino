import { Episode, Project } from "../misc/types";

/**
 * Get an episode from a project
 * @param project Project to get the episode in
 * @param num Episode number to get
 * @returns Episode ID and object, or undefined
 */
export const getEpisode = (project: Project, num: number) => {
  for (let id in project.episodes) {
    const episode = project.episodes[id];
    if (episode.number == num) {
      return {
        id,
        episode
      };
    }
  }
  return {
    id: undefined,
    episode: undefined
  };
}

/**
 * Get a key staff from a project
 * @param project Project to get the staff from
 * @param abbreviation Abbreviation of the staff
 * @returns KeyStaff ID and object, or undefined
 */
export const getKeyStaff = (project: Project, abbreviation: string) => {
  for (let id in project.keyStaff) {
    const keyStaff = project.keyStaff[id];
    if (keyStaff.role.abbreviation == abbreviation) {
      return {
        id,
        keyStaff
      };
    }
  }
  return {
    id: undefined,
    keyStaff: undefined
  };
}

/**
 * Get an additional staff from an episode
 * @param episode Episode to get the staff from
 * @param abbreviation Abbreviation of the staff
 * @returns AddStaff ID and object, or undefined
 */
export const getAdditionalStaff = (episode: Episode, abbreviation: string) => {
  for (let id in episode.additionalStaff) {
    const addStaff = episode.additionalStaff[id];
    if (addStaff.role.abbreviation == abbreviation) {
      return {
        id,
        addStaff
      };
    }
  }
  return {
    id: undefined,
    addStaff: undefined
  };
}

/**
 * Get a task from an episode
 * @param episode Episode to get the task from
 * @param abbreviation Abbreviation of the task
 * @returns Task ID and object, or undefined
 */
export const getTask = (episode: Episode, abbreviation: string) => {
  for (let id in episode.tasks) {
    const task = episode.tasks[id];
    if (task.abbreviation == abbreviation) {
      return {
        id,
        task
      };
    }
  }
  return {
    id: undefined,
    task: undefined
  };
}

/**
 * Get the blameable episode
 * @param project Project to get the episode from
 * @param episodeNumber Optional episode number
 * @returns Episode ID and object, or undefined
 */
export const getBlameableEpisode = (project: Project, episodeNumber: number | null) => {
  if (episodeNumber) return getEpisode(project, episodeNumber);

  for (let id in project.episodes) {
    const episode = project.episodes[id];
    if (!episode.done) {
      return {
        id,
        episode
      };
    }
  }
  return {
    id: undefined,
    episode: undefined
  };
}

/**
 * Get a list of all privileged IDs for a project (they can see private projects)
 * @param project Project to get the privileged IDs of
 * @returns List of privileged IDs
 */
export const getAllPrivilegedIds = (project: Project, guildAdmins: string[]) => {
  const ids: string[] = [];

  // Owner
  ids.push(project.owner);

  // Project admins
  for (let admin of (project.administrators ?? [])) {
    ids.push(admin);
  }

  // Guild admins
  for (let admin of guildAdmins) {
    ids.push(admin);
  }

  // Key Staff members
  for (let id in project.keyStaff) {
    const keyStaff = project.keyStaff[id];
    ids.push(keyStaff.id);
  }

  // Additional Staff members
  for (let id in project.episodes) {
    const episode = project.episodes[id];
    for (let id in episode.additionalStaff) {
      const addStaff = episode.additionalStaff[id];
      ids.push(addStaff.id);
    }
  }
  return ids;
}
