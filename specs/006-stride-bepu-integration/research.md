# Research: Stride BepuPhysics Integration & Query Extensions

**Feature**: 006-stride-bepu-integration
**Date**: 2026-03-22

## R1: Sweep Cast API Pattern

**Decision**: Use `Simulation.Sweep<TShape, TSweepHitHandler>` (simplified overload) with a custom `SweepHitHandler` struct implementing `ISweepHitHandler`.

**Rationale**: BepuPhysics2 provides two `Simulation.Sweep` overloads. The simplified overload uses default progression/convergence settings suitable for general use. The handler struct pattern matches existing `SingleHitHandler`/`MultiHitHandler` in Queries.fs. The `AllowTest(CollidableReference)` method on `ISweepHitHandler` is the natural injection point for collision mask filtering.

**Alternatives considered**:
- `BroadPhase.Sweep` — too low-level, only tests AABB bounding boxes without narrow-phase shape testing.
- `Tree.Sweep` — internal acceleration structure API, not intended for user-facing queries.

**Key constraint**: `Simulation.Sweep` is generic over `TShape`, requiring compile-time shape type. Implementation must dispatch on all 8 `PhysicsShape` DU cases to call the correct generic instantiation. This is unavoidable with BepuPhysics2's zero-allocation generic design.

## R2: Overlap Query API Pattern

**Decision**: Use `BroadPhase.GetOverlaps<TOverlapEnumerator>` for broad-phase candidate collection, followed by narrow-phase shape intersection testing via `Simulation.Sweep` with zero velocity (contact-at-rest test).

**Rationale**: `BroadPhase.GetOverlaps` returns all collidables whose AABB intersects the query bounding box. This is a necessary first pass but insufficient alone — AABBs are conservative approximations. A narrow-phase confirmation step (shape-vs-shape contact test) is needed to produce accurate overlap results. BepuPhysics2 does not expose a direct "shape-at-pose vs world" overlap API, so the standard approach is broad-phase + narrow confirm.

**Alternatives considered**:
- Broad-phase only (no narrow confirm) — would return false positives for non-AABB shapes.
- `Tree.GetOverlaps` — lower-level, would need to query both active and static trees separately.

**Key detail**: The `IBreakableForEach<int>` enumerator receives leaf indices, which must be mapped back to `CollidableReference` via the broad-phase's internal encoding. Return `true` from `LoopBody` to continue enumeration, `false` to stop early.

## R3: Filtered Raycasting Approach

**Decision**: Add an optional `CollisionFilter voption` field to `SingleHitHandler` and `MultiHitHandler`. When `ValueSome filter`, the `AllowTest` method performs the bidirectional mask check against the collidable's stored filter.

**Rationale**: The existing `AllowTest` in both handlers always returns `true`. The filter lookup requires accessing the per-body/per-static filter tables already stored in `DefaultNarrowPhaseCallbacks`. Since handlers are value types passed by ref, adding a filter field has zero allocation cost. The mask check logic is identical to `DefaultNarrowPhaseCallbacks.AllowContactGeneration`.

**Alternatives considered**:
- Separate `FilteredSingleHitHandler`/`FilteredMultiHitHandler` types — unnecessary duplication; a single handler with optional filter is cleaner.
- Closure-based filter callback — would require heap allocation and break the struct handler pattern.

**Backward compatibility**: The public `raycast`/`raycastAll` functions get a new optional `?filter: CollisionFilter` parameter. F# optional parameters default to `None`, preserving existing call sites.

## R4: Constraint Readback Strategy

**Decision**: Store a constraint type discriminator (int tag) in an internal `Dictionary<int, int>` (handle value → type tag) at `addConstraint` time. On readback, dispatch to the correct `Solver.GetDescription<T>` generic instantiation using a match on the stored tag.

**Rationale**: `Solver.GetDescription<T>` requires the constraint type at compile time. Without stored metadata, readback would need to try all 10 constraint types until one succeeds — fragile and slow. Storing a single int tag per constraint (mapping to the `ConstraintDesc` DU tag) is negligible overhead and enables O(1) dispatch.

