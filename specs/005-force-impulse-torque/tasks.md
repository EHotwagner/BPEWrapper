# Tasks: Force, Impulse, and Torque Application

**Input**: Design documents from `/specs/005-force-impulse-torque/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Required by Constitution Principle III (Test Evidence Is Mandatory).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create test file and register it in the test project

- [x] T001 Create `BepuFSharp.Tests/ForceTests.fs` with module declaration and empty test list skeleton using Expecto
- [x] T002 Add `ForceTests.fs` to `<Compile Include>` list in `BepuFSharp.Tests/BepuFSharp.Tests.fsproj` (before `Program.fs`)

**Checkpoint**: Project compiles with empty ForceTests module

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No foundational/blocking tasks needed — all user stories add functions to existing files using existing types. No new modules, types, or infrastructure required.

**Checkpoint**: Foundation ready — user story implementation can begin

---

## Phase 3: User Story 1 - Apply Instantaneous Impulse (Priority: P1) 🎯 MVP

**Goal**: Developer can apply a linear impulse to a dynamic body and observe correct velocity change in one function call. Includes single-body and bulk variants.

**Independent Test**: Create a body at rest with mass=2, apply impulse=(10,0,0), verify velocity=(5,0,0). Verify kinematic bodies are unaffected. Verify sleeping bodies wake.

### Tests for User Story 1

- [x] T003 [P] [US1] Write test "applyLinearImpulse changes velocity by impulse/mass" in `BepuFSharp.Tests/ForceTests.fs` — create sphere body (mass=2), apply impulse=(10,0,0), read velocity, assert linear=(5,0,0)
- [x] T004 [P] [US1] Write test "applyLinearImpulse on kinematic body has no effect" in `BepuFSharp.Tests/ForceTests.fs` — create kinematic body, apply impulse, assert velocity unchanged
- [x] T005 [P] [US1] Write test "applyLinearImpulses bulk matches per-body results" in `BepuFSharp.Tests/ForceTests.fs` — create 3 bodies, apply impulses via bulk, verify each velocity

### Implementation for User Story 1

- [x] T006 [US1] Add `applyLinearImpulse` signature with XML doc comment to `BepuFSharp/PhysicsWorld.fsi`: `val applyLinearImpulse: bodyId: BodyId -> impulse: Vector3 -> PhysicsWorld -> unit`
- [x] T007 [US1] Implement `applyLinearImpulse` in `BepuFSharp/PhysicsWorld.fs` — ThrowIfDisposed, convert BodyId via Interop, get BodyReference, check Kinematic (return if true), call `sim.Awakener.AwakenBody(handle)`, call `bodyRef.ApplyLinearImpulse(impulse)`
- [x] T008 [US1] Add `applyLinearImpulses` signature with XML doc comment to `BepuFSharp/PhysicsWorld.fsi`: `val applyLinearImpulses: ids: BodyId[] -> impulses: Vector3[] -> PhysicsWorld -> unit`
- [x] T009 [US1] Implement `applyLinearImpulses` in `BepuFSharp/PhysicsWorld.fs` — ThrowIfDisposed, loop over parallel arrays, same per-body logic as `applyLinearImpulse`
- [x] T010 [US1] Verify all US1 tests pass by running `dotnet test BepuFSharp.Tests/`

**Checkpoint**: `applyLinearImpulse` and `applyLinearImpulses` work. MVP is functional.

---

## Phase 4: User Story 2 - Apply Continuous Force (Priority: P2)

**Goal**: Developer can apply a continuous force vector and a timestep duration, and the body accelerates correctly for one step.

**Independent Test**: Create body (mass=2), apply force=(10,0,0) with dt=0.5, verify velocity=(2.5,0,0). Verify force does not persist across steps.

### Tests for User Story 2

- [x] T011 [P] [US2] Write test "applyForce changes velocity by force*dt/mass" in `BepuFSharp.Tests/ForceTests.fs` — create sphere body (mass=2), apply force=(10,0,0) with dt=0.5, assert velocity=(2.5,0,0)
- [x] T012 [P] [US2] Write test "applyForce does not persist across steps" in `BepuFSharp.Tests/ForceTests.fs` — apply force once, step twice (without gravity), verify second step adds no further velocity from the force
- [x] T013 [P] [US2] Write test "applyForces bulk matches per-body results" in `BepuFSharp.Tests/ForceTests.fs` — create 3 bodies, apply forces via bulk, verify each velocity

### Implementation for User Story 2

- [x] T014 [US2] Add `applyForce` signature with XML doc comment to `BepuFSharp/PhysicsWorld.fsi`: `val applyForce: bodyId: BodyId -> force: Vector3 -> dt: float32 -> PhysicsWorld -> unit`
- [x] T015 [US2] Implement `applyForce` in `BepuFSharp/PhysicsWorld.fs` — same pattern as `applyLinearImpulse` but calls `bodyRef.ApplyLinearImpulse(force * dt)`
- [x] T016 [US2] Add `applyForces` signature with XML doc comment to `BepuFSharp/PhysicsWorld.fsi`: `val applyForces: ids: BodyId[] -> forces: Vector3[] -> dt: float32 -> PhysicsWorld -> unit`
- [x] T017 [US2] Implement `applyForces` in `BepuFSharp/PhysicsWorld.fs` — bulk loop version of `applyForce`
- [x] T018 [US2] Verify all US2 tests pass by running `dotnet test BepuFSharp.Tests/`

**Checkpoint**: `applyForce` and `applyForces` work. Forces correctly convert to impulses via dt scaling.

---

## Phase 5: User Story 3 - Apply Torque and Angular Impulse (Priority: P2)

**Goal**: Developer can apply angular impulse or continuous torque to spin a body, with correct rotational behavior based on inertia.

**Independent Test**: Create sphere body (mass=1, radius=1), apply angular impulse, verify angular velocity changes according to sphere's inverse inertia tensor.

### Tests for User Story 3

- [x] T019 [P] [US3] Write test "applyAngularImpulse changes angular velocity" in `BepuFSharp.Tests/ForceTests.fs` — create sphere body, apply angular impulse, verify angular velocity is non-zero and directionally correct
- [x] T020 [P] [US3] Write test "applyTorque changes angular velocity by torque*dt scaled by inertia" in `BepuFSharp.Tests/ForceTests.fs` — create sphere body, apply torque with known dt, verify angular velocity
- [x] T021 [P] [US3] Write test "applyAngularImpulses bulk matches per-body results" in `BepuFSharp.Tests/ForceTests.fs` — create 3 bodies, apply angular impulses via bulk, verify each angular velocity

### Implementation for User Story 3

- [x] T022 [US3] Add `applyAngularImpulse` signature with XML doc comment to `BepuFSharp/PhysicsWorld.fsi`: `val applyAngularImpulse: bodyId: BodyId -> angularImpulse: Vector3 -> PhysicsWorld -> unit`
- [x] T023 [US3] Implement `applyAngularImpulse` in `BepuFSharp/PhysicsWorld.fs` — same pattern, calls `bodyRef.ApplyAngularImpulse(angularImpulse)`
- [x] T024 [US3] Add `applyTorque` signature with XML doc comment to `BepuFSharp/PhysicsWorld.fsi`: `val applyTorque: bodyId: BodyId -> torque: Vector3 -> dt: float32 -> PhysicsWorld -> unit`
- [x] T025 [US3] Implement `applyTorque` in `BepuFSharp/PhysicsWorld.fs` — calls `bodyRef.ApplyAngularImpulse(torque * dt)`
- [x] T026 [US3] Add `applyAngularImpulses` signature with XML doc comment to `BepuFSharp/PhysicsWorld.fsi`: `val applyAngularImpulses: ids: BodyId[] -> angularImpulses: Vector3[] -> PhysicsWorld -> unit`
- [x] T027 [US3] Implement `applyAngularImpulses` in `BepuFSharp/PhysicsWorld.fs` — bulk loop version
- [x] T028 [US3] Verify all US3 tests pass by running `dotnet test BepuFSharp.Tests/`

**Checkpoint**: All angular/torque functions work. Rotational effects are physically correct.

---

## Phase 6: User Story 4 - Apply Impulse at Point (Priority: P3)

**Goal**: Developer can apply an impulse at a world-space offset from a body's center of mass, producing both linear and angular velocity changes.

**Independent Test**: Create box body, apply impulse at center → only linear change. Apply impulse at edge → both linear and angular change.

### Tests for User Story 4

- [x] T029 [P] [US4] Write test "applyImpulseAtPoint at center produces only linear velocity" in `BepuFSharp.Tests/ForceTests.fs` — create body, apply impulse with offset=Vector3.Zero, verify angular velocity remains zero
- [x] T030 [P] [US4] Write test "applyImpulseAtPoint at offset produces linear and angular velocity" in `BepuFSharp.Tests/ForceTests.fs` — create body, apply impulse with non-zero offset, verify both linear and angular velocity change

### Implementation for User Story 4

- [x] T031 [US4] Add `applyImpulseAtPoint` signature with XML doc comment to `BepuFSharp/PhysicsWorld.fsi`: `val applyImpulseAtPoint: bodyId: BodyId -> impulse: Vector3 -> offset: Vector3 -> PhysicsWorld -> unit`
- [x] T032 [US4] Implement `applyImpulseAtPoint` in `BepuFSharp/PhysicsWorld.fs` — same pattern, calls `bodyRef.ApplyImpulse(impulse, offset)`
- [x] T033 [US4] Verify all US4 tests pass by running `dotnet test BepuFSharp.Tests/`

**Checkpoint**: All 8 public functions implemented and tested.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Surface area baseline, documentation, scripting accessibility

- [x] T034 Update `BepuFSharp.Tests/baselines/PhysicsWorld.baseline` to match the updated `BepuFSharp/PhysicsWorld.fsi` (copy full .fsi content to baseline)
- [x] T035 Verify surface area test passes by running `dotnet test BepuFSharp.Tests/ --filter SurfaceArea`
- [x] T036 Run full test suite `dotnet test BepuFSharp.Tests/` and verify all tests pass (existing + new)
- [x] T037 Verify `dotnet pack BepuFSharp/` succeeds with new API surface
- [x] T038 Create `scripts/examples/09-forces.fsx` demonstrating applyLinearImpulse, applyForce, applyAngularImpulse, and applyImpulseAtPoint with a simple scenario (e.g., launch a ball, apply wind force, spin a body, off-center hit)
- [x] T039 Run `doc-build` to verify documentation site builds successfully after all `.fsi` XML doc comment additions

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: N/A — no foundational tasks
- **US1 (Phase 3)**: Depends on Phase 1 completion
- **US2 (Phase 4)**: Depends on Phase 3 (same files — .fs/.fsi)
- **US3 (Phase 5)**: Depends on Phase 4 (same files — .fs/.fsi)
- **US4 (Phase 6)**: Depends on Phase 5 (same files — .fs/.fsi)
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

- **US1 (P1)**: Independent — first to implement
- **US2 (P2)**: Sequenced after US1 (same source files)
- **US3 (P2)**: Sequenced after US2 (same source files)
- **US4 (P3)**: Sequenced after US3 (same source files); builds on US1+US3 concepts

Note: While user stories are conceptually independent, they all modify the same two files (`PhysicsWorld.fs` and `PhysicsWorld.fsi`), so they must be implemented sequentially.

### Within Each User Story

1. Tests can be written in parallel with .fsi signature additions (different files)
2. .fsi signature must be added before .fs implementation (compiler contract)
3. Implementation follows signature
4. Test verification is last

### Parallel Opportunities

- Within each story: test writing [P] tasks can run alongside .fsi signature tasks
- T003, T004, T005 can all run in parallel (all write to ForceTests.fs but different test cases)
- T011, T012, T013 can all run in parallel
- T019, T020, T021 can all run in parallel
- T029, T030 can run in parallel

---

## Parallel Example: User Story 1

```text
# These can run in parallel (different files):
Task T003: Write impulse/mass test in ForceTests.fs
Task T006: Add applyLinearImpulse signature to PhysicsWorld.fsi

