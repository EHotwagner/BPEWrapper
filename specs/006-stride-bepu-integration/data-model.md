# Data Model: Stride BepuPhysics Integration & Query Extensions

**Feature**: 006-stride-bepu-integration
**Date**: 2026-03-22

## New Types

### SweepHit

Result of a sweep cast query. Identifies what was hit and where.

| Field | Type | Description |
|-------|------|-------------|
| Body | BodyId voption | Dynamic/kinematic body hit (if any) |
| Static | StaticId voption | Static body hit (if any) |
| Position | Vector3 | World-space contact point |
| Normal | Vector3 | Surface normal at contact point |
| Distance | float32 | Parametric distance along sweep path (0.0 = start, 1.0 = end of maxT) |

**Invariants**:
- Exactly one of Body or Static is ValueSome (never both, never neither — when a hit occurs).
- Distance is in range [0.0, maximumT].
- Normal is unit length.

### OverlapResult

A single entry from an overlap query. Identifies one body/static that intersects the test volume.

| Field | Type | Description |
|-------|------|-------------|
| Body | BodyId voption | Dynamic/kinematic body (if overlapping) |
| Static | StaticId voption | Static body (if overlapping) |

**Invariants**:
- Exactly one of Body or Static is ValueSome.

## Modified Types

### CollisionFilter (existing — no structural change)

Used as an optional query parameter for sweep casts, overlaps, and filtered raycasts. The existing `Group: uint32` and `Mask: uint32` fields are reused. The bidirectional mask check logic (`filterA.Mask &&& (1 <<< filterB.Group) <> 0 && filterB.Mask &&& (1 <<< filterA.Group) <> 0`) applies identically to query filtering.

## Internal State Changes

### Constraint Type Registry (internal to PhysicsWorld)

Tracks the `ConstraintDesc` discriminated union tag for each active constraint, enabling type-dispatched readback via `Solver.GetDescription<T>`.

| Key | Value | Description |
|-----|-------|-------------|
| ConstraintHandle.Value (int) | DU case tag (int, 0-9) | Maps constraint handle to its ConstraintDesc case |

**Lifecycle**:
- Entry added at `addConstraint` time.
- Entry removed at `removeConstraint` time.
- Entries removed in bulk when `removeBody` auto-removes associated constraints.

### Filter and Material Tables (existing — mutation access added)

The `DefaultNarrowPhaseCallbacks` struct already maintains:
- `Dictionary<int, CollisionFilter>` — per-body/static collision filters
- `Dictionary<int, MaterialProperties>` — per-body/static material properties

Runtime modification functions (`setCollisionFilter`, `setMaterial`) write directly to these tables. No structural change to the dictionaries; only new write access paths.

## Entity Relationships

```text
PhysicsWorld
├── manages → Bodies (BodyId)
│   ├── has → CollisionFilter (mutable at runtime)
│   ├── has → MaterialProperties (mutable at runtime)
│   └── connected via → Constraints (ConstraintId)
│       └── tracked by → Constraint Type Registry (internal)
├── manages → Statics (StaticId)
│   ├── has → CollisionFilter (mutable at runtime)
│   └── has → MaterialProperties (mutable at runtime)
├── queries
│   ├── raycast / raycastAll (existing + optional filter)
│   ├── sweepCast (new, returns SweepHit option)
│   └── overlap (new, returns OverlapResult[])
└── interop
    └── StrideInterop module (stateless conversions)
```

## Stride Type Mappings

Bidirectional conversion between BepuFSharp and Stride.BepuPhysics types:

| BepuFSharp Type | Stride.BepuPhysics Type | Notes |
|-----------------|------------------------|-------|
| PhysicsShape.Sphere | SphereCollider | radius ↔ Radius |
| PhysicsShape.Box | BoxCollider | width/height/length ↔ Size |
| PhysicsShape.Capsule | CapsuleCollider | radius/length ↔ Radius/Length |
| PhysicsShape.Cylinder | CylinderCollider | radius/length ↔ Radius/Height |
| PhysicsShape.Triangle | — | No direct Stride equivalent; skip or custom |
| PhysicsShape.ConvexHull | ConvexHullCollider | points ↔ Points |
| PhysicsShape.Compound | CompoundCollider | children ↔ Children (recursive) |
| PhysicsShape.Mesh | MeshCollider | triangles ↔ Mesh data |
| CollisionFilter | CollisionGroup | Group/Mask ↔ CollisionGroup bits |
| MaterialProperties | — | No direct Stride type; use property bag or custom |
| ConstraintDesc | — | Constraint conversion maps to Stride constraint components |
