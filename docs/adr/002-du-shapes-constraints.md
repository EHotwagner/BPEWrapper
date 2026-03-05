---
title: "ADR-002: Discriminated Unions for Shapes and Constraints"
category: Architecture Decisions
categoryindex: 2
index: 2
---

# ADR-002: Discriminated Unions for Shapes and Constraints

## Status

Accepted

## Context

BepuPhysics2 defines shapes and constraints as separate concrete struct types (`Sphere`, `Box`,
`Capsule`, `BallSocket`, `Hinge`, etc.). In C#, users construct the specific type and pass it
to generic methods. F# users expect a more unified type-safe approach.

## Decision

Shapes are represented as a single discriminated union `PhysicsShape` with 8 cases:

```fsharp
type PhysicsShape =
    | Sphere of radius: float32
    | Box of width: float32 * height: float32 * length: float32
    | Capsule of radius: float32 * length: float32
    | Cylinder of radius: float32 * length: float32
    | Triangle of a: Vector3 * b: Vector3 * c: Vector3
    | ConvexHull of points: Vector3[]
    | Compound of children: CompoundChild[]
    | Mesh of triangles: (Vector3 * Vector3 * Vector3)[]
```

Constraints use `ConstraintDesc` with 10 cases (BallSocket, Hinge, Weld, etc.).

The wrapper internally matches on the DU and constructs the appropriate BepuPhysics2 struct.

## Consequences

- Exhaustive pattern matching catches missing cases at compile time
- Shape/constraint creation has a small overhead from DU allocation (not a hot path)
- Cases with heap data (ConvexHull, Mesh, Compound) are already reference-typed
- Adding new shape/constraint types requires updating the DU and the match in PhysicsWorld
- Users get a single, discoverable type instead of 18+ separate struct types