**Alternatives considered**:
- Try-each-type approach — error-prone, poor performance, breaks if BepuPhysics2 throws on type mismatch.
- Store full `ConstraintDesc` copy — wastes memory duplicating data already in the solver.
- Use BepuPhysics2's `TypeBatch.TypeId` — internal detail not reliably exposed, couples to implementation.

**Key detail**: The dictionary maps `ConstraintHandle.Value` (int) → DU case tag (0-9). On `removeConstraint`, the entry is also removed. On `removeBody` (which auto-removes constraints), the constraint cleanup path must also clean the dictionary.

## R5: Runtime Filter and Material Modification

**Decision**: Expose `setCollisionFilter`, `setStaticCollisionFilter`, and `setMaterial` functions that update the internal lookup tables in `DefaultNarrowPhaseCallbacks`.

**Rationale**: The narrow phase callbacks already store per-body filter and material data (used during `AllowContactGeneration` and `ConfigureContactManifold`). Modifying these tables is sufficient — BepuPhysics2 re-evaluates contacts each step, so changes take effect immediately on the next `step` call. No need to modify the solver or body descriptions.

**Alternatives considered**:
- Remove and re-add body — destructive, loses velocity/constraints, not what users expect.
- Direct BepuPhysics2 `BodyReference` property modification — filters and materials are wrapper-level concepts stored in the callbacks, not in BepuPhysics2 body descriptions.

**Key detail**: The callbacks struct must expose internal mutation methods for the filter/material tables. Currently the tables use `Dictionary<int, CollisionFilter>` and `Dictionary<int, MaterialProperties>` keyed by handle value (with static offset encoding).

## R6: Stride.BepuPhysics Type Interop

**Decision**: Create a new `StrideInterop` module with pure conversion functions. Add `Stride.BepuPhysics` (v4.3.0.2507) as a package dependency to BepuFSharp.fsproj.

**Rationale**: The upstream PhysicsSandbox requires bidirectional conversion. Bundling in the same package (per clarification) avoids a separate NuGet package. Stride.BepuPhysics provides collider component types (SphereCollider, BoxCollider, etc.) that map 1:1 to `PhysicsShape` DU cases. Pure functions with no state keep the module simple and testable.

**Alternatives considered**:
- Separate NuGet package (BepuFSharp.Stride) — rejected per user clarification; single package is preferred.
- Runtime reflection-based conversion — unnecessary complexity; types are known at compile time.

**Compatibility concern**: Stride.BepuPhysics depends on BepuPhysics 2.5.0-beta.25 while the project uses 2.5.0-beta.28. Both are pre-release betas within the same minor version. NuGet should resolve this, but build verification is required. If incompatible, the project may need to request a Stride.BepuPhysics update.

## R7: Constraint Type Auto-Removal Cleanup

**Decision**: Hook into the existing body removal path to clean up the constraint type dictionary.

**Rationale**: `removeBody` already auto-removes constraints associated with the body. The constraint type dictionary must stay in sync. The existing removal callback path in PhysicsWorld.fs must be extended to also remove dictionary entries for auto-removed constraints.

**Alternatives considered**:
- Lazy cleanup (check validity on readback) — risks unbounded dictionary growth with frequent body add/remove cycles.
- No tracking, try-each on readback — already rejected in R4.

## Known Issues

### KI-1: Compound and Mesh sweep cast performance

**Issue**: `Simulation.Sweep` requires `IConvexShape`, which Compound and Mesh do not implement. The current implementation decomposes these shapes and sweeps each child individually:

- **Compound**: Each child convex shape is swept with its local pose composed onto the overall sweep pose. The handler tracks the closest hit across all children. Cost is O(N) sweeps where N = number of children.
- **Mesh**: Each triangle is swept individually. Cost is O(N) sweeps where N = number of triangles. This can be expensive for large meshes (hundreds or thousands of triangles).

**Mitigation**: For performance-critical mesh sweeps, callers should consider using a bounding sphere or capsule approximation instead of sweeping the full mesh. A future optimization could add broad-phase AABB culling to skip triangles far from the sweep path.

**Decision**: Ship the per-child/per-triangle decomposition for correctness. Optimize if profiling shows it as a bottleneck in upstream use cases.
