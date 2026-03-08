# Tasks: API Safety & Missing Features

**Input**: Design documents from `/specs/004-api-safety-features/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Tests are included — the constitution (Principle III) mandates automated test evidence for all behavior-changing code.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Dependency pinning and project configuration (FR-011, User Story 6)

- [X] T001 Pin BepuPhysics dependency to exact version `[2.4.0]` in BepuFSharp/BepuFSharp.fsproj
- [X] T002 Pin BepuUtilities dependency to exact version `[2.4.0]` in BepuFSharp/BepuFSharp.fsproj
- [X] T003 Verify build succeeds with pinned versions: `dotnet build`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Existence check functions that all subsequent user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Add `bodyExists` signature to BepuFSharp/PhysicsWorld.fsi with XML doc comment
- [X] T005 [P] Add `staticExists` signature to BepuFSharp/PhysicsWorld.fsi with XML doc comment
- [X] T006 Implement `bodyExists` using `sim.Bodies.BodyExists(handle)` in BepuFSharp/PhysicsWorld.fs
- [X] T007 [P] Implement `staticExists` using `sim.Statics.StaticExists(handle)` in BepuFSharp/PhysicsWorld.fs
- [X] T008 Add tests for `bodyExists` (active body returns true, removed body returns false) in BepuFSharp.Tests/BodyTests.fs
- [X] T009 [P] Add tests for `staticExists` (active static returns true, removed static returns false) in BepuFSharp.Tests/BodyTests.fs
- [X] T010 Verify build and tests pass: `dotnet build && dotnet test`

**Checkpoint**: `bodyExists`/`staticExists` are available — user story implementation can begin

---

## Phase 3: User Story 1 - Safe Body Access After Removal (Priority: P1) MVP

**Goal**: Provide `tryGetBodyPose`, `tryGetBodyVelocity`, `trySetBodyPose`, `trySetBodyVelocity` that return safe results for removed bodies instead of crashing.

**Independent Test**: Create a body, remove it, call each try* function — verify no crash and correct return value.

### Implementation for User Story 1

- [X] T011 [P] [US1] Add `tryGetBodyPose` and `tryGetBodyVelocity` signatures to BepuFSharp/PhysicsWorld.fsi with XML doc comments
- [X] T012 [P] [US1] Add `trySetBodyPose` and `trySetBodyVelocity` signatures to BepuFSharp/PhysicsWorld.fsi with XML doc comments
- [X] T013 [US1] Implement `tryGetBodyPose` (gate on `bodyExists`, return `Pose voption`) in BepuFSharp/PhysicsWorld.fs
- [X] T014 [P] [US1] Implement `tryGetBodyVelocity` (gate on `bodyExists`, return `Velocity voption`) in BepuFSharp/PhysicsWorld.fs
- [X] T015 [US1] Implement `trySetBodyPose` (gate on `bodyExists`, return `bool`) in BepuFSharp/PhysicsWorld.fs
- [X] T016 [P] [US1] Implement `trySetBodyVelocity` (gate on `bodyExists`, return `bool`) in BepuFSharp/PhysicsWorld.fs
- [X] T017 [US1] Add tests: `tryGetBodyPose` returns `ValueSome` for active body and `ValueNone` for removed body in BepuFSharp.Tests/BodyTests.fs
- [X] T018 [P] [US1] Add tests: `tryGetBodyVelocity` returns `ValueSome`/`ValueNone` for active/removed body in BepuFSharp.Tests/BodyTests.fs
- [X] T019 [P] [US1] Add tests: `trySetBodyPose` returns `true`/`false` for active/removed body in BepuFSharp.Tests/BodyTests.fs
- [X] T020 [P] [US1] Add tests: `trySetBodyVelocity` returns `true`/`false` for active/removed body in BepuFSharp.Tests/BodyTests.fs
- [X] T021 [US1] Verify all US1 tests pass: `dotnet test --filter "tryGet|trySet"`

**Checkpoint**: Safe body access is functional — removed bodies no longer crash the process

---

## Phase 4: User Story 2 - Apply Forces and Impulses (Priority: P1)

**Goal**: Provide `applyImpulse`, `applyLinearImpulse`, `applyAngularImpulse` functions for dynamic bodies.

**Independent Test**: Create a body at rest, apply an impulse, step the simulation, verify the body moved.

### Implementation for User Story 2

- [X] T022 [P] [US2] Add `applyImpulse`, `applyLinearImpulse`, `applyAngularImpulse` signatures to BepuFSharp/PhysicsWorld.fsi with XML doc comments
- [X] T023 [US2] Implement `applyImpulse` (gate on `bodyExists`, call `bodyRef.ApplyImpulse`) in BepuFSharp/PhysicsWorld.fs
- [X] T024 [P] [US2] Implement `applyLinearImpulse` (gate on `bodyExists`, call `bodyRef.ApplyLinearImpulse`) in BepuFSharp/PhysicsWorld.fs
- [X] T025 [P] [US2] Implement `applyAngularImpulse` (gate on `bodyExists`, call `bodyRef.ApplyAngularImpulse`) in BepuFSharp/PhysicsWorld.fs
- [X] T026 [US2] Create BepuFSharp.Tests/ImpulseTests.fs and add to BepuFSharp.Tests/BepuFSharp.Tests.fsproj compile list
- [X] T027 [US2] Add test: `applyImpulse` with offset produces linear + angular velocity change in BepuFSharp.Tests/ImpulseTests.fs
- [X] T028 [P] [US2] Add test: `applyLinearImpulse` produces linear velocity change only in BepuFSharp.Tests/ImpulseTests.fs
- [X] T029 [P] [US2] Add test: `applyAngularImpulse` produces angular velocity change only in BepuFSharp.Tests/ImpulseTests.fs
- [X] T030 [US2] Add test: impulse on removed body does not crash (no-op) in BepuFSharp.Tests/ImpulseTests.fs
- [X] T031 [US2] Verify all US2 tests pass: `dotnet test --filter "Impulse"`

**Checkpoint**: Impulse application works — consumers can apply forces without escape hatch

---

## Phase 5: User Story 3 - Enumerate All Active Bodies and Statics (Priority: P2)

**Goal**: Provide `getAllBodyIds` and `getAllStaticIds` to discover active entities without maintaining external tracking.

**Independent Test**: Add bodies, remove some, call enumeration — verify exactly the active IDs are returned.

### Implementation for User Story 3

- [X] T032 [P] [US3] Add `getAllBodyIds` and `getAllStaticIds` signatures to BepuFSharp/PhysicsWorld.fsi with XML doc comments
- [X] T033 [US3] Implement `getAllBodyIds` (iterate active + sleeping body sets, collect handles via `IndexToHandle`) in BepuFSharp/PhysicsWorld.fs
- [X] T034 [US3] Implement `getAllStaticIds` (iterate statics, collect handles) in BepuFSharp/PhysicsWorld.fs
- [X] T035 [US3] Add test: `getAllBodyIds` returns correct count after adds and removes in BepuFSharp.Tests/WorldTests.fs
- [X] T036 [P] [US3] Add test: `getAllStaticIds` returns correct count after adds and removes in BepuFSharp.Tests/WorldTests.fs
- [X] T037 [P] [US3] Add test: `getAllBodyIds` returns empty array for empty world in BepuFSharp.Tests/WorldTests.fs
- [X] T038 [US3] Verify all US3 tests pass: `dotnet test --filter "getAll"`

**Checkpoint**: Body/static enumeration works — consumers can discover all active entities

---

## Phase 6: User Story 4 - Runtime Gravity Modification (Priority: P2)

**Goal**: Provide `setGravity` and `getGravity` for runtime gravity changes without escape hatch or reflection.

**Independent Test**: Create world with default gravity, change to zero, step a body — verify no downward acceleration.

### Implementation for User Story 4

- [X] T039 [US4] Add gravity getter/setter access to PhysicsWorld — store typed PoseIntegrator reference at construction in BepuFSharp/PhysicsWorld.fs (may require exposing Gravity field in BepuFSharp/Callbacks.fs)
- [X] T040 [P] [US4] Add `setGravity` and `getGravity` signatures to BepuFSharp/PhysicsWorld.fsi with XML doc comments
- [X] T041 [US4] Implement `setGravity` (write to PoseIntegrator.Callbacks.Gravity) in BepuFSharp/PhysicsWorld.fs
- [X] T042 [US4] Implement `getGravity` (read from PoseIntegrator.Callbacks.Gravity) in BepuFSharp/PhysicsWorld.fs
- [X] T043 [US4] Add test: `setGravity` to zero prevents downward acceleration in BepuFSharp.Tests/WorldTests.fs
- [X] T044 [P] [US4] Add test: `getGravity` returns default gravity on fresh world in BepuFSharp.Tests/WorldTests.fs
- [X] T045 [P] [US4] Add test: reversed gravity causes upward motion in BepuFSharp.Tests/WorldTests.fs
- [X] T046 [US4] Verify all US4 tests pass: `dotnet test --filter "ravity"`

**Checkpoint**: Runtime gravity modification works — consumers can change gravity without reflection

---

## Phase 7: User Story 5 - Query Body Shape (Priority: P3)

**Goal**: Provide `getBodyShape` to determine what shape is associated with a body.

**Independent Test**: Create bodies with different shapes, query each — verify correct shape is returned.

### Implementation for User Story 5

- [X] T047 [US5] Add `getBodyShape` signature to BepuFSharp/PhysicsWorld.fsi with XML doc comment
- [X] T048 [US5] Implement `getBodyShape` (read body's ShapeIndex, reconstruct PhysicsShape DU from typed shape data) in BepuFSharp/PhysicsWorld.fs
- [X] T049 [US5] Add test: `getBodyShape` returns `Some (Sphere ...)` for sphere body in BepuFSharp.Tests/ShapeTests.fs
- [X] T050 [P] [US5] Add test: `getBodyShape` returns `Some (Box ...)` for box body in BepuFSharp.Tests/ShapeTests.fs
- [X] T051 [P] [US5] Add test: `getBodyShape` returns `None` for removed body in BepuFSharp.Tests/ShapeTests.fs
- [X] T052 [US5] Verify all US5 tests pass: `dotnet test --filter "getBodyShape"`

**Checkpoint**: Shape query works — consumers can determine body shapes for visualization

---

## Phase 8: User Story 7 - Shape Description for Debugging (Priority: P3)

**Goal**: Provide `PhysicsShape.describe` for human-readable shape descriptions.

**Independent Test**: Create each shape type, call describe — verify readable parameterized strings.

### Implementation for User Story 7

- [X] T053 [US7] Add `PhysicsShape` module with `describe` signature to BepuFSharp/Shapes.fsi with XML doc comment
- [X] T054 [US7] Implement `PhysicsShape.describe` (pattern match on all 8 shape cases, format as `"Sphere(r=0.5)"` etc.) in BepuFSharp/Shapes.fs
- [X] T055 [US7] Add tests: `describe` returns correct strings for Sphere, Box, Capsule, Cylinder in BepuFSharp.Tests/ShapeTests.fs
- [X] T056 [P] [US7] Add tests: `describe` returns correct strings for Triangle, ConvexHull, Compound, Mesh in BepuFSharp.Tests/ShapeTests.fs
- [X] T057 [US7] Verify all US7 tests pass: `dotnet test --filter "describe"`

**Checkpoint**: Shape descriptions work — consumers get readable output for debugging

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Baselines, documentation, example scripts, and final validation

- [X] T058 Update BepuFSharp.Tests/baselines/PhysicsWorld.baseline to match updated PhysicsWorld.fsi
- [X] T059 [P] Update BepuFSharp.Tests/baselines/Shapes.baseline to match updated Shapes.fsi
- [X] T060 [P] Create scripts/examples/09-impulse-and-gravity.fsx demonstrating impulse application, gravity modification, body enumeration, and existence checks
- [X] T061 Verify surface area tests pass with updated baselines: `dotnet test --filter "SurfaceArea"`
- [X] T062 Verify all tests pass: `dotnet test`
- [X] T063 Verify package builds: `dotnet pack`
- [X] T064 [P] Verify example script runs: `dotnet fsi scripts/examples/09-impulse-and-gravity.fsx`
- [X] T065 Verify docs build: `dotnet fsdocs build --properties Configuration=Release`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 — BLOCKS all user stories
- **US1 Safe Access (Phase 3)**: Depends on Phase 2 (`bodyExists`)
- **US2 Impulse (Phase 4)**: Depends on Phase 2 (`bodyExists`) — can run in parallel with US1
- **US3 Enumeration (Phase 5)**: Depends on Phase 2 — can run in parallel with US1/US2
- **US4 Gravity (Phase 6)**: Depends on Phase 2 — can run in parallel with US1/US2/US3
- **US5 Shape Query (Phase 7)**: Depends on Phase 2 — can run in parallel with others
- **US7 Shape Describe (Phase 8)**: No dependencies on other stories — can run in parallel with all
- **Polish (Phase 9)**: Depends on ALL user story phases completing

### User Story Dependencies

- **US1 (P1)**: Depends on `bodyExists` from Phase 2. No cross-story dependencies.
- **US2 (P1)**: Depends on `bodyExists` from Phase 2. No cross-story dependencies.
- **US3 (P2)**: Depends on Phase 2 only. No cross-story dependencies.
- **US4 (P2)**: Depends on Phase 2 only. Requires PhysicsWorld constructor change (T039).
- **US5 (P3)**: Depends on Phase 2 only. No cross-story dependencies.
- **US7 (P3)**: No dependencies — only modifies Shapes.fsi/Shapes.fs.

### Within Each User Story

- Signatures (.fsi) before implementations (.fs)
- Implementations before tests
- All tests pass before checkpoint

### Parallel Opportunities

- T004/T005 (bodyExists/staticExists signatures) can run in parallel
- T006/T007 (bodyExists/staticExists implementations) can run in parallel
- T008/T009 (existence check tests) can run in parallel
- All US1-US7 user story phases can run in parallel after Phase 2
- Within each story, tasks marked [P] can run in parallel

---

## Parallel Example: User Story 1

```text
# Signatures in parallel:
T011: Add tryGetBodyPose/tryGetBodyVelocity signatures to PhysicsWorld.fsi
T012: Add trySetBodyPose/trySetBodyVelocity signatures to PhysicsWorld.fsi

