# Shards of the Empire Online

Shard of the Empire Online (Soteo) is a game that fits between classical MMOs and match-based games.
A campaign that lasts for hours is played over many short sessions so that any dedicated player can play from the
beginning to the end.

The project aims to provide MMO level scale while keeping the gameplay engaging and concentrated. It aims to avoid
the grind, long downtime and filler content which are common in the MMO genre.

Two factions: Empire and Syndicate fight in a for for shards of the shattered world.
Players pick a side and work alongside their allies to destroy the enemy.

In Soteo there is no fog of war, every player or spectator can observe any point of the world in real time.

A secondary game mode called challenges lets you try yourself in series of trials at your own pace either alone or
in a party.

The game is currently in early development.

## Architecture

Campaign is run by a cluster of servers consisting of one campaign server, which coordinates the cluster and handles
global state, and several shard servers, each representing a shard of the game world.

Singleplayer mode works by running the required servers locally alongside the client.

AuthServer contain an ASP.NET project handling user accounts and issuing access tokens.

Godot folder contains a Godot project containing campaign server, shard server and client components.

See Architecture.md files in subfolders to learn about specific components.

## To-do

- [x] Rework azimuth indicator
- [x] Fix pixel perfect rendering
- [x] Show statuses in HUD
- [ ] Stop command
- [x] Switch to double
- [ ] Status / abililty icons
- [ ] Death
- [ ] UI scaling
- [ ] Status / ability tooltips
- [ ] Tell apart factions at low health
- [ ] Cluster startup / shutdown
- [ ] Stress test / optimization
- [ ] Manual character creation and spawning
- [ ] Extra data dictionary
- [ ] Shard limits
- [ ] Travel between shards
- [ ] Navigation
- [ ] Items
- [ ] Singleplayer scenarios
- [ ] Email sending