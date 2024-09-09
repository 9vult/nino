# Nino Documentation

## Getting Started

### Creating a Project

Despite there being a single `/project create` command, it actually takes a few different commands to get a project fully set up. Here's an outline of the steps to take to bootstrap a new project:

0. Ensure Nino has permission to view, send messages and embeds, and mention everyone in your Progress, Releases, and project channels.
1. `/project create`. This command will initialize the project. I recommend picking something short for the project nickname.
2. `/keystaff add`. Key Staff are tasks that need to be done for every episode. You can set placeholder members and use `/keystaff swap` to swap in someone else later.
    - If you need to re-arrange your Key Staff, you can use `/keystaff setweight`. By default, each Key Staff's weight is equal to their position in the line.
3. Use `/additionalstaff add` to add any one-off tasks, like Song Styling.

Some additional setup options:

- If you want subsequent Key Staff members to be notified when the preceeding task is completed, use `/project conga add`.
- If you would like air times to be displayed in /blame embeds, use `/project edit` to set the `AniDB ID` and `Air Time 24h` properties.
    - If you would like to be notified when an episode airs, use `/project airreminder enable`.
- You might want to use `/project alias add` to add aliases. These aliases can be used as alternatives to the project's nickname for most commands.

> **Recomendation**: Set task names to `-ing` verbs, as they look the best in status embeds.

> **Info**: If you are coming from Deschtimes and have been using its Joint feature to relay progress updates and use /blame in another server, see the section for [Adding an Observer](#observer-add).

### Server Options

Once you've created a project in your server, you have the option to set some server settings. For more details, see the section on Server Commands.

## Commands

### about

This simple command displays some basic information about the version of Nino currently running.

### additionalstaff

#### additionalstaff add

Add a one-off task to a single episode.

#### additionalstaff remove

Remove a one-off task from a single episode.

#### additionalstaff swap

Swap someone else in for an additionalstaff task.

#### additionalstaff setweight

Set the weight of an Additional Staff position. The weight determines the order staff appear in progress and blame embeds. By default, Additional Staff have a weight of `1000000`. This does mean that [setting a Key Staff weight](#keystaff-setweight) to greater than `1000000` will place it after (default weight) Additional Staff positions for all episodes.

### blame

Check on the status of a project. May include observed projects if the `blame` flag was set during observer creation. Defaults to the current "working episode", but any episode can be requested using the `episode` option.

Additionally, an explanitory version of the embed can be requested using the `explain` option.

### blameall

Similar to [/blame](#blame), but it displays an entire cour's worth at a time. The page length is determined via `Episodes.Count() % 13 == 0 ? 13 : 12`, or, 12 episodes, unless the episode count is divisible by 13.

Use the `page` option to request pages other than page 1.

### bulk

A shortcut command for running [/done](#done), [/undone](#undone), or [/skip](#skip) on multiple sequential episodes. Note that the `start` and `end` episodes are inclusive.

### done

Mark a task complete. The episode number is optional if:

- you are working on the same episode as everyone else
- you are working ahead in a sequential manner

### episode

#### episode add

Add an episode to the project. Decimals are permitted.

#### episode remove

Remove an episode from the project.

### Help

Provides some basic tips.

### keystaff

#### keystaff add

Add a task to every episode in the project.

#### keystaff remove

Remove a task from every episode in the project.

#### keystaff setweight

Set the weight of a Key Staff position. The weight determines the order staff appear in progress and blame embeds. By default, each staff's weight is its index, ie the first staff added's weight is 1.

#### keystaff swap

Swap someone else into a Key Staff position.

### observer

#### observer add

Observe a project on another server. Provides options for displaying that project's aliases in your server's [/blame](#blame) autocomplete and for subscribing to updates and releases with webhooks.

#### observer remove

Stop observing a project on another server.

### project

#### project admin add

Add someone as a project admin. Project admins have permission to add/remove tasks, mark tasks complete, release, etc.

#### project admin remove

Remove a project admin.

#### project airreminder enable

Receive a notification in the project channel when an episode airs. Requires the series' [AniDB ID to be set](#project-edit).

#### project airreminder disable

Turn off air reminders.

#### project alias add

Add an alias to a project. Aliases can be used in place of the project nickname for most commands.

#### project alias remove

Remove an alias. Note that the proejct nickname cannot be removed.

#### project conga add

Add a link to the project's Conga line. Members of the Conga line will be pinged when the preceeding task is complete.

For example, if TL →  ED is in the conga line, ED will be pinged when TL is complete.

#### project conga list

List all the links in the project's Conga line.

#### project conga remove

Remvove a link from the Conga line.

#### project create

Scaffold a new project. See [Getting Started](#getting-started) for tips.

Note that only users with discord server administrator permissions can create projects. You can [transfer](#project-transferownership) the project to someone if this poses an issue.

#### project delete

Delete a project. Will also remove all observers observing the project.

#### project edit

Edit the project. This is also where you can change privacy settings and add the series' AniDB ID and air time.

The MOTD option can be reset by setting it to a hyphen (`-`).

#### project transferownership

Transfer ownership of the project to someone else. Useful if the project owner isn't a server administrator and can't create projects themselves.

#### project transferserver

Transfer a project from another server. You must be the owner of the project and an administrator in the server.

### release

Notify your server of a release. The Release command allows you to streamline your release notifications will automatically broadcast the release message if the channel is an announcements channel, and forward the notification to any observers.

### roster

See who's assigned to each task in an episode.

### server

#### server admin add

Add someone as a server admin. Being a server admin gives the same permissions as project admins, but for all projects at once.

> **Note**: This is _not_ the same thing as the discord server administrator permission!!

#### server admin remove

Remove a server admin.

#### server display progress

Control how Nino's replies to [/done](#done), etc look. For example, add a blame status string.

#### server display updates

Control how updates in the Progress channel look. For example, set it to use the [explanitory blame](#blame) format instead of the short format.

#### server releaseprefix

Add a prefix to releases. The release prefix can be reset by setting it to a hyphen (`-`).

### skip

Skip a task. Considers the task complete.

### Undone

Mark a task as not done.
