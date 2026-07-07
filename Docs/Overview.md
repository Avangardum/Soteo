The game runs in campaigns. A campaign is an interconnected cluster of servers running a game event independently from
other campaigns. On the server side campaign is run by the single campaign server which orchestrates the campaign and
multiple shard servers, each managing a single shard. The term "Gameplay" refers to shard servers and clients.

The solution consists of the following projects:

- AuthServer - ASP.NET authentication server which manages user accounts and issues tokens.
- Core - Core game logic independent from Godot.
- Util - Language level utilities extending standard libraries.
- Soteo - Main Godot entry point project for the client, the shard server and the campaign server. Contains all Godot
  dependent code.

The shard server runs with a fixed tick rate, all gameplay logic is executed exclusively in _PhysicsProcess.

The client sends commands to the shard server and replicates the state using snapshots and deltas sent by the server.

`Main` node is the entry point for the client and the shard server that sets up dependency injection and creates
services. Scoped services are instanced once per every loaded shard.
For the server, they are equivalent to singletons, since a shard server handles only one shard.
For the client, they are created for each shard the client connects to.
`CampaignServer` node is the entry point for the campaign server.

Constructor dependency injection is used for all classes. When a node needs dependencies, but at the same time it needs
to exist in a scene (which wouldn't allow using constructor injection), it's split into two classes: the plain C# class
and the node class. Most code resides in the plain C# class, which gets its dependencies from a constructor. Among
these dependencies it receives the node, which it uses as a helper to interact with Godot. Other benefits of this
approach include avoidance of manual lifetime management (no need to call Free, GC will collect objects after they are
unreferenced) and ability to use object pooling separately (such as pooling nodes for performance and avoiding pooling
plain C# objects with complex lifetime).

Most of the codebase is thread-unaware, meaning that it works under the assumption that no other thread is performing
any concurrent reads or writes on any state. Since Godot always calls scripts on the main thread and uses a
synchronization context that runs async continuations on the main thread, all code runs on the main thread by default,
unless a thread is created explicitly. If any extra threads are created explicitly, they must honor the aforementioned
assumption by not reading or writing any state.

# DTO

DTO (data transfer objects) are immutable groupings of data to be exchanged between functions.

Snapshots are DTO that capture some state at some point of time. Packets are DTO that are serialized and transferred
over network. Snapshots and packets should follow extra rules:

- They should not reference anything mutable either directly or transitively to remain unchanged even after
  the state they captured changes.
- They should not reference objects that have an id, instead storing just the id 
  to avoid duplication in serialized data.

# Models

Models are domain objects that are not positioned at a specific position of a specific shard, such as
shards themselves and players (unlike player **characters** that have such a position and are entities).

# Entities

Entities are domain objects that have a concrete position in the game world.
The `Entity` class is the base of all entities.
`Unit` and `Projectile` are server side entities containing gameplay logic.
`UnitPuppet` and `ProjectilePuppet` are client side entities containing only presentation logic.

## Units

Units are player characters, NPCs and buildings. Each unit has position and azimuth as well as collections of
stats, abilities and statuses.

### Stats

Each unit has a dictionary of stats containing a numeric value for every existing `Stat`.
Resource stats such as current health can be changed externally for any reason.
Computed stats such as move speed are read-only, derived from default values and statuses.

### Abilities

Units have abilities representing different actions a unit can do: attacks, spells, item usage, as well as passive
effects. `Ability` is the base class for abilities, each specific ability is defined in a stateless singleton derived
class. `AbilityContext` contains state for a specific usage of an ability. Ability class defines various properties of
an ability, as well as its effect. `TakeEffect` method defines the ability's immediate effect. Deferred effects are
implemented using projectiles or statuses.

### Statuses

Statuses are effects that can affect a unit temporarily or permanently (such as for passive abilities).
`Status` is the base class for statuses, each specific status is defined in a stateless singleton derived class.
`StatusContext` contains state for a specific instance of a status. Statuses can hook into unit events, alter stats,
tick with a fixed interval.

# Services

Services are objects that do not correspond to any specific "thing" in the game, they either perform some work
in the background, provide methods to be used by other objects or both of these.

## Communication

Services suffixed `Communicator` manage communications between servers and clients by sending and receiving packets.
They are not used directly, instead `IFromGameplayPacketSender` / `IFromCampaignServerPacketSender` interfaces are used
for sending packets, while handling received packets is done by packet handlers inheriting from `PacketHandler` that are
called automatically by communicators when a matching packet is received. By default they only accept packets
from the server side, add `[AllowClientPackets]` to change it. Handlers allowing client packets are responsible for
validating them.

## Gameplay singleton services

`EntityLocator` is a client utility to search for an entity across all loaded shards.

## Gameplay shard scoped services

`EntityManager` creates entities, stores the index of existing entities, notifies about entity additions and removals.

`SynchronizationServer` and `SynchronizationClient` synchronize shard state between shard servers and clients.
