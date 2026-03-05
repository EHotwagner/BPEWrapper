# Implementation Plan: BepuPhysics2 F# Wrapper

**Branch**: `001-bepu-fsharp-wrapper` | **Date**: 2026-03-05 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-bepu-fsharp-wrapper/spec.md`

## Summary

Create an idiomatic F# wrapper library around BepuPhysics2 v2 for use in a custom data-oriented game engine. The wrapper exposes physics world management, rigid body lifecycle, shape definitions, constraints, collision events, raycasting, and bulk ECS-friendly state access through discriminated unions, opaque typed handles, and pipeline-style module functions. The physics world is mutable (matching BepuPhysics2's design) but presented with an F# functional-style API surface.

## Technical Context

**Language/Version**: F# 8.0 on .NET 8.0
**Primary Dependencies**: BepuPhysics 2.4.0, BepuUtilities 2.4.0, System.Numerics.Vectors
**Storage**: N/A (in-memory physics simulation)
**Testing**: Expecto (test framework) + FsCheck (property-based testing)
**Target Platform**: Cross-platform .NET 8.0 (Linux, Windows, macOS)
**Project Type**: Library (NuGet package)
**Performance Goals**: <1ms bulk readback for 10K bodies; <5% overhead vs raw BepuPhysics2; zero managed allocations on hot path
**Constraints**: Zero-allocation on step/read/write hot path; all public modules require .fsi signature files; mutable world with functional API surface
**Scale/Scope**: Designed for game-scale simulations (thousands to tens of thousands of bodies)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Gate | Status | Evidence |
|-----------|------|--------|----------|
| I. Spec-First Delivery | Feature spec exists with user stories, acceptance criteria, scope boundaries | PASS | spec.md: 13 stories (7 P1, 4 P2, 2 P3), 38 FRs, 12 SCs, 7 edge cases |
| II. Compiler-Enforced Structural Contracts | Plan defines .fsi files for all public modules; surface-area baselines planned | PASS | FR-030/031/032; project structure includes .fsi for every public .fs |
| III. Test Evidence Is Mandatory | Each story has independent test criteria and acceptance scenarios | PASS | All 13 stories have Given/When/Then scenarios; test project planned |
| IV. Observability and Safe Failure Handling | Structured diagnostics for operationally significant events | PASS | FR-036; Diagnostics module planned with structured error types |
| V. Scripting Accessibility | Prelude script and numbered examples planned | PASS | FR-034/035; scripts/prelude.fsx + scripts/examples/ planned |
| VI. Comprehensive Documentation | FSharp.Formatting setup, XML docs in .fsi, literate scripts, ADRs | PASS | FR-037/038; docs/ with literate scripts and ADRs planned |
| Eng: Primary Stack | F# on .NET | PASS | F# 8.0 / .NET 8.0 |
| Eng: dotnet pack | Package to local NuGet store | PASS | FR-033; Directory.Build.props with PackageOutputPath |
| Eng: .fsi files | Every public module has .fsi | PASS | Project structure shows .fsi for each public .fs |
| Eng: Surface-area baselines | Baseline files validated in CI | PASS | SurfaceAreaTests.fs planned |

All gates pass. No violations to justify.

## Project Structure

### Documentation (this feature)

```text
specs/001-bepu-fsharp-wrapper/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── public-api.md
├── checklists/
│   └── requirements.md
└── tasks.md
```

### Source Code (repository root)

```text
BepuFSharp/
├── BepuFSharp.fsproj
├── Types.fsi
├── Types.fs
├── Shapes.fsi
├── Shapes.fs
├── Materials.fsi
├── Materials.fs
├── Bodies.fsi
├── Bodies.fs
├── Constraints.fsi
├── Constraints.fs
├── Callbacks.fs                  # internal, no .fsi
├── Queries.fs                    # internal, no .fsi
├── ContactEvents.fs              # internal, no .fsi
├── PhysicsWorld.fsi
├── PhysicsWorld.fs
├── Interop.fs                    # internal, no .fsi
├── Diagnostics.fsi
├── Diagnostics.fs
Directory.Build.props

BepuFSharp.Tests/
├── BepuFSharp.Tests.fsproj
├── WorldTests.fs
├── BodyTests.fs
├── ShapeTests.fs
├── ConstraintTests.fs
├── QueryTests.fs
├── BulkOperationTests.fs
├── ContactEventTests.fs
├── PropertyTests.fs
├── SurfaceAreaTests.fs

