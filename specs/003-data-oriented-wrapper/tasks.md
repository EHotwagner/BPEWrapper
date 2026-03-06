# Tasks: Data-Oriented Wrapper API & README Attribution

**Input**: Design documents from `/specs/003-data-oriented-wrapper/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/terminology-contract.md, quickstart.md

**Tests**: Not required — this is a documentation-only feature. Validation is via grep checks, `dotnet test` (regression), and `dotnet fsdocs build` (doc site integrity).

**Organization**: Tasks are grouped by user story. US2 and US3 can run in parallel after US1 completes (US1 establishes README baseline that US2 appends to).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Baseline Validation)

**Purpose**: Confirm existing tests and docs build pass before making any changes

- [x] T001 Run `dotnet test` and confirm all tests pass (baseline snapshot)
- [x] T002 Run `dotnet fsdocs build` and confirm docs site builds without errors (baseline snapshot)

---

## Phase 2: Foundational (File Renames)

**Purpose**: Rename ECS-named files before editing content. Must complete before user story phases to avoid editing files that will move.

- [x] T003 [P] Rename docs/ecs-integration.fsx to docs/game-loop-integration.fsx via `git mv docs/ecs-integration.fsx docs/game-loop-integration.fsx`
- [x] T004 [P] Rename scripts/examples/03-bulk-ecs-sync.fsx to scripts/examples/03-bulk-game-loop-sync.fsx via `git mv scripts/examples/03-bulk-ecs-sync.fsx scripts/examples/03-bulk-game-loop-sync.fsx`

**Checkpoint**: Both files renamed, git tracks the renames. No content changes yet.

---

## Phase 3: User Story 1 - Data-Oriented Integration Without ECS (Priority: P1)

**Goal**: Replace ECS-specific terminology in README.md with architecture-neutral phrasing per the terminology map in data-model.md, and validate that bulk API already accepts plain arrays without ECS types.

**Independent Test**: Search README.md for "ECS" and verify zero instances appear as prerequisites or assumptions. Only comparative/clarifying usage (FR-006) should remain.

- [x] T005 [US1,US3] In README.md, replace line 98 "Synchronizing physics with an ECS requires reading thousands of poses per frame." with neutral phrasing: "Synchronizing physics with a game loop (whether ECS-based or not) requires reading thousands of poses per frame." (FR-006 comparative context)
- [x] T006 [US1,US3] In README.md summary table (line 117), replace "Bulk ECS sync" with "Bulk game loop sync" in the Concern column (FR-008, terminology map T4)
- [x] T007 [US1,US3] In README.md features list (line 130), replace "Bulk pose and velocity read/write for ECS integration" with "Bulk pose and velocity read/write for game loop integration" (FR-005, terminology map T12)
- [x] T008 [US1] Validate FR-007: inspect BepuFSharp/PhysicsWorld.fsi to confirm `readPoses`, `writePoses`, `readVelocities`, `writeVelocities` accept plain arrays (no ECS types in signatures). Document validation result as a comment in this task — no code changes.

**Checkpoint**: README.md ECS terminology is neutral. Bulk API validated as ECS-agnostic. US1 acceptance scenarios 1-3 satisfied.

---

## Phase 4: User Story 2 - README Speckit Attribution (Priority: P2)

**Goal**: Add a speckit/Claude Code attribution section to README.md as the first content after the H1 heading.

**Independent Test**: Read README.md and verify the attribution section appears before "Quick Start", contains all required text and links (FR-001 through FR-004).

- [x] T009 [US2] In README.md, insert an attribution paragraph immediately after line 1 (`# BepuFSharp`) and before the existing description line. The section must contain: "Created using [speckit](https://github.com/github/spec-kit)" (FR-002), "Claude Code with Opus 4.6", a link to the [project constitution](.specify/memory/constitution.md) (FR-003), and a link to the [documentation skills](.specify/memory/constitution.md#vi-comprehensive-documentation) (FR-004). Use italicized format per data-model.md attribution structure.

**Checkpoint**: README.md attribution section present with all required links. US2 acceptance scenarios 1-3 satisfied.

---

## Phase 5: User Story 3 - ECS-Neutral API Naming (Priority: P2)

**Goal**: Replace ECS-specific terminology across all remaining public-facing text: .fsi doc comments, literate documentation scripts, tutorial chapters, and example scripts. Apply the terminology map from data-model.md and the rules from contracts/terminology-contract.md.

**Independent Test**: Run `grep -rn "ECS" BepuFSharp/*.fsi docs/ scripts/examples/` and verify only comparative/educational instances remain (glossary definition, "What Is ECS?" section).

