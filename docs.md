# Nino Documentation

## Getting Started

### Creating a project

Despite there being a single "[/newproject](#newproject)" command, it actually takes
a few different commands to get a project fully set up. Here's an outline of
the steps to take to bootstrap a project:

1. Run [/newproject](#newproject). The newproject command will set all the basic information,
  most relating to the display in progress updates.
2. [/addstaff](#addstaff) to the project. These Key Staff are the tasks that will be done on every episode.
  You can set placeholder members and [/swapstaff](#swapstaff) later on.
3. If there are any tasks that will only be on a single episode, such as song styling
  on episode 1, use [/addadditionalstaff](#addadditionalstaff).
4. Use [/editproject](#editproject) to set the AniDB ID and Air Time if you would like
  episode air times to be displayed in [/blame](#blame) for untouched episodes.
5. Add any aliases you might want with [/addalias](#addalias) to make commands more accessible.
6. You're all set!

- **Rec**: I recommend setting the Name of staff positions to `-ing` verbs, as they will look
  the best in progress updates and [/blame with explain flag](#blame).
- **Note**: By default, Key Staff will be displayed in creation order. Use [/setweight](#setweight) to change the order.
- **Note**: Additional Staff will always be displayed after Key Staff. This cannot be changed.
- **Info**: If you are coming from Deschtimes and have been using the Joint feature to relay
  progress updates and [/blame](#blame) functionality to another server, see the section
  for [adding an observer](#addobserver).

## Commands - Project management

### newproject

Set up a new project. This command is only accessible by server administrators.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| nickname | yes | string | Short nickname for referring to the project in other commands |
| title | yes | string | Full name of the series (used in embeds) |
| type | yes | choose | Type of project. Choose between TV, Movie, and BD |
| length | yes | number | Number of episodes. Generally, 1 for movies |
| poster | yes | string<URL> | Poster image URL (used in embeds) |
| private | yes | boolean | Is this project private? ([See below note](#a-note-on-private-projects-and-privileged-users)) |
| updatechannel | yes | channel | Channel to post progress updates in |
| releasechannel | yes | channel | Channel to post releases in |

### addstaff

Add a staff position to the project, applying to all episodes.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| member | yes | user | Member being assigned the position |
| abbreviation | yes | string | Shorthand for the position (ex: TM) |
| title | yes | string | Full name of the position (ex: Timing) |

### removestaff

Remove a staff position from the project, applying to all episodes.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| abbreviation | yes | string | Shorthand for the position (ex: TM) |

### swapstaff

Swap someone else in for a position, applying to all episodes.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| abbreviation | yes | string | Shorthand for the position (ex: TM) |
| member | yes | user | Member being assigned the position |

### addadditionalstaff

Add a staff position for a single episode.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| episode | yes | number | Episode to add the task to |
| member | yes | user | Member being assigned the position |
| abbreviation | yes | string | Shorthand for the position (ex: KFX) |
| title | yes | string | Full name of the position (ex: Song Styling) |

### removeadditionalstaff

Remove an additional-staff position.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| episode | yes | number | Episode to add the task to |
| abbreviation | yes | string | Shorthand for the position (ex: KFX) |

### swapadditionalstaff

Swap someone else in for an additional-staff position.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| episode | yes | number | Episode to add the task to |
| abbreviation | yes | string | Shorthand for the position (ex: KFX) |
| member | yes | user | Member being assigned the position |

### setweight

Set the weight of a Key Staff position to change the display order. By default, positions are weighted in `n+1` fashion, starting with `0`, eg `0, 1, 2, 3...`. Negative numbers and decimals are accepted.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| abbreviation | yes | string | Shorthand for the position (ex: TM) |
| weight | yes | number | Weight of the position |

### addepisode

Add another episode to the project.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| number | yes | number | Episode number |

### removeepisode

Remove an episode from the project. Scope: project owner

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| number | yes | number | Episode number |

### addalias

Add an alias for the project. Will display in autocomplete pop-ups, and can be used anywhere `project` is a parameter.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| alias | yes | string | Project alias |

### removealias

Remove an alias from the project.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| alias | yes | string | Project alias |

### addobserver

Observe a project from another server. Server administrators can use this command. **Note: webhook messages will not include role pings.**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| guild | yes | string | ID of the guild you want to observe |
| project | yes | string | Nickname of the project you want to observe |
| blame | yes | boolean | Whether to populate `/blame` with this project's aliases |
| updates | no | string<URL> | Webhook URL for project updates |
| releases | no | string<URL> | Webhook URL for project releases |

### removeobserver

Remove an observed project. Server administrators can use this command.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| guild | yes | string | ID of the guild to remove |
| project | yes | string | Project nickname |

### addadmin

Add a user as an administrator for the project. Project administrators have access to most of the commands traditionally relegated to the owner, except:

- [/addadmin](#addadmin)
- [/addobserver](#addobserver)
- [/deleteproject](#deleteproject)
- [/transferownership](#transferownership)
- [/removeadmin](#removeadmin)
- [/removeobserver](#removeobserver)

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| member | yes | user | User to add as an admin |

### removeadmin

Remove an administrator from the project.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| member | yes | user | User to remove |

### transferownership

Transfer ownership of the project to someone else.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| member | yes | user | New project owner |

### deleteproject

Delete a project.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |

### editproject

Edit project values.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| option | yes | choose | Choose an option |
| newvalue | yes | any | New value of the option |

Options:

| Option | Input type | Description |
|--------|------------|-------------|
| Title | string | The show title |
| Poster | string<URL> | Poster URL |
| AniDB | string | AniDB ID. Used for air dates in untouched episodes' `/blame` embeds |
| AirTime24h | time | 24-hour air time (Japan time) of episodes (ex: `16:30`) |
| UpdateChannelID | string | ID of channel to post updates in |
| ReleaseChannelID | string | ID of channel to post releases in |
| IsPrivate | string<boolean> | Is this project private? ([See below note](#a-note-on-private-projects-and-privileged-users)) |

### configuration

Subcommands for server configuration

#### progress_display

Options:

| Option | Description |
|--------|-------------|
| Normal | Progress display will use abbreviations |
| Extended | Progress display will mimic [/done with the `explain` toggle enabled](#done)  |

#### done_display

Options:

| Option | Description |
|--------|-------------|
| Succinct | Default replies to progress commands |
| Verbose | Include status line |

## Commands - Progress

### done

Mark a task as complete. Accessible by assigned user and project owner.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| abbreviation | yes | string | Shorthand for the position (ex: TM) |
| episode | no | number | Episode number |

If an episode is not specified, Nino will first try to mark the task in the "current working episode"
(the first not-done episode) as done. If the task there is complete, the first subsequent episode where
the task is incomplete will be found, and the user will be asked if they want to mark that episode or do nothing.

- The look-ahead feature does not work on Additional Staff
- Role autocomplete suggestions for Additional Staff will be accurate only to the working episode, even if another
episode is then specified, due to a Discord limitation. This does not affect manual entry of the role.

### undone

Mark a task as incomplete. Accessible by assigned user and project owner.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| episode | yes | number | Episode number |
| abbreviation | yes | string | Shorthand for the position (ex: TM) |

### skip

Skip a task. Accessible by assigned user and project owner.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| episode | yes | number | Episode number |
| abbreviation | yes | string | Shorthand for the position (ex: TM) |

### release

Release! Accessible by project owner.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| type | yes | choose | Choose the type of release: Episode, Volume, Batch |
| number | yes | string | Number or range being released (ex: `7`, `1-12`, `5v0`) |
| url | yes | string<URL> | Url linking to the release |
| role | no | role | Role to be pinged (Does not affect observer servers) |

### bulk

Action a task in bulk. Accessible by assigned user and project owner.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| action | choose | string | Action to perform |
| abbreviation | yes | string | Shorthand for the position (ex: TM) |
| start_episode | yes | number | Starting episode |
| end_episode | yes | number | Ending episode (inclusive) |

### blame

Check the status of a project or episode. Accessible by everyone.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| episode | no | number | Episode number |
| explain | no | boolean | Display more details about the positions |

### roster

Check who is assigned to what for an episode. Accessible by Key Staff.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| episode | yes | number | Episode number |

### conga

#### add

Add a link to the project's Conga line. When the Current task is completed, the Next task will be pinged.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| abbreviation | yes | string | Current task abbreviation |
| next | yes | string | Next task abbreviation |

#### remove

Remove a link from the project's Conga line.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| abbreviation | yes | string | Current task abbreviation |

## Commands - Other

### help

Displays some simple help.

### about

Displays some information about Nino.

## A Note on Private Projects and Privileged Users

For the case of Private Projects, a privileged user is any of the following:
 - Project owner
 - Project-scope administrators
 - Guild-scope administrators
 - Key Staff
 - Additional Staff

| Item                    | Public              | Privileged         | Non-Privileged     |
|-------------------------|---------------------|--------------------|--------------------|
| Show in Autocomplete    | :heavy_check_mark:  | :heavy_check_mark: | :x:                |
| `/blame`                | :heavy_check_mark:  | :heavy_check_mark: | :x:                |
| See Public Updates\*    | :heavy_check_mark:  | :heavy_check_mark: | :heavy_check_mark: |
| See Public Releases\*   | :heavy_check_mark:  | :heavy_check_mark: | :heavy_check_mark: |
| Add New Observer        | :heavy_check_mark:  | :heavy_check_mark: | :x:                |
| Observer Autocomplete   | :heavy_check_mark:  | :heavy_check_mark: | :x:                |
| Observer `/blame`       | :heavy_check_mark:  | :heavy_check_mark: | :x:                |
| Observer Updates        | :heavy_check_mark:  | :heavy_check_mark: | :heavy_check_mark: |
| Observer Releases       | :heavy_check_mark:  | :heavy_check_mark: | :heavy_check_mark: |

\* "Public Updates" and "Public Releases" refer to a publicly accessible `updatechannel` or `releasechannel`.
