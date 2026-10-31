# Shards of the Empire Online

Shard of the Empire Online (Soteo) is a game that fits between classical MMOs and match-based games.
A campaign that lasts for hours is played over many short sessions, so that any dedicated player can play from the
beginning to the end.

The project aims to provide an MMO level scale while keeping gameplay engaging and concentrated. It aims to avoid
grind, long downtime and filler content which are common in the MMO genre.

Two factions: Empire and Syndicate fight for shards of a shattered world.
Players pick a side and work alongside their allies to destroy the enemy.

In Soteo there is no fog of war, every player or spectator can observe any point of the world in real time.

A secondary game mode called challenges lets you try yourself in a series of trials at your own pace either alone or
in a party.

See Vision.md for a detailed description of Soteo ideology, values and design goals.

The game is currently in early development, working towards its first v0.1 demo release containing a
singleplayer challenge showcasing core mechanics.

[Discord](https://discord.gg/kJzPpwdwrc)

## Getting started

### Building and running from source

[Install Rider](https://www.jetbrains.com/rider/)

[Install .NET 10 SDK](https://dotnet.microsoft.com/en-us/download)

[Install Godot 4.7.2](https://godotengine.org/download/archive/4.7.2-stable/)
(pick the .NET version, not the standard one)

[Install latest PostgreSQL](https://www.postgresql.org/download/)

Export the .NET development certificate into any directory outside of the repository by running the following command
with `<PATH>` replaced by a path where the certificate file will be created, including the .crt extension. This will
produce a pair of .crt and .key files.

```
dotnet dev-certs https --export-path <PATH> --no-password --format PEM
```

Trust the certificate.

```
dotnet dev-certs https --trust
```

Set the following environment variables:

`Soteo__AuthServerConnectionString` - PostgreSQL connection string in the following format:
`Server=127.0.0.1;Port=5432;Database=SoteoAuthServer;User Id=postgres;Password=postgres;`

`Soteo__SnapshotFolder` - Path to any folder outside the repository where snapshot files will be stored

`Soteo__CertificatePath` - Path to the .crt file

`Soteo__PrivateKeyPath` - Path to the .key file

`Soteo__Environment` - `Development`

Restart your IDE / Godot editor / terminal for the environment variable changes to be applied.

In Rider click the 3 dots in the top right and click `Edit...`. In the top left of the opened window click `+` and
add a `.NET Project` configuration named `AuthServer`. Set the project to `Soteo.AuthServer` and environment variables
to `ASPNETCORE_ENVIRONMENT=Development`.

Add a `.NET Executable` configuration named `CampaignServer`, set the exe path to the Godot executable, and the working
directory to the `Godot` folder inside the repository.

Copy `CampaignServer` 4 times using the copy button and name new configurations `ShardServer1`, `ShardServer2`,
`Client1`, `Client2`. Set the following program arguments for each configuration:

```
--no-header --headless Side=CampaignServer ShardIds:0=00000000-0000-0000-0000-0000000005d1 ShardIds:1=00000000-0000-0000-0000-0000000005d2
--no-header --headless Side=ShardServer ShardId=00000000-0000-0000-0000-0000000005d1
--no-header --headless Side=ShardServer ShardId=00000000-0000-0000-0000-0000000005d2
--no-header --position 10,50 --resolution 1400x700 NoScroll=true
--no-header --position 10,800 --resolution 1400x700 NoScroll=true Email=player2@soteo.net
```

`NoScroll=true` disables camera scrolling by holding the mouse at the edge of the
screen, because this can be annoying when switching between several clients. You can drag the camera by holding the
mouse wheel or you can remove this if you wish.

Create a `Multi-Launch` configuration named `All` and add all the previous configurations to it. Run it.

Go to https://localhost:3705 in a browser, register a new account with email `player1@soteo.net` and password
`Pa55_word`, then click the link to confirm the email. Repeat for `player2@soteo.net`.

Log in with a Godot client with default credentials, choose a shard and a character and click `Deploy`.

For more info on configuration, see the `Config` class.

### Exploring the codebase

Start by reading the contents of the Docs folder to get a top level overview on the project. After that you can
explore the classes mentioned in the docs or the ones you are interested in.

## Troubleshooting

### First step - delete snapshots

During development snapshot format can change, causing crashes or undefined behavior when trying to replicate an
old snapshot. So the first step if something breaks suddently should be clearing the snapshot folder
(specified in environment variables).

### Problems with running the game from a terminal

Windows releases of Godot include 2 executables, one with the _console suffix and one without. Make sure you use
the console one. Also Godot doesn't work well with terminal IO redirection, so don't use tools that do that,
such as Git Bash, instead use a native terminal app (cmd or PowerShell on Windows).

## Contributing

If you'd like to contribute, join the Discord server and let me know.

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