- [x] T010 [P] [US3] In BepuFSharp/Types.fsi line 8, replace "bulk ECS operations" with "bulk game loop operations" in the namespace XML doc comment (terminology map T1)
- [x] T011 [P] [US3] In docs/index.fsx: replace "ECS sync" with "game loop sync" (line 21, terminology map T2); update link on line 60 from `[ECS Integration](ecs-integration.html)` to `[Game Loop Integration](game-loop-integration.html)` (terminology map T5 + file rename cross-ref)
- [x] T012 [US3] In docs/game-loop-integration.fsx (renamed in T003): update YAML title from "ECS Integration" to "Game Loop Integration"; replace heading "# ECS Integration Patterns" with "# Game Loop Integration Patterns"; replace all 10 ECS prerequisite references with neutral equivalents per terminology map (T3, T5, T6, T7, T8, T9). Ensure the content describes sync patterns for any data-oriented engine, not just ECS. Note: this rename changes the public doc URL from `ecs-integration.html` to `game-loop-integration.html` — if the doc hosting supports redirects, add one; otherwise accept the URL change as intentional.
- [x] T013 [P] [US3] In docs/tutorial/09-raycasting.fsx line 262, update "Next" link text from "Bulk Operations and ECS Integration" to "Bulk Operations and Game Loop Integration" (keep the `10-bulk-operations.html` href unchanged)
- [x] T014 [US3] In docs/tutorial/10-bulk-operations.fsx: update YAML title from "Bulk Operations and ECS Integration" to "Bulk Operations and Game Loop Integration"; update H1 heading to match; replace ~12 ECS prerequisite references per terminology map (T3, T5, T10, T11). KEEP the "## What Is ECS?" educational section as-is (FR-006 clarifying context). Reframe surrounding prose to present ECS as one architecture option, not the assumed one.
- [x] T015 [P] [US3] In docs/tutorial/11-capstone.fsx line 333, replace "efficient ECS integration" with "efficient game loop integration" (terminology map T3)
- [x] T016 [US3] Verify docs/tutorial/12-glossary.md retains the ECS glossary entry unchanged (FR-006 — definitional/clarifying context). No edits needed; document verification as a comment in this task.
- [x] T017 [US3] In scripts/examples/03-bulk-game-loop-sync.fsx (renamed in T004): update all ECS references in comments and print statements to use neutral terminology per terminology map. Ensure the script remains runnable after changes.

**Checkpoint**: All public-facing text uses architecture-neutral terminology. Only comparative/educational ECS references remain (glossary, "What Is ECS?"). US3 acceptance scenarios 1-2 satisfied.

---

## Phase 6: Polish & Validation

**Purpose**: Final validation across all changes. Confirm no regressions, doc site builds, and renamed scripts run.

- [x] T018 Run terminology compliance check per contracts/terminology-contract.md: `grep -rn "ECS" README.md BepuFSharp/*.fsi docs/ scripts/examples/ | grep -v "What Is ECS" | grep -v "12-glossary.md"` — must return zero results (SC-001)
- [x] T019 Run `dotnet test` — confirm all existing tests pass with zero failures (SC-004)
- [x] T020 Run `dotnet fsdocs build` — confirm docs site builds successfully with no broken links from file renames (SC-002 link validation)
- [x] T021 Run `dotnet fsi scripts/examples/03-bulk-game-loop-sync.fsx` — confirm renamed script executes without errors (Constitution V: Scripting Accessibility)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — BLOCKS all user stories (files must be renamed before content edits)
- **US1 (Phase 3)**: Depends on Phase 2 — edits README.md
- **US2 (Phase 4)**: Depends on Phase 3 — appends to README.md after US1 establishes neutral terminology baseline
- **US3 (Phase 5)**: Depends on Phase 2 — edits renamed files. Can run in parallel with US1 (different files except README, which US3 does not touch)
- **Polish (Phase 6)**: Depends on Phases 3, 4, and 5 completion

### User Story Dependencies

- **US1 (P1)**: Depends on foundational file renames. Touches only README.md.
- **US2 (P2)**: Depends on US1 completing README changes (to avoid merge conflicts in README.md).
- **US3 (P2)**: Depends on foundational file renames. Touches .fsi, docs/, scripts/examples/ — no overlap with US1/US2 files (except README is NOT touched by US3).

### Parallel Opportunities

- T003 and T004 (file renames) can run in parallel
- US1 (Phase 3) and US3 (Phase 5) can run in parallel (different files)
- Within US3: T010, T011, T013, T015 can all run in parallel (different files)
- T018, T019, T020, T021 (validation) should run sequentially to catch cascading failures

---

## Parallel Example: User Story 3

```text
# These four tasks edit different files with no dependencies — launch in parallel:
T010: "Update BepuFSharp/Types.fsi doc comment"
T011: "Update docs/index.fsx ECS refs and link"
T013: "Update docs/tutorial/09-raycasting.fsx Next link text"
T015: "Update docs/tutorial/11-capstone.fsx one mention"

# Then sequentially (larger edits requiring more care):
T012: "Update docs/game-loop-integration.fsx — 10 replacements"
T014: "Update docs/tutorial/10-bulk-operations.fsx — 12 replacements, keep What Is ECS?"
T017: "Update scripts/examples/03-bulk-game-loop-sync.fsx content"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (baseline validation)
2. Complete Phase 2: File renames
3. Complete Phase 3: US1 — README terminology + bulk API validation
4. **STOP and VALIDATE**: grep README.md for ECS; run `dotnet test`
5. README is now ECS-neutral — primary public-facing document is correct

### Incremental Delivery

1. Setup + Foundational → Files renamed, baselines recorded
2. US1 → README ECS-neutral → Validate (MVP!)
3. US2 → Attribution section added → Validate links
4. US3 → All docs/tutorials/scripts ECS-neutral → Validate full sweep
5. Polish → Final grep check, test, doc build, script run

---

## Notes

- All tasks are documentation-only edits — no F# code or API changes
- The terminology map in `specs/003-data-oriented-wrapper/data-model.md` is the single source of truth for replacements
- The terminology contract in `specs/003-data-oriented-wrapper/contracts/terminology-contract.md` defines MUST NOT / MAY rules
- FR-006 explicitly permits ECS in comparative/educational contexts — do not over-remove
- File renames use `git mv` to preserve history
