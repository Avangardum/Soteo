# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Shards of the Empire Online (Soteo)** is an online multiplayer game built with Godot 3.6.2 (C#) and an ASP.NET Core
auth service. It's in early development.

## Structure

- `Godot/` — Godot project containing shard server, campaign server, and client code (all in one binary, different
  modes via launch flags)
- `AuthServer/` — ASP.NET Core 10 / EF Core service for user accounts and JWT issuance (PostgreSQL)

There are no automated tests in this repository.

## Architecture

### Server cluster

A live campaign runs as a cluster:
- **Campaign server** (`Godot/Scripts/CampaignServer/`) — coordinates the cluster and manages global state (users,
  characters). Communicates with clients and shard servers over WebSocket.
- **Shard servers** (`Godot/Scripts/Gameplay/`) — each represents a section of the game world. Communicate with
  clients over WebRTC.
- **AuthServer** — separate ASP.NET process; issues JWT access tokens used to authenticate WebSocket/WebRTC
  connections.

Singleplayer mode runs campaign server + shard server in-process with the client using **JSMQ** (an in-browser
message queue) instead of real networking.

### Godot project namespaces

- `Soteo.Gameplay` — shared client + shard server code (entities, abilities, statuses, synchronization, UI)
- `Soteo.CampaignServer` — campaign server code
- `Soteo.Shared` — packets, serializers, math utilities, extensions

### Dependency injection

All classes, including Godot nodes, use **constructor DI** via Autofac (registered through `IServiceCollection` in
`Main.cs`). Godot nodes that require DI cannot be instanced by the engine — they are created manually through their
constructor.

**Service scopes** map to shards. Scoped services are created once per shard; the server hosts one shard so scoped ≈
singleton there. The client creates a new scope per shard it connects to.

`ServerDependency<T>` / `ClientDependency<T>` are wrappers that resolve `null` when the dependency doesn't apply to
the current runtime mode (e.g., camera doesn't exist on the server).

### Scenes and Proxy.gd

Dependency-injected nodes that have a child scene structure store that structure in a `.tscn` file with a `Proxy.gd`
placeholder as the root. The node's constructor instances the scene and reparents the children to itself, then frees
the proxy. Any state on the proxy root is lost — initialization must happen in the constructor.

### Entities

Dynamic world objects are plain C# classes (`Entity<TNode>`) — not Godot nodes — managed by the GC. Each entity owns
a `UnitNode` or `ProjectileNode` (Godot `Node2D` subclasses) for rendering/physics.

- `Unit` — player characters, NPCs, buildings. Has stats, abilities, and statuses.
- `Projectile` / `TargetedProjectile` — attack effects.

Entities produce `EntitySnapshot` objects used for network synchronization and persistence.

### Abilities and Statuses

Both follow the same stateless-singleton + context pattern:

- **`Ability`** (base) — stateless singleton defining properties and `TakeEffect`. `AbilityContext` holds per-use
  state. Deferred effects use projectiles or statuses.
- **`Status`** (base) — stateless singleton defining behavior. `StatusContext` holds per-instance state. Statuses can
  hook unit events, modify stats, and tick at a fixed interval.

### Networking

All packets flow through `IPacketSender`. Incoming packets are routed to `PacketHandler` subclasses. By default
handlers only accept server-side packets; apply `[AllowClientPackets]` to accept client packets (which must be
validated by the handler).

`SynchronizationServer` sends entity snapshots every tick. `SynchronizationClient` applies them with interpolation.

All server-side gameplay logic runs exclusively in `_PhysicsProcess` for deterministic fixed-tick simulation.
