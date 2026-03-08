# Implementation Plan: API Safety & Missing Features

**Branch**: `004-api-safety-features` | **Date**: 2026-03-08 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/004-api-safety-features/spec.md`

## Summary

BepuFSharp 0.1.0 has crash-severity bugs (body accessors crash with `AccessViolationException` on removed bodies) and is missing fundamental interactive physics operations (impulse application, gravity modification, body enumeration). This plan adds safe accessor variants (`tryGet*`/`trySet*`), existence checks, impulse functions, enumeration, runtime gravity, shape queries, shape descriptions, and dependency version pinning — all following the existing codebase patterns (`.fsi`/`.fs` pairs, surface-area baselines, Expecto tests, inline interop).

## Technical Context

**Language/Version**: F# 8.0 on .NET 10.0
**Primary Dependencies**: BepuPhysics 2.4.0, BepuUtilities 2.4.0
**Storage**: N/A (in-memory physics simulation)
**Testing**: Expecto + FsCheck (BepuFSharp.Tests project)
**Target Platform**: Cross-platform .NET 10.0
**Project Type**: Library (NuGet package)
**Performance Goals**: All new functions are thin wrappers over BepuPhysics2 internals — no new hot paths. Enumeration (`getAllBodyIds`) should avoid unnecessary allocation where practical.
**Constraints**: No new dependencies. All new public API must have `.fsi` signatures, surface-area baselines, XML doc comments, and tests.
**Scale/Scope**: ~15 new public functions added to `PhysicsWorld` module, 1 new function on `PhysicsShape` module, dependency version pinning in `.fsproj`.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First Delivery | PASS | Spec and plan created before implementation |
| II. Compiler-Enforced Structural Contracts | PASS | All new functions will be declared in `.fsi` files; surface-area baselines will be updated |
| III. Test Evidence Is Mandatory | PASS | Each user story has acceptance scenarios; tests will cover success and failure paths |
| IV. Observability and Safe Failure Handling | PASS | `try*` variants provide explicit failure signals; impulse on removed body fails safely |
| V. Scripting Accessibility | PASS | New functions will be usable from FSI; example scripts will be updated |
| VI. Comprehensive Documentation | PASS | `.fsi` XML doc comments via `api-doc` skill; doc site build verification via `doc-build` |

**Engineering Constraints:**
- `.fsi` signature files: Required for all modified public modules (PhysicsWorld, Shapes) ✓
- Surface-area baselines: PhysicsWorld.baseline and Shapes.baseline must be updated ✓
- Dependency pinning: FR-011 directly addresses the constraint about version pinning ✓
- `dotnet pack` packaging: No changes needed — existing packaging configuration preserved ✓
- Scripts: prelude.fsx unchanged; new example script for impulse/gravity ✓

## Project Structure

### Documentation (this feature)

```text
specs/004-api-safety-features/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
BepuFSharp/
├── Types.fsi / Types.fs           # No changes
├── Diagnostics.fsi / Diagnostics.fs # No changes
├── Shapes.fsi / Shapes.fs         # Add PhysicsShape.describe function
├── Bodies.fsi / Bodies.fs         # No changes
├── Constraints.fsi / Constraints.fs # No changes
├── Interop.fs                     # No changes (existing conversions sufficient)
├── ContactEvents.fs               # No changes
├── Queries.fs                     # No changes
├── Callbacks.fs                   # Expose gravity getter/setter on DefaultPoseIntegratorCallbacks
├── PhysicsWorld.fsi / PhysicsWorld.fs # Add ~15 new public functions
└── BepuFSharp.fsproj              # Pin dependency versions with [2.4.0]

BepuFSharp.Tests/
├── baselines/
│   ├── PhysicsWorld.baseline      # Update with new function signatures
│   └── Shapes.baseline            # Update with describe function
├── BodyTests.fs                   # Add tryGet*/trySet*/exists tests
├── WorldTests.fs                  # Add gravity/enumeration tests
└── (new) ImpulseTests.fs          # Impulse application tests

scripts/examples/
└── 09-impulse-and-gravity.fsx     # New example script
```

**Structure Decision**: This feature extends the existing single-project structure. No new projects or directories needed beyond a new test file and example script.
