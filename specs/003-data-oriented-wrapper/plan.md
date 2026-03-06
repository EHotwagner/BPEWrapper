# Implementation Plan: Data-Oriented Wrapper API & README Attribution

**Branch**: `003-data-oriented-wrapper` | **Date**: 2026-03-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-data-oriented-wrapper/spec.md`

## Summary

Replace ECS-specific terminology with architecture-neutral phrasing ("game loop sync", "game loop integration") across all public-facing documentation: README.md, `.fsi` XML doc comments, literate documentation scripts (`docs/`), tutorial chapters, and example scripts. Add a speckit attribution section to the README with links to the speckit repository, project constitution, and documentation skills (Principle VI). This is a documentation-only feature — no API signatures or runtime code will change.

## Technical Context

**Language/Version**: F# 8.0 on .NET 10.0
**Primary Dependencies**: FSharp.Formatting (fsdocs-tool v21.0.0) for documentation site generation
**Storage**: N/A (no data storage — documentation files only)
**Testing**: Expecto + FsCheck (existing test suite; `dotnet test` for regression validation)
**Target Platform**: Cross-platform (.NET 10.0)
**Project Type**: Library (documentation-only changes in this feature)
**Performance Goals**: N/A (no runtime changes)
**Constraints**: Documentation-only scope; no API signature changes; renamed files must not break doc site navigation
**Scale/Scope**: 9 files across README, docs/, scripts/examples/, and one .fsi doc comment

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Spec-First Delivery | PASS | Feature has spec, clarifications, and this plan. All changes traceable to FR-001 through FR-008. |
| II. Compiler-Enforced Structural Contracts | PASS | No API surface changes. Types.fsi doc comment update only (XML summary text). No new .fsi files needed. Surface-area baselines unaffected. |
| III. Test Evidence Is Mandatory | PASS (exemption) | This is a documentation-only feature with zero behavior changes. Existing tests (`dotnet test`) must pass to confirm no regressions. No new tests required. |
| IV. Observability and Safe Failure Handling | N/A | No runtime code changes. |
| V. Scripting Accessibility | PASS | `scripts/examples/03-bulk-ecs-sync.fsx` will be renamed to `03-bulk-game-loop-sync.fsx`. Content updated to use neutral terminology. Script must remain runnable after changes. |
| VI. Comprehensive Documentation | PASS | Changes use `api-doc` skill for .fsi update, `doc-examples` skill for literate script updates, and `doc-build` skill for site verification. |

**Gate result: ALL PASS** — no violations requiring justification.

## Project Structure

### Documentation (this feature)

```text
specs/003-data-oriented-wrapper/
├── plan.md              # This file
├── research.md          # Phase 0 output — ECS reference audit + speckit URL research
├── data-model.md        # Phase 1 output — terminology map
├── quickstart.md        # Phase 1 output — verification checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
# Files to modify (documentation-only changes)
README.md                                          # Attribution section + ECS terminology replacement
BepuFSharp/Types.fsi                               # XML doc comment: "bulk ECS operations" → neutral

docs/
├── index.fsx                                      # "ECS sync" → "game loop sync"; link text update
├── ecs-integration.fsx → game-loop-integration.fsx  # Rename + rewrite ECS refs to neutral terms

docs/tutorial/
├── 09-raycasting.fsx                              # Update "Next" link text
├── 10-bulk-operations.fsx                         # Rewrite ECS-centric sections to neutral terms
├── 11-capstone.fsx                                # One mention of "ECS integration"
└── 12-glossary.md                                 # Update glossary entry

scripts/examples/
└── 03-bulk-ecs-sync.fsx → 03-bulk-game-loop-sync.fsx  # Rename + content update
```

**Structure Decision**: No new directories or projects. All changes are edits or renames within existing file tree.

## Complexity Tracking

> No violations to justify — all constitution gates pass.
