The solution consists of the following projects:

AuthServer - ASP.NET authentication server which manages user accounts and issues tokens.
Core - Core game logic independent from Godot. Covered with unit tests.
    CampaignServer - Orchestrates shard servers and clients, handles global state.
    Gameplay - Client and shard server (shard server simulates gameplay for a single shard).
    Shared - Shared core logic.
Util - Utilities extending standard libraries.
Soteo - Main Godot entry point project for the client, the shard server and the campaign server. Contains all Godot
dependent code.

# Gameplay

The shard server runs with a fixed tick rate, all gameplay logic is executed exclusively in _PhysicsProcess.

The client sends commands to the shard server and replicates the state using snapshots sent by the server.

`Main` node is the entry point that sets up dependency injection and creates services. Scoped services are instantced
once per every loaded shard. For the server, they are equivalent to singletons, since a shard server handles only one
shard. For the client, they are created for each shard the client connects to.

Constructor dependency injection is used for all classes. When a node needs dependencies, but at the same time it needs
to exist in a scene (which wouldn't allow using constructor injection), it's split into two classes: plain C# class
and a node class. Most code resides in the plain C# class, which gets its dependencies from a constrcutror. Among these
dependencies it receives the node, which it uses as a helper to interact with Godot. Other benefits of this approach
include avoidance of manual lifetime management (no need to call Free, GC will collect entities after they are
unreferenced) and object pooling for entities.

## Entities

Any dynamic object in the game world is an entity. The `Entity` class is a base of all entities.
`Unit` and `Projectile` are server side entities containing the gameplay logic.
`UnitPuppet` and `ProjectilePuppet` are client side entities containing only the presentation logic.

### Units

Units are player characters, NPCs and buildings. Each unit has position and azimuth as well as collections of
stats, abilities and statuses.

#### Stats

Each unit has a set of stats. Volatile stats such as current health can change freely and for any reason.
Non-volatile stats such as move speed are derived from default values and statuses.

#### Abilities

Units have abilities representing different actions a unit can do: attacks, spells, item usage.
`Ability` is a base class for abilities, each specific ability is defined in a stateless singleton child class.
`AbilityContext` contains state for a specific usage of an ability. Ability class defines various properties of an
ability, as well as its effect. `TakeEffect` method defines the ability immediate effect. Deferred effects are
implemented using projectiles or statuses.

#### Statuses

Statuses are effects that can affect a unit temporarily or permanently (such as for passive abilities).
`Status` is a base class for statuses, each specific satus is defined in a stateless singleton child class.
`StatusContext` contains state for a specific instance of a status. Statuses can hook into unit events, alter stats,
tick with a fixed interval.

## Singleton services

`WebRtcFromGameplayToGameplayCommunicator` handles WebRTC communication between clients and shard servers.

`WebSocketFromGameplayToCampaignServerCommunicator` handles WebSocket communication with the campaign server.

All packets are sent through the unified `IPacketSender` interface.

Incoming packets are handled by packet handlers inheriting from `PacketHandler`. By default they only accept packets
from the server side, add `[AllowClientPackets]` to change it. Handlers recieving client packets are responsible for
validating them.

`EntityLocator` is a client utility to search for an entity across all loaded shards.

## Shard scoped services

`EntityManager` creates entities, stores an index of existing entities, notifies about entity additions and removals.

`Synchronization Server` and `Synchronization Client` synchronize entities between shard servers and clients.