# Implementations in parallel (after signatures):
T013 + T014: tryGetBodyPose + tryGetBodyVelocity
T015 + T016: trySetBodyPose + trySetBodyVelocity

# Tests in parallel (after implementations):
T017 + T018 + T019 + T020: All four try* test groups
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Dependency pinning (T001-T003)
2. Complete Phase 2: bodyExists/staticExists (T004-T010)
3. Complete Phase 3: Safe body access — try* variants (T011-T021)
4. **STOP and VALIDATE**: Test that removed bodies no longer crash
5. This alone resolves the highest-severity bugs (crash on stale BodyId)

### Incremental Delivery

1. Setup + Foundational → Dependency safety + existence checks
2. Add US1 (safe access) → Crash bugs fixed (MVP!)
3. Add US2 (impulse) → Interactive physics enabled
4. Add US3 (enumeration) + US4 (gravity) → Visualization/tooling support
5. Add US5 (shape query) + US7 (describe) → Developer experience improvements
6. Polish → Baselines, docs, example scripts

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- User Story 6 (dependency pinning) is handled in Phase 1 as it's a project config change, not code
- All new `.fsi` signatures must include `///` XML doc comments per Constitution VI
- Surface area baselines must be updated AFTER all API changes are complete (Phase 9)
- The `getBodyShape` implementation for ConvexHull/Compound/Mesh may return simplified representations — document this limitation in the XML doc comment
