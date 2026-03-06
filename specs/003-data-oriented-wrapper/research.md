# Research: Data-Oriented Wrapper API & README Attribution

**Feature**: `003-data-oriented-wrapper`
**Date**: 2026-03-06

## Decision 1: Speckit Repository URL

**Decision**: Use `https://github.com/github/spec-kit` as the speckit repository URL.

**Rationale**: URL provided directly by the project maintainer.

## Decision 2: ECS Reference Audit Results

**Decision**: 32 ECS references require replacement across 8 non-spec files. Two files require renaming. Zero F# identifiers (function/type/parameter names) contain "ECS".

**Rationale**: Full-repository grep + identifier search confirmed all ECS references are in documentation text and comments only. No code changes needed.

### Files requiring changes

| File | ECS Refs | Action |
|------|----------|--------|
| `README.md` | 3 | Replace 2 prerequisite usages; keep/rewrite 1 comparative usage |
| `BepuFSharp/Types.fsi` | 1 | Replace "bulk ECS operations" in XML doc comment |
| `docs/index.fsx` | 2 | Replace "ECS sync" and "ECS Integration" link text |
| `docs/ecs-integration.fsx` | 10 | **Rename** to `game-loop-integration.fsx` + replace all |
| `docs/tutorial/09-raycasting.fsx` | 1 | Update "Next" link text |
| `docs/tutorial/10-bulk-operations.fsx` | 13 | Replace titles and most refs; keep "What Is ECS?" as educational |
| `docs/tutorial/11-capstone.fsx` | 1 | Replace "ECS integration" |
| `docs/tutorial/12-glossary.md` | 1 | Keep glossary definition (clarifying context per FR-006) |
| `scripts/examples/03-bulk-ecs-sync.fsx` | 1+ | **Rename** to `03-bulk-game-loop-sync.fsx` + content update |

### References that MAY stay (FR-006 comparative/clarifying)

- `docs/tutorial/10-bulk-operations.fsx` "What Is ECS?" section — educational explanation of the pattern
- `docs/tutorial/12-glossary.md` ECS glossary entry — definitional
- `README.md` line 98 — can be rewritten to clarify ECS is one use case, not the only one

### File renames

1. `docs/ecs-integration.fsx` → `docs/game-loop-integration.fsx`
2. `scripts/examples/03-bulk-ecs-sync.fsx` → `scripts/examples/03-bulk-game-loop-sync.fsx`

**Cross-references to update after renames**:
- `docs/index.fsx` line 60: link `ecs-integration.html` → `game-loop-integration.html`
- Any tutorial cross-links referencing the old filename

### F# identifier audit

**Result**: No F# function names, type names, or parameter names contain "ecs" or "ECS". The only `.fsi` reference is a doc comment string in `Types.fsi:8`. Scope confirmed as documentation-only.

## Decision 3: Canonical Terminology Map

**Decision**: Use "game loop" as the replacement stem for all ECS-specific terms.

**Rationale**: "Game loop" is the universal concept shared by all data-oriented engines regardless of architecture. It accurately describes per-frame synchronization without implying any specific pattern.

| Original Term | Replacement |
|---------------|-------------|
| ECS sync | game loop sync |
| ECS integration | game loop integration |
| Bulk ECS sync | Bulk game loop sync |
| ECS operations | game loop operations |
| ECS transforms | engine transforms |
| ECS component storage | component storage |
| ECS archetype storage | engine storage |
| ECS physics component | physics component array |

## Decision 4: Attribution Section Format

**Decision**: A concise paragraph immediately after the H1 heading, using italicized text with inline links.

**Rationale**: A lightweight format avoids visual clutter at the top of a technical README while still being immediately visible. Links use relative paths for in-repo files and absolute URLs for external resources.

**Links**:
- Speckit: `https://github.com/github/spec-kit`
- Constitution: `.specify/memory/constitution.md` (relative path)
- Doc skills: `.specify/memory/constitution.md#vi-comprehensive-documentation` (relative path with anchor to Principle VI)
