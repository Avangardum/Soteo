# Gameplay

The gameplay section of the Godot project represents both the shard server and the client.

The shard server runs with a fixed tick rate, all gameplay logic is executed exclusively in _PhysicsProcess.

The client sends commands to the shard server and replicates the state using snapshots sent by the server.

`Main` node is the entry point that sets up dependency injection and creates services. Scoped services are instantiated
once per every loaded shard. For the server, they are equivalent to singletons, since a shard server handles only one
shard. For the client, they are created for each shard the client connects to.

Constructor dependency injection is used for all classes, even for nodes. Since nodes without a parameterless
constructor can't be instanced by Godot, they must created from the code. Dependency-injected nodes that have children
nodes as part of their inherent structure use scenes to store this structure, which are instanced and reparented to
the node in its constructor, such scenes have a `Proxy.gd` node as a placeholder parent, which is freed immediately
afer instancing, so any properties of it are lost, any initialization of the parent node should happen in the
constructor.

## Entities

Any dynamic object in the game world is an entity. The `Entity` class is a base of all entities. `Unit` and `Projectile`
subclasses define the correspoinding entity types. Entities are plain C# classes, not inherited from `Node`, instead
they create nodes of `UnitNode` / `ProjectileNode` type and use them. This way entities are managed by the garbage
collector and remain valid even after their node is freed. Entities can create and replicate `EntitySnapshot`s, which
are used for network synchronization and persistence.

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

`EntityManager` creates entities, stores an index of existing entities,
notifies about entity additions and removals.

`Synchronization Server` and `Synchronization Client` synchronize entities between the shard server and the client.