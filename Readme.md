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

[Discord](https://discord.gg/kJzPpwdwrc)

## Getting started

### Building and running from source

### Browser singleplayer

The simplest way to run the Soteo from source is browser singleplayer.
Install .NET and Godot using links form the next section. Open the Godot project and press the HTML5
icon in the top right, in the opened browser window click singleplayer and log in with default credentials.

### Multiplayer

#### Environment setup

[Install .NET 10 SDK](https://dotnet.microsoft.com/en-us/download)

[Install Godot 3.6.2](https://godotengine.org/download/archive/3.6.2-stable/) (pick the .NET version, not standard).
Create a symlink to the Godot executable called `soteo` (`soteo.exe` on Windows) and update the PATH environment
variable to include the directory containing the symlink. Open a new terminal and type `soteo`. If everything is
correct, the Godot project selection menu will open.

[Install latest PostgreSQL](https://www.postgresql.org/download/) and create an empty database called `SoteoAuthServer`

TODO Devcert

Set the following environment variables:

`Soteo__AuthServerConnectionString` - PostgreSQL connection string in the following format (adjust if necessary):
`Server=127.0.0.1;Port=5432;Database=SoteoAuthServer;User Id=postgres;Password=postgres;`

`Soteo__AuthServerUrl` - Auth server url, `localhost:3705` if using the default port

`Soteo__MasterServerUrl` - Campaign server url, `localhost:3706` if using the default port TODO rename

`Soteo__CampaignSnapshotPath` - TODO

`Soteo__IntercomSecret` - Base64 encoded secret that servers use for internal authentication, use any random
base64 string

`Soteo__PrivateKeyPath` - TODO

Go to the auth server url in a browser and register a new account with email `player1@soteo.net`
and password `Pa55_word`

#### Running from a terminal

Open a terminal and navigate to `/Godot`, all commands should run from here

Before running build the solution with `dontet build` or with a `Build` button in Godot

Start the auth server, the campaign server, 2 shard servers and 2 clients by running the following commands,
each in a separate terminal:

```bash
dotnet run --project ../Soteo.AuthServer
soteo --quiet --no-window --campaign-server --shard 00000000-0000-0000-0000-0000000005d1 --shard 00000000-0000-0000-0000-0000000005d2
soteo --quiet --no-window --shard-server 00000000-0000-0000-0000-0000000005d1
soteo --quiet --no-window --shard-server 00000000-0000-0000-0000-0000000005d2
soteo --quiet --no-scroll --position 10,10 --resolution 1000x500
soteo --quiet --no-scroll --position 10,550 --resolution 1000x500 --email player2@soteo.net
```

Log in with default credentials.

#### Running from Rider

If using Rider, you can user Run/Debug configurations to run the game instead of terminal. For the auth server add
".NET Launch Settings Profile", for other processes add "Godot 3 Start and Debug" (set working directory to `/Godot`,
copy arguments from above and use "Redirect standard streams" terminal mode). To launch all processes in one click add
"Multi-Launch" with first step building the solution immediately, second step launching auth server after previous
finished and the rest after previous started.

### Exploring the codebase

Start by reading the contents of the Docs folder to get a top level overview on the project. After that you can
explore the classes mentioned in the docs or the ones you are interested in.

## Contributing

If you'd like to contribute, join the Discord server and let me know

## To-do

- [x] Rework azimuth indicator
- [x] Fix pixel perfect rendering
- [x] Show statuses in HUD
- [x] Stop command
- [x] Switch to double
- [x] Status / abililty icons
- [x] Status / ability tooltips
- [x] Death
- [x] Style convention
- [x] Testing
- [x] Cluster startup / shutdown
- [ ] Stress test / optimization
- [ ] Staging polygon
- [x] Manual character creation and spawning
- [ ] Extra data dictionary
- [ ] Shard capacity limits
- [ ] Travel between shards
- [ ] Navigation
- [ ] Items
- [ ] Singleplayer scenarios
- [ ] Email sending

## License

Shards of the Empire Online (Soteo)
Copyright (C) Soteo contributors

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
