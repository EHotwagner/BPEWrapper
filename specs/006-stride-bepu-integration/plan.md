# Implementation Plan: Stride BepuPhysics Integration & Query Extensions

**Branch**: `006-stride-bepu-integration` | **Date**: 2026-03-22 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/006-stride-bepu-integration/spec.md`

## Summary

Extend BepuFSharp with sweep cast queries, overlap queries, filtered raycasting, constraint readback, runtime filter/material modification, and Stride.BepuPhysics type interop. The implementation adds new handler structs following existing patterns in Queries.fs, extends PhysicsWorld with new public functions, and adds a StrideInterop module for bidirectional type conversion with Stride.BepuPhysics. Version bumped to 0.2.0.

## Technical Context

**Language/Version**: F# 8.0 on .NET 10.0
**Primary Dependencies**: BepuPhysics 2.5.0-beta.28, BepuUtilities 2.5.0-beta.28, Stride.BepuPhysics 4.3.0.2507 (new)
**Storage**: N/A (stateless operations)
**Testing**: Expecto 10.*, FsCheck 2.*
**Target Platform**: .NET library (cross-platform)
**Project Type**: Library
**Performance Goals**: Query performance inherited from BepuPhysics2 engine; no additional overhead beyond handler allocation
**Constraints**: Backward compatibility with existing API surface; all existing tests must pass unmodified
**Scale/Scope**: 6 user stories, 17 functional requirements, ~10 new public functions on PhysicsWorld + 1 new module (StrideInterop)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First Delivery | ✅ Pass | Spec complete with 6 prioritized user stories, 17 FRs, acceptance scenarios |
| II. Compiler-Enforced Structural Contracts | ✅ Plan | .fsi updates needed for Types, PhysicsWorld; new .fsi for StrideInterop. Queries.fs stays internal (no .fsi). Surface-area baseline updates required. |
| III. Test Evidence Is Mandatory | ✅ Plan | Each user story has independent test criteria; tests will use existing Expecto patterns |
| IV. Observability and Safe Failure Handling | ✅ Plan | New error cases (invalid handles on readback/modification) use existing PhysicsError DU; degenerate sweep/overlap inputs return empty results |
| V. Scripting Accessibility | ✅ Plan | Prelude update + new example scripts for sweep, overlap, constraints readback |
| VI. Comprehensive Documentation | ✅ Plan | .fsi XML doc comments for all new public symbols; example scripts serve as living docs |

**Gate result**: PASS — no violations. All principles addressed in planned work.

## Project Structure

### Documentation (this feature)

```text
specs/006-stride-bepu-integration/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (.fsi signature contracts)
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
BepuFSharp/
├── Types.fsi            # MODIFY: Add SweepHit, OverlapResult types
├── Types.fs             # MODIFY: Add SweepHit, OverlapResult types
├── Diagnostics.fsi      # UNCHANGED
├── Diagnostics.fs       # UNCHANGED
├── Shapes.fsi           # UNCHANGED
├── Shapes.fs            # UNCHANGED
├── Bodies.fsi           # UNCHANGED
├── Bodies.fs            # UNCHANGED
├── Constraints.fsi      # UNCHANGED
├── Constraints.fs       # UNCHANGED
├── Interop.fs           # MODIFY: Add motorSettingsToBepu/bepuToMotorSettings helpers
├── ContactEvents.fs     # UNCHANGED
├── Queries.fs           # MODIFY: Add SweepHitHandler, OverlapEnumerator, filtered ray handlers
├── Callbacks.fs         # MODIFY: Expose internal filter/material table mutation methods
├── StrideInterop.fsi    # NEW: Stride type conversion signatures
├── StrideInterop.fs     # NEW: Stride type conversion implementations
├── PhysicsWorld.fsi     # MODIFY: Add sweep, overlap, filtered ray, constraint readback, runtime mod
└── PhysicsWorld.fs      # MODIFY: Implement new public functions

BepuFSharp.Tests/
├── QueryTests.fs        # MODIFY: Add sweep cast, overlap, filtered raycast tests
├── ConstraintTests.fs   # MODIFY: Add constraint readback tests
├── BodyTests.fs         # MODIFY: Add runtime filter/material modification tests
├── StrideInteropTests.fs # NEW: Round-trip conversion tests
└── SurfaceAreaTests.fs  # MODIFY: Update baseline for new API surface

scripts/examples/
├── 10-sweep-casts.fsx       # NEW
├── 11-overlap-queries.fsx   # NEW
└── 12-constraint-readback.fsx # NEW
```

**Structure Decision**: Single-project library. New StrideInterop module added to the existing BepuFSharp project (same package, Stride.BepuPhysics as transitive dependency per clarification). Compilation order: StrideInterop.fsi/fs inserted after Callbacks.fs and before PhysicsWorld.fsi/fs.

## Implementation Strategy

### Story Ordering & Dependencies

```text
Story 3 (Filtered Raycasting) ← no deps, modifies existing handlers
    ↓
Story 1 (Sweep Cast) ← reuses filter pattern from Story 3
Story 2 (Overlap) ← reuses filter pattern from Story 3
    ↓
Story 4 (Constraint Readback) ← independent, extends PhysicsWorld
Story 5 (Runtime Modification) ← independent, extends PhysicsWorld
    ↓
Story 6 (Stride Interop) ← depends on all types being finalized
```

### Key Technical Decisions

**Sweep Cast Handler**: New `SweepHitHandler` struct implementing `ISweepHitHandler`. Tracks closest hit (like `SingleHitHandler` for rays). Uses `Simulation.Sweep<TShape, TSweepHitHandler>` — requires dispatching on all 8 shape types since TShape is a compile-time generic parameter.

**Overlap Enumerator**: New `OverlapEnumerator` struct implementing `IBreakableForEach<CollidableReference>`. Uses `BroadPhase.GetOverlaps` with bounding box of the test shape, then performs narrow-phase shape intersection test to confirm true overlaps (broad phase returns AABB overlaps only).

**Filtered Raycasting**: Add optional `CollisionFilter` parameter to `SingleHitHandler` and `MultiHitHandler`. When present, `AllowTest` checks the collidable's filter against the query filter using the existing bidirectional mask logic from Callbacks.fs. Default (no filter) preserves backward-compatible `AllowTest() = true` behavior.

**Constraint Readback**: Store constraint type tag alongside `ConstraintHandle` in an internal dictionary (`Dictionary<ConstraintHandle, int>`) populated at `addConstraint` time. On readback, use the stored tag to dispatch to the correct `Solver.GetDescription<T>` generic instantiation without trial-and-error.

**Runtime Modification**: Access body/static references via `Simulation.Bodies[handle]` / `Simulation.Statics[handle]` and update the internal filter/material lookup tables already maintained by `DefaultNarrowPhaseCallbacks`.

**Stride Interop**: Pure conversion functions with no runtime state. Shape conversion dispatches on `PhysicsShape` DU cases. Stride.BepuPhysics types are referenced at compile time only.

## Complexity Tracking

No constitution violations to justify. All work fits within the existing single-project structure.