# Then sequentially:
Task T007: Implement applyLinearImpulse in PhysicsWorld.fs (needs T006)
Task T010: Run tests (needs T003, T007)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 3: User Story 1 (T003-T010)
3. **STOP and VALIDATE**: `applyLinearImpulse` works correctly
4. This alone delivers the highest-value capability (jumps, knockbacks, explosions)

### Incremental Delivery

1. US1 → Linear impulse works → MVP
2. US2 → Continuous forces work → Environmental effects enabled
3. US3 → Angular impulse/torque work → Full rotational control
4. US4 → Point impulse works → Realistic off-center hits
5. Polish → Baseline updated, all tests green, packable

### Task Summary

| Phase | Story | Tasks | Parallel |
|-------|-------|-------|----------|
| Setup | — | 2 | 0 |
| US1 (P1) | MVP | 8 | 3 tests |
| US2 (P2) | Force | 8 | 3 tests |
| US3 (P2) | Angular | 10 | 3 tests |
| US4 (P3) | Point | 5 | 2 tests |
| Polish | — | 6 | 0 |
| **Total** | | **39** | |

---

## Notes

- All 8 functions follow the identical internal pattern (see quickstart.md)
- No new types or modules — purely additive to PhysicsWorld
- Tests use zero-gravity config to isolate force/impulse effects from gravity
- Surface area baseline MUST be updated last (after all .fsi changes)
