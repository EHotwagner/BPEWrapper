---
title: "ADR-001: Mutable World with Functional API"
category: Architecture Decisions
categoryindex: 2
index: 1
---

# ADR-001: Mutable World with Functional API

## Status

Accepted

## Context

BepuPhysics2's `Simulation` is fundamentally mutable — it manages large contiguous memory buffers,
body arrays, and constraint graphs that are updated in place. Wrapping this in an immutable F#
interface would require deep-copying gigabytes of physics state on every operation.

## Decision

`PhysicsWorld` is a **mutable sealed class** that owns the simulation. Module functions
(`PhysicsWorld.step`, `PhysicsWorld.addBody`, etc.) take the world as the **last parameter**
to enable F# pipeline syntax:

```fsharp
world
|> PhysicsWorld.addBody desc
|> ignore
```

The world mutates in place. This is the same pattern used by `System.Collections.Generic`
wrappers in F# (e.g., `dict |> Dictionary.add key value`).

## Consequences

- Users must understand that `PhysicsWorld` functions mutate state
- The world implements `IDisposable` for deterministic cleanup
- Thread safety is the caller's responsibility (same as BepuPhysics2)
- Pipeline syntax works naturally despite mutation
- No performance penalty from copying state
