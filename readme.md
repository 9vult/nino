# Nino

### Nino is a discord bot for tracking and managing episode-by-episode tasks for fansub groups.

## Commands
### add
 - Add a new title
 - Syntax: `.nino add FullName | Nickname | EpisodeCount | Task1 Task2 ...`
 - Example: `.nino add Sword Art Online | SAO | 25 | TL ED TM TS QC`

### done
 - Mark a task as done
 - Syntax: `.nino done Nickname EpisodeNumber Task`
 - Example: `.nino done SAO 3 TL`
 - _Note: This command pushes an embed to #progress_

### complete
 - Mark an episode as complete
 - Syntax: `.nino complete Nickname EpisodeNumber`
 - Example: `.nino complete SAO 3`
 - _Note: This command pushes an embed to #progress_

### about
 - Sends an about box
 - Syntax: `.nino about`

### help
 - Sends a help box
 - Syntax: `.nino help`
 - _Note: More help coming soon_

## Server Requirements
 - A `#progress` channel for Nino to write update embeds to
 - An `@Quintuplet` user role for administrative users

## Setup Requirements
 - Dependencies: `discord.js` and `moment.js`
 - Requires a node.js release of at least v12.0
 - Discord bot auth token should go in `nino/token.txt`

## Other Notes
 - Data is written to `nino/data/`
