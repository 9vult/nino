# Nino Documentation

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

Permit another server to "observe" the project. All aliases for the project will be shown as options in the `/blame` autocomplete list in the observer server. Additionally, webhooks can be assigned, allowing for updates and releases to be distributed in many servers. **Note: webhook messages will not include role pings.**

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| guild | yes | string | ID of the guild observing the project |
| updates | no | string<URL> | Webhook URL for project updates |
| releases | no | string<URL> | Webhook URL for project releases |

### removeobserver

Remove an observer server.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| guild | yes | string | ID of the guild to remove |

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

## Commands - Progress

### done

Mark a task as complete. Accessible by assigned user and project owner.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| episode | yes | number | Episode number |
| abbreviation | yes | string | Shorthand for the position (ex: TM) |

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

### blame

Check the status of a project or episode. Accessible by everyone.

| Parameter | Required | Type | Description |
|-----------|----------|------|-------------|
| project | yes | string | Project nickname |
| episode | no | number | Episode number |
| explain | no | boolean | Display more details about the positions |

## Commands - Other

### help

Displays some simple help.

### about

Displays some information about Nino.
