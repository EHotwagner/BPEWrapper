# Research: BepuPhysics2 F# Wrapper

**Feature**: 001-bepu-fsharp-wrapper
**Date**: 2026-03-05

## R-1: BepuPhysics2 v2 NuGet Packages

**Decision**: Use `BepuPhysics` 2.4.0 and `BepuUtilities` 2.4.0 NuGet packages.

**Rationale**: These are the official v2 packages published by the BepuPhysics2 author. The `BepuPhysics` package depends on `BepuUtilities` (memory management, SIMD math types). Version 2.4.0 is the latest stable release targeting modern .NET.

**Alternatives considered**:
- Building from source: Rejected -- adds build complexity and breaks reproducibility.
- Using prerelease versions: Rejected -- stability is required for a wrapper library.

## R-2: F# Struct Callback Implementation

**Decision**: Default callbacks (`NarrowPhaseCallbacks`, `PoseIntegratorCallbacks`) are implemented as `[<Struct>]` types in an internal `Callbacks.fs` module.

**Rationale**: BepuPhysics2 requires callback types that are `struct` and implement specific interfaces (`INarrowPhaseCallbacks`, `IPoseIntegratorCallbacks`). F# supports `[<Struct>]` types implementing interfaces. The callbacks are internal because users interact with higher-level wrapper functions. Advanced users can bypass defaults via `PhysicsWorld.createCustom`.

**Alternatives considered**:
- Class-based callbacks: Rejected -- BepuPhysics2 requires `struct` constraint.
- Exposing callbacks publicly: Rejected -- leaks BepuPhysics2 internals. The `createCustom` escape hatch covers advanced use.

## R-3: Opaque Handle Type Strategy

**Decision**: Use single-case `[<Struct>]` discriminated unions for `BodyId`, `StaticId`, `ShapeId`, `ConstraintId`.

**Rationale**: Single-case struct DUs in F# are zero-cost wrappers that provide type safety without runtime overhead. They prevent accidental misuse (e.g., passing a body handle where a static handle is expected). They are pattern-matchable for when users need the raw handle value.

**Alternatives considered**:
- `[<Struct>]` record wrappers: Similar cost but less idiomatic for single-field wrappers in F#.
- Raw integer handles: Rejected -- no type safety, error-prone.
- Type aliases: Rejected -- aliases are erased at compile time, no safety.

## R-4: Mutable World vs Immutable Wrapper

**Decision**: `PhysicsWorld` is a mutable type. Module functions mutate it in place. The API follows F# pipeline conventions (world as last parameter).

**Rationale**: BepuPhysics2's `Simulation` is inherently mutable and holds gigabytes of physics state in native buffers. Copying state for immutability is infeasible. The mutable-world-with-functional-API pattern is well-established in F# (e.g., `System.Collections.Generic` interop, `MailboxProcessor`).

**Alternatives considered**:
- Immutable world with copy-on-write: Rejected -- impractical for physics state sizes.
- Builder pattern: Rejected -- awkward for ongoing simulation step loops.

## R-5: Contact Event Buffer Strategy

**Decision**: Double-buffered flat array. Write buffer collects events during `step`; after step, buffers are swapped. `getContactEvents` reads from the completed buffer.

**Rationale**: BepuPhysics2's narrow phase callbacks execute on multiple threads. A shared write buffer with a lock or concurrent collection is needed during stepping. After step completes, the write buffer is swapped to become the read buffer (lock-free read). This avoids exposing BepuPhysics2's threading model to the user.

**Alternatives considered**:
- Callback-based events: Rejected -- callbacks during step execute on worker threads, not user-friendly.
- Single buffer with copy: Similar but wastes an allocation per frame.

## R-6: Collision Filtering Model

**Decision**: 32-layer bitmask model with `uint32` group and `uint32` mask per body/static.

**Rationale**: Bitmask filtering is the standard approach in game physics (used by Unity, PhysX, Bullet). 32 layers is sufficient for the vast majority of games. The filter check `(a.mask &&& (1u <<< b.group)) <> 0u && (b.mask &&& (1u <<< a.group)) <> 0u` is evaluated in `AllowContactGeneration` and has negligible cost.

**Alternatives considered**:
- String-tagged layers: Rejected -- requires dictionary lookup, allocates, slower.
- 64-bit masks: Rejected -- 32 layers is industry-standard; 64-bit adds complexity without clear need.

## R-7: Testing Strategy

**Decision**: Expecto for test framework, FsCheck for property-based tests. Separate test project `BepuFSharp.Tests`.

