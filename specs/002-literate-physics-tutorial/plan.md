# Implementation Plan: Literate Physics Tutorial Book

**Branch**: `002-literate-physics-tutorial` | **Date**: 2026-03-05 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-literate-physics-tutorial/spec.md`

## Summary

Create a 12-chapter progressive tutorial book authored as literate F# scripts (`.fsx`) using FSharp.Formatting. The tutorial teaches 3D physics engine concepts from beginner to advanced using the BepuFSharp wrapper API, with ASCII diagrams, hands-on experiments, and a glossary. It integrates into the existing documentation site as a new "Tutorial" category (categoryindex: 3).

## Technical Context

**Language/Version**: F# 8.0 on .NET 10.0
**Primary Dependencies**: FSharp.Formatting (fsdocs-tool v21.0.0), BepuFSharp (local DLL), BepuPhysics 2.4.0, BepuUtilities 2.4.0
**Storage**: N/A (static documentation files only)
**Testing**: `dotnet fsdocs build --properties Configuration=Release` (compiles and evaluates all `.fsx` scripts)
**Target Platform**: Static HTML documentation site (GitHub Pages)
**Project Type**: Documentation / literate programming content
**Performance Goals**: N/A (static content generation)
**Constraints**: Each chapter must be self-contained; BepuFSharp wrapper API only; ASCII diagrams only
**Scale/Scope**: 12 files (11 `.fsx` + 1 `.md`), estimated 200-400 lines per chapter

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First Delivery | PASS | Spec complete with clarifications, traced to this plan |
| II. Compiler-Enforced Structural Contracts | N/A | No new `.fs`/`.fsi` modules — documentation only |
| III. Test Evidence Is Mandatory | PASS | SC-002 requires all scripts compile/evaluate via `dotnet fsdocs build`. This is the test evidence for documentation features. |
| IV. Observability and Safe Failure | N/A | No runtime code |
| V. Scripting Accessibility | N/A | No new library API; tutorial *uses* existing scripts/API |
| VI. Comprehensive Documentation | PASS | This feature *is* documentation. Uses `doc-examples` pattern (literate `.fsx` scripts in `docs/`). Build verified via `doc-build`. |

**Engineering Constraints**:
| Constraint | Status | Notes |
|------------|--------|-------|
| Primary Stack: F# on .NET | PASS | Literate F# scripts |
| `.fsi` for public modules | N/A | No new public modules |
| Surface-area baselines | N/A | No API changes |
| Documentation via doc skills | PASS | Content authored per `doc-examples` patterns; verified via `doc-build` |
| Dependencies minimized | PASS | No new dependencies |
| Packable via `dotnet pack` | N/A | No new library code |
| Scripting prelude/examples | N/A | No new API surface |

**Gate result**: PASS — no violations, no complexity tracking needed.

## Project Structure

### Documentation (this feature)

```text
specs/002-literate-physics-tutorial/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0: research findings
├── data-model.md        # Phase 1: content model
├── quickstart.md        # Phase 1: authoring guide
├── contracts/
│   └── chapter-template.md  # Phase 1: chapter structure contract
├── checklists/
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Phase 2 output (created by /speckit.tasks)
```

### Source Code (repository root)

```text
docs/
├── tutorial/
│   ├── 01-what-is-physics.fsx      # Ch 1: Introduction + F# primer
│   ├── 02-shapes.fsx               # Ch 2: Shape geometry
│   ├── 03-bodies.fsx               # Ch 3: Dynamic/static/kinematic
│   ├── 04-simulation-loop.fsx      # Ch 4: Timesteps, stepping, poses
│   ├── 05-collisions.fsx           # Ch 5: Contact events lifecycle
│   ├── 06-materials.fsx            # Ch 6: Friction & restitution
│   ├── 07-constraints.fsx          # Ch 7: Joints and springs
│   ├── 08-collision-filtering.fsx  # Ch 8: Layer-based filtering
│   ├── 09-raycasting.fsx           # Ch 9: Single/multi-hit queries
│   ├── 10-bulk-operations.fsx      # Ch 10: ECS sync patterns
│   ├── 11-capstone.fsx             # Ch 11: Complete scene
│   └── 12-glossary.md              # Ch 12: Term reference
├── index.fsx                        # Updated: add Tutorial link
├── getting-started.fsx              # Unchanged
├── ecs-integration.fsx              # Unchanged
└── adr/                             # Unchanged
```

**Structure Decision**: All new files go under `docs/tutorial/`. No changes to `src/`, `tests/`, or any `.fs`/`.fsi` files. The only change to existing files is adding a Tutorial link to `docs/index.fsx`.

## Post-Design Constitution Re-Check

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First Delivery | PASS | Plan traces to spec; all requirements addressed |
| II. Structural Contracts | N/A | Confirmed: no new modules |
| III. Test Evidence | PASS | fsdocs build evaluates all scripts |
| IV. Observability | N/A | Confirmed: no runtime code |
| V. Scripting Accessibility | N/A | Confirmed: no new API |
| VI. Documentation | PASS | Literate scripts follow `doc-examples` pattern; chapter contract defined |

**Post-design gate result**: PASS — no changes from initial check.

## Complexity Tracking

No violations to justify. Feature is documentation-only with no new code, no new dependencies, and no architectural changes.
