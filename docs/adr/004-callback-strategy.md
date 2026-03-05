---
title: "ADR-004: Internal Callback Structs with Shared Event Buffer"
category: Architecture Decisions
categoryindex: 2
index: 4
---

# ADR-004: Internal Callback Structs with Shared Event Buffer

## Status

Accepted

## Context

BepuPhysics2 requires users to implement `INarrowPhaseCallbacks` and `IPoseIntegratorCallbacks`
as struct types. These callbacks are called on every collision pair and every body integration
step — they are the hottest code paths in the simulation. The callback structs are copied into
the `Simulation` at creation time, but reference-type fields are shared by reference.

## Decision

The wrapper provides default internal callback structs:

- **`DefaultNarrowPhaseCallbacks`** — A `[<Struct>]` implementing `INarrowPhaseCallbacks` with:
  - Per-body material lookup from a shared `Dictionary<int, MaterialProperties>`
  - Collision filtering via shared `Dictionary<int, CollisionFilter>`
  - Contact event recording to a shared `ContactEventBuffer`

- **`DefaultPoseIntegratorCallbacks`** — A `[<Struct>]` implementing `IPoseIntegratorCallbacks`
  with SIMD gravity integration using `Vector3Wide`.

The `ContactEventBuffer` is a double-buffered class:
1. During `Simulation.Timestep`, callbacks append events to the write buffer (thread-safe via lock)
2. After timestep, `SwapBuffers()` moves write buffer to read buffer and emits `Ended` events
   for pairs that were in the previous frame but not the current frame
3. `GetEvents()` returns the read buffer

Material and filter dictionaries are created once in `PhysicsWorld.create` and shared by
reference with both the callback struct and the `PhysicsWorld` instance.

## Consequences

- Users get contact events, collision filtering, and per-body materials without implementing callbacks
- The `simulation` escape hatch allows advanced users to access raw BepuPhysics2 APIs
- The dictionary-based material/filter lookup adds a hash lookup per collision pair (acceptable overhead)
- The lock on `AppendContact` is per-frame contention — acceptable for typical body counts
- Adding new callback features (e.g., custom contact modification) requires extending the internal structs
