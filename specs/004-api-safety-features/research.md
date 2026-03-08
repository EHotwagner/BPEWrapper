# Research: API Safety & Missing Features

**Feature**: 004-api-safety-features | **Date**: 2026-03-08

## R1: Safe body access — existence check pattern

**Decision**: Use `sim.Bodies.BodyExists(BodyHandle)` for dynamic/kinematic bodies and `sim.Statics.StaticExists(StaticHandle)` for statics. Gate all `try*` variants on this check before accessing body data.

**Rationale**: BepuPhysics2 provides these methods specifically for safe handle validation. They check internal handle-to-memory mappings without accessing body data, so they cannot trigger `AccessViolationException`. This is the same pattern the BPImprovements.md workaround uses via the escape hatch.

**Alternatives considered**:
- Try/catch around body access: Not viable — `AccessViolationException` is a corrupted state exception and cannot be caught in .NET Core.
- Maintain a separate `HashSet<BodyId>` tracking live bodies: Adds memory overhead and sync risk. The physics engine already tracks this.

## R2: Impulse application — BepuPhysics2 API

**Decision**: Use `BodyReference.ApplyImpulse(impulse, offset)`, `BodyReference.ApplyLinearImpulse(impulse)`, and `BodyReference.ApplyAngularImpulse(impulse)` accessed via `sim.Bodies[handle]`.

**Rationale**: These are the standard BepuPhysics2 methods for applying impulses. They modify the body's velocity directly (impulse = instantaneous velocity change scaled by inverse mass/inertia). They are safe to call on awake or sleeping bodies (they auto-wake sleeping bodies).

**Alternatives considered**:
- Exposing force accumulators: BepuPhysics2 doesn't have a built-in force accumulator API — forces must be applied as impulses scaled by dt. The wrapper could add a `applyForce` convenience function, but this is better deferred to a future feature.

## R3: Runtime gravity modification — Simulation.PoseIntegrator access

**Decision**: Access gravity via `sim.PoseIntegrator` which exposes the `Callbacks` property. Since `DefaultPoseIntegratorCallbacks` stores `Gravity` as a mutable `val`, it can be read and written directly: `sim.PoseIntegrator.Callbacks.Gravity`.

**Rationale**: The `Simulation<TNarrow, TPose>.PoseIntegrator` property returns a `PoseIntegrator<TPose>` with a mutable `Callbacks` field. Since `DefaultPoseIntegratorCallbacks` is a struct with `val mutable Gravity: Vector3`, the gravity can be modified in-place. No reflection needed.

**Alternatives considered**:
- Store gravity separately in PhysicsWorld and sync on step: Adds complexity and risks desync.
- Reflection: Fragile, version-dependent — exactly what the BPImprovements.md warns about.

**Implementation note**: The `PhysicsWorld` type currently stores `sim: Simulation` as `Simulation` (non-generic base type). To access `PoseIntegrator.Callbacks.Gravity`, we need access to the typed `Simulation<DefaultNarrowPhaseCallbacks, DefaultPoseIntegratorCallbacks>`. The `PhysicsWorld` constructor receives the generic simulation but stores it as the base `Simulation` type. We need to either: (a) store an additional reference to the typed simulation, (b) store the `PoseIntegrator` reference directly, or (c) store a gravity getter/setter delegate captured at construction time. Option (c) is cleanest — capture `sim.PoseIntegrator` reference at construction and store it on PhysicsWorld for gravity access.

## R4: Body enumeration — BepuPhysics2 iteration pattern

**Decision**: Use `sim.Bodies.ActiveSet` and `sim.Bodies.Sets` to iterate over all body handles. BepuPhysics2 organizes bodies into sets (set 0 = active, sets 1+ = sleeping). Iterate all sets and collect `BodyHandle` values, converting to `BodyId[]`. For statics, use `sim.Statics` which has a simpler flat indexing.

**Rationale**: BepuPhysics2's `Bodies` exposes `ActiveSet` (set index 0) and sleeping body sets. Each set has an `IndexToHandle` span mapping internal indices to `BodyHandle` values. This is the canonical way to enumerate bodies.

**Alternatives considered**:
- Track IDs in a parallel list on `addBody`/`removeBody`: Works but duplicates state the engine already maintains.

## R5: Shape query — reconstructing PhysicsShape from body

**Decision**: Given a `BodyId`, read the body's `ShapeIndex` (a `TypedIndex`), then use the simulation's shape registry to reconstruct the `PhysicsShape` DU case. The `TypedIndex.Type` identifies the shape kind (0=Sphere, 1=Capsule, 2=Box, etc.), and the shape data can be read from `sim.Shapes.GetShape<T>(index)`.

**Rationale**: BepuPhysics2 stores shape data in typed batches indexed by `TypedIndex`. Each shape type has a known type ID. Reading the shape data and reconstructing the F# DU is a reverse of the `addShape` operation.

**Alternatives considered**:
- Store a `Dictionary<BodyId, PhysicsShape>` at add time: Simpler but uses memory and can drift if shapes are shared across bodies.

**Limitation**: `ConvexHull`, `Compound`, and `Mesh` shapes store their data in Bepu-managed buffers. Reconstructing the original point/triangle arrays from these buffers is complex and potentially expensive. For these types, we may return a simplified representation (e.g., `ConvexHull` with empty points array) or return the shape type without full parameters. This is documented as a known limitation.

## R6: Dependency version pinning — NuGet exact version syntax

**Decision**: Change `<PackageReference Include="BepuPhysics" Version="2.4.0" />` to `<PackageReference Include="BepuPhysics" Version="[2.4.0]" />` (and same for BepuUtilities). The bracket syntax `[x.y.z]` in NuGet means "exactly this version" and prevents NuGet resolution from upgrading to a higher version even when another package in the graph requests it.

**Rationale**: The default `Version="2.4.0"` means "minimum 2.4.0" — NuGet will happily resolve to 2.5.0-beta if another dependency requests it. The exact version constraint prevents this.

**Alternatives considered**:
- `PrivateAssets="all"`: Prevents the dependency from flowing to consumers but doesn't prevent version conflicts in the resolution graph.
- Version range `[2.4.0, 2.5.0)`: More permissive but BepuPhysics2 doesn't follow SemVer strictly between 2.4 and 2.5. Exact pin is safer.

## R7: Shape description function — placement

**Decision**: Add a `describe` function to a new `PhysicsShape` module in `Shapes.fsi`/`Shapes.fs`. This follows the existing pattern where types and their companion modules coexist (e.g., `Pose` type + `Pose` module in Types.fsi).

**Rationale**: The `describe` function is a pure function over the `PhysicsShape` DU. Placing it in a companion module keeps the API discoverable and follows existing codebase conventions.

**Format**: `"Sphere(r=0.5)"`, `"Box(w=1, h=2, l=3)"`, `"Capsule(r=0.3, l=1.2)"`, etc. Use lowercase parameter abbreviations for conciseness.