**Rationale**: Expecto is idiomatic for F# testing with good assertion primitives and performance test support. FsCheck enables property-based testing for invariants like "pose round-trip preserves values" and "removing a body invalidates its handle." Surface-area baseline tests validate .fsi conformance.

**Alternatives considered**:
- xUnit: Viable but less idiomatic for F#.
- NUnit: Viable but less idiomatic for F#.

## R-8: Documentation Tooling

**Decision**: FSharp.Formatting (`fsdocs`) for documentation site generation. XML doc comments in `.fsi` files. Literate F# scripts (`.fsx`) in `docs/`.

**Rationale**: Constitution VI mandates FSharp.Formatting. XML docs authored in `.fsi` files are consumed by the compiler and emitted to assembly XML, which `fsdocs` uses for API reference pages. Literate scripts provide executable, validated documentation.

**Alternatives considered**: None -- constitution mandates this approach.

## R-9: BepuPhysics2 API Verification (from source code)

**Decision**: The following BepuPhysics2 API details were verified from source and affect wrapper design:

**Findings**:

1. **Namespaces**: Core types in `BepuPhysics`, shapes in `BepuPhysics.Collidables`, constraints in `BepuPhysics.Constraints`, callbacks in `BepuPhysics.CollisionDetection` (narrow phase) and `BepuPhysics` (pose integrator), memory in `BepuUtilities.Memory`.

2. **Box shape uses half-extents**: `Box` constructor takes `HalfWidth`, `HalfHeight`, `HalfLength`. The wrapper's `PhysicsShape.Box(width, height, length)` must halve dimensions internally.

3. **Raycast uses callback pattern**: `Simulation.RayCast<THitHandler>(origin, direction, maxT, pool, ref handler)` requires an `IRayHitHandler` struct. The wrapper must implement internal handler structs for single-hit and multi-hit raycasts. This is NOT a simple function returning a result.

4. **ComputeInertia only on IConvexShape**: `Mesh`, `Compound`, and `BigCompound` do not implement `ComputeInertia`. The wrapper must handle these cases separately (e.g., require explicit inertia for compound/mesh bodies, or compute from constituent shapes).

5. **Constraint addition via Solver**: `Simulation.Solver.Add<TDescription>(bodyHandleA, bodyHandleB, in description)` returns `ConstraintHandle`. Constraint types must be `unmanaged` structs implementing `ITwoBodyConstraintDescription<T>`.

6. **SpringSettings internals**: Stores `AngularFrequency` and `TwiceDampingRatio`. Constructor `SpringSettings(frequency, dampingRatio)` converts to internal representation. The wrapper's `SpringConfig` maps directly.

7. **Simulation.Create pattern**: `Simulation.Create<TNarrow, TPose>(bufferPool, narrowCallbacks, poseCallbacks, solveDescription)` where both callback types must be `struct`.

8. **Simulation.Timestep**: `Simulation.Timestep(float dt, IThreadDispatcher threadDispatcher = null)` -- pass null for single-threaded.

9. **SolveDescription**: `SolveDescription(velocityIterationCount, substepCount)` -- maps directly to PhysicsConfig fields.

10. **Latest versions**: NuGet has 2.4.0 stable and 2.5.0-beta.28 prerelease. Targeting 2.4.0 stable.

**Sources**: BepuPhysics2 GitHub source code, NuGet Gallery.

## R-10: F# Interop Patterns for BepuPhysics2

**Decision**: Internal `Interop.fs` module with inline helper functions for struct conversions, byref handling, and address-of operations.

**Rationale**: BepuPhysics2 uses patterns (mutable struct ref parameters, implicit operators, `out` parameters) that require explicit handling in F#. Centralizing these conversions in an internal module keeps the public API clean. `[<MethodImpl(MethodImplOptions.AggressiveInlining)>]` is applied to hot-path conversions to eliminate call overhead.

**Alternatives considered**:
- Inline conversions at call sites: Rejected -- duplicates boilerplate.
- C# shim library: Rejected -- adds a project, build complexity, and violates F#-primary stack constraint.

## R-11: Auto-Removal of Constraints on Body Removal

**Decision**: When a body is removed, all constraints referencing it are automatically removed first. A diagnostic event is emitted for each auto-removed constraint.

**Rationale**: Clarification session resolved this -- auto-removal follows the principle of least surprise for game developers. Entities are frequently destroyed in game loops, and requiring manual constraint cleanup adds boilerplate and risk of orphaned constraints. The diagnostic emission satisfies Constitution IV (observability).

**Alternatives considered**:
- Error on removal: Rejected per clarification -- too burdensome for game development workflows.