scripts/
├── prelude.fsx
└── examples/
    ├── 01-hello-physics.fsx
    ├── 02-body-management.fsx
    ├── 03-bulk-ecs-sync.fsx
    ├── 04-constraints.fsx
    ├── 05-raycasting.fsx
    ├── 06-collision-events.fsx
    ├── 07-collision-filtering.fsx
    └── 08-materials.fsx

docs/
├── index.fsx
├── getting-started.fsx
├── ecs-integration.fsx
└── adr/
    ├── 001-mutable-world.md
    ├── 002-du-shapes-constraints.md
    ├── 003-opaque-handles.md
    └── 004-callback-strategy.md
```

**Structure Decision**: Single library project (`BepuFSharp/`) with a companion test project (`BepuFSharp.Tests/`). F# compilation order requires a specific file ordering in the .fsproj. Internal modules (`Callbacks.fs`, `Interop.fs`) have no .fsi files since they expose no public surface. The `scripts/` and `docs/` directories sit at repository root per constitution requirements.

## Architecture Decisions

### AD-1: Mutable World, Functional API Surface

BepuPhysics2's `Simulation` is fundamentally mutable. Rather than pretending immutability (which would require deep-copying gigabytes of physics state), the wrapper acknowledges mutation. `PhysicsWorld` is a mutable container, but the module functions follow F# pipeline conventions (`world |> PhysicsWorld.addBody desc`). This is the same pattern used by `System.Collections.Generic` wrappers in F#.

### AD-2: Discriminated Unions for Shape and Constraint Variants

Instead of mirroring BepuPhysics2's many concrete struct types, shapes and constraints are represented as DUs. The wrapper performs the match internally and constructs the appropriate BepuPhysics type. This sacrifices some performance at creation time (DU allocation for reference-type cases) but creation is not a hot path. For shapes with heap data (ConvexHull, Mesh, Compound), the DU case is already reference-typed.

### AD-3: Opaque Handle Types

`BodyId`, `StaticId`, `ShapeId`, and `ConstraintId` are single-case struct DUs or `[<Struct>]` wrappers around the raw integer handles. This prevents passing a `BodyHandle` where a `StaticHandle` is expected -- a common mistake in C#.

### AD-4: Internal Callback Structs Own the Event Buffer

The default `NarrowPhaseCallbacks` struct holds a reference to a shared `ContactEventBuffer` (a resizable flat array). During narrow phase processing, contacts are appended. After `step` returns, the buffer is swapped to a read-side buffer and the write-side is cleared. This avoids exposing BepuPhysics2's callback threading model to the user.

### AD-5: Bulk Operations Use Pre-Allocated Arrays

`readPoses` and `writePoses` take pre-allocated arrays to avoid allocation. In future .NET versions, these could accept `Span<Pose>` for stack-allocated buffers. The current API uses arrays for broad compatibility.

### AD-6: Collision Filtering via Bitmask

Each body/static stores a `uint32` group and `uint32` mask. Contact is generated if `(a.mask &&& (1u <<< b.group)) <> 0u && (b.mask &&& (1u <<< a.group)) <> 0u`. This is evaluated in `AllowContactGeneration`. Up to 32 collision layers are supported.

### AD-7: .fsi as API Contract (Constitution II)

Every public module has a curated `.fsi` file that serves as the compiler-enforced structural contract. XML doc comments are authored exclusively in `.fsi` files. Implementation files (`.fs`) contain no doc comments. Internal modules (`Callbacks.fs`, `Interop.fs`) do not require `.fsi` files since they have no public surface.

## F#-Specific Interop Considerations

| BepuPhysics2 Pattern | F# Challenge | Wrapper Solution |
|----------------------|-------------|-----------------|
| Generic struct callbacks (`where T : struct, IInterface`) | F# supports `[<Struct>]` types implementing interfaces | Default callbacks are `[<Struct>]` types in `Callbacks.fs`; `createCustom` allows user-supplied structs |
| Mutable struct ref parameters | F# requires explicit `&` and `byref` | Inline helper functions in `Interop.fs` that handle address-of operations |
| Implicit operators (`RigidPose` from `Vector3`) | F# does not support implicit conversions | Explicit `Pose.ofPosition`, `Pose.ofPositionAndOrientation` functions |
| `MethodImplOptions.AggressiveInlining` | F# supports `[<MethodImpl(...)>]` | Applied to hot-path conversion functions |
| `out` parameters | F# uses `outref<'T>` or returns via tuple | Conversion helpers return tuples instead |

## Complexity Tracking

No constitution violations to justify. All gates pass.
