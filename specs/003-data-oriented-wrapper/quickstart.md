# Quickstart: Data-Oriented Wrapper API & README Attribution

**Feature**: `003-data-oriented-wrapper`
**Date**: 2026-03-06

## Implementation Verification Checklist

This feature is documentation-only. Use this checklist to verify each change before marking complete.

### Pre-Implementation Validation

- [ ] Run `dotnet test` — confirm all tests pass (baseline)
- [ ] Run `dotnet fsdocs build` — confirm docs site builds (baseline)
- [ ] Verify no F# identifiers contain "ECS" (already confirmed in research)

### README Changes

- [ ] Attribution section added after H1 heading, before "Quick Start"
- [ ] Attribution contains "speckit" with link (or TODO placeholder)
- [ ] Attribution contains "Claude Code with Opus 4.6"
- [ ] Attribution links to constitution (`.specify/memory/constitution.md`)
- [ ] Attribution links to doc skills (constitution Principle VI anchor)
- [ ] Line 98: ECS prerequisite language neutralized
- [ ] Line 117: "Bulk ECS sync" → "Bulk game loop sync" in table
- [ ] Line 130: "ECS integration" → "game loop integration" in features list

### Types.fsi Doc Comment

- [ ] Line 8: "bulk ECS operations" → "bulk game loop operations"

### Docs Site Changes

- [ ] `docs/ecs-integration.fsx` renamed to `docs/game-loop-integration.fsx`
- [ ] All ECS references in renamed file replaced with neutral terms
- [ ] `docs/index.fsx` line 21: "ECS sync" → "game loop sync"
- [ ] `docs/index.fsx` line 60: link updated to `game-loop-integration.html` with neutral text

### Tutorial Changes

- [ ] `09-raycasting.fsx`: "Next" link text updated
- [ ] `10-bulk-operations.fsx`: Title, headings, and prose updated (keep "What Is ECS?" section)
- [ ] `11-capstone.fsx`: "ECS integration" → "game loop integration"
- [ ] `12-glossary.md`: ECS glossary entry retained (clarifying context)

### Script Changes

- [ ] `scripts/examples/03-bulk-ecs-sync.fsx` renamed to `03-bulk-game-loop-sync.fsx`
- [ ] Content within renamed script updated to neutral terminology

### Post-Implementation Validation

- [ ] `grep -ri "ECS" README.md` — zero prerequisite/assumption usages remain
- [ ] `grep -ri "ECS" BepuFSharp/*.fsi` — zero instances remain
- [ ] `grep -ri "ECS" docs/` — only comparative/educational instances remain
- [ ] `dotnet test` — all tests pass (no regressions)
- [ ] `dotnet fsdocs build` — docs site builds successfully with no broken links
- [ ] Renamed scripts remain runnable: `dotnet fsi scripts/examples/03-bulk-game-loop-sync.fsx`
