# Data Model: Data-Oriented Wrapper API & README Attribution

**Feature**: `003-data-oriented-wrapper`
**Date**: 2026-03-06

## Entities

This feature is documentation-only. There are no runtime data entities to model. The "data model" for this feature is the terminology map governing text replacements.

## Terminology Map

The canonical mapping of ECS-specific terms to architecture-neutral equivalents. This map drives all text replacements across the codebase.

### Replacement Rules

| # | Original (case-sensitive match) | Replacement | Scope |
|---|--------------------------------|-------------|-------|
| T1 | `bulk ECS operations` | `bulk game loop operations` | Types.fsi doc comment |
| T2 | `ECS sync` | `game loop sync` | docs, README |
| T3 | `ECS integration` | `game loop integration` | docs, README, tutorials |
| T4 | `Bulk ECS sync` | `Bulk game loop sync` | README table |
| T5 | `ECS Integration` (title case) | `Game Loop Integration` | doc titles, headings |
| T6 | `ECS transforms` | `engine transforms` | docs prose |
| T7 | `ECS component storage` | `component storage` | docs prose |
| T8 | `ECS archetype storage order` | `engine storage order` | docs prose |
| T9 | `ECS physics component` | `physics component array` | docs prose |
| T10 | `ECS positions` | `engine positions` | tutorial tables |
| T11 | `back to ECS` | `back to engine` | tutorial tables |
| T12 | `for ECS integration` | `for game loop integration` | README features list |
| T13 | `an ECS requires` | `a game loop requires` | README prose |

### Retention Rules (FR-006 — comparative/clarifying context)

The following ECS references are retained or adapted to clarify that ECS is one supported architecture, not a requirement:

| Context | Current Text | Adapted Text | File |
|---------|-------------|--------------|------|
| Tutorial educational section | "What Is ECS?" | Keep as-is — educational definition | 10-bulk-operations.fsx |
| Glossary definition | "ECS — Entity-Component-System..." | Keep as-is — definitional | 12-glossary.md |
| README comparative | "Synchronizing physics with an ECS..." | Rewrite to: "Synchronizing physics with a game loop (whether ECS-based or not)..." | README.md |

## File Rename Map

| Original Path | New Path | Cross-References to Update |
|---------------|----------|---------------------------|
| `docs/ecs-integration.fsx` | `docs/game-loop-integration.fsx` | `docs/index.fsx` link href |
| `scripts/examples/03-bulk-ecs-sync.fsx` | `scripts/examples/03-bulk-game-loop-sync.fsx` | None (scripts not cross-linked in docs) |

## Attribution Section Structure

```
Position: After H1 "# BepuFSharp", before existing content
Format: Italicized paragraph with inline links
Content fields:
  - Tool name: "speckit"
  - Tool link: [speckit](https://github.com/github/spec-kit)
  - AI tool: "Claude Code with Opus 4.6"
  - Constitution link: [constitution](.specify/memory/constitution.md)
  - Doc skills link: [documentation skills](.specify/memory/constitution.md#vi-comprehensive-documentation)
```
