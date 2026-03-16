# Implementation Plan: Force, Impulse, and Torque Application

**Branch**: `005-force-impulse-torque` | **Date**: 2026-03-16 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-force-impulse-torque/spec.md`

## Summary

Add 8 new functions to the `PhysicsWorld` module for applying forces, impulses, and torques to dynamic bodies. The implementation delegates to BepuPhysics2's existing `BodyReference.ApplyLinearImpulse`, `ApplyAngularImpulse`, and `ApplyImpulse` methods, with automatic wake-on-apply behavior and kinematic body skipping. Force/torque functions accept an explicit `dt` parameter and convert to impulses internally.

## Technical Context

**Language/Version**: F# 8.0 on .NET 8.0
**Primary Dependencies**: BepuPhysics 2.5.0-beta.28, BepuUtilities 2.5.0-beta.28, System.Numerics.Vectors
**Storage**: N/A (stateless operations — no persistent data)
**Testing**: Expecto + FsCheck (existing test framework), surface area baseline tests
**Target Platform**: Cross-platform (.NET 8.0)
**Project Type**: Library (NuGet-packable F# wrapper)
**Performance Goals**: Zero-allocation single-body operations; bulk operations at array-iteration speed
**Constraints**: No new mutable state on PhysicsWorld; no callback modifications
**Scale/Scope**: 8 new public functions added to PhysicsWorld module

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First Delivery | PASS | Spec completed at `specs/005-force-impulse-torque/spec.md` with traceable requirements FR-001 through FR-009 |
| II. Compiler-Enforced Structural Contracts | PASS | Contract defined at `contracts/PhysicsWorld-forces.fsi`; implementation will update `PhysicsWorld.fsi` and baseline |
| III. Test Evidence Is Mandatory | PASS | Test plan covers each user story independently; ForceTests.fs will be created |
| IV. Observability and Safe Failure Handling | PASS | Uses existing `ThrowIfDisposed()` pattern; kinematic bodies silently skipped (consistent with physics semantics) |
| V. Scripting Accessibility | PASS | New functions will be usable from FSI via existing prelude; example scripts should demonstrate force application |
| VI. Comprehensive Documentation | PASS | XML doc comments will be added to `.fsi` file for all 8 new functions |

**Post-Phase 1 Re-check**: All gates still pass. No new types or modules introduced — only additions to existing `PhysicsWorld` module. No callback modifications. No new dependencies.

## Project Structure

### Documentation (this feature)

```text
specs/005-force-impulse-torque/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: BepuPhysics2 API research
├── data-model.md        # Phase 1: Entity analysis (no new types)
├── quickstart.md        # Phase 1: Implementation guide
├── contracts/
│   └── PhysicsWorld-forces.fsi  # Phase 1: API contract for new functions
├── checklists/
│   └── requirements.md  # Spec quality checklist
└── tasks.md             # Phase 2 output (via /speckit.tasks)
```

### Source Code (repository root)

```text
BepuFSharp/
├── PhysicsWorld.fs       # Modified: add 8 new functions
├── PhysicsWorld.fsi      # Modified: add 8 new function signatures with XML docs
└── (no new files)

BepuFSharp.Tests/
├── ForceTests.fs         # New: tests for force/impulse/torque functions
├── baselines/
│   └── PhysicsWorld.baseline  # Modified: updated surface area baseline
└── (existing test files unchanged)
```

**Structure Decision**: No new modules or projects needed. All 8 functions are additions to the existing `PhysicsWorld` module, following the established pattern of `PhysicsWorld.functionName : args -> PhysicsWorld -> unit`.

## Complexity Tracking

No constitution violations. No complexity justifications needed.
