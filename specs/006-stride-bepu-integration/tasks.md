# Tasks: Stride BepuPhysics Integration & Query Extensions

**Input**: Design documents from `/specs/006-stride-bepu-integration/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included — constitution Principle III (Test Evidence Is Mandatory) requires automated tests for all behavior-changing code.

**Organization**: Tasks grouped by user story in priority order. Stories 1 and 2 are both P1; Story 3 (P2) establishes the filter pattern reused by Stories 1 and 2 but each story creates independent handler structs, so no hard code dependency exists between them.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add new types, update project file, prepare shared infrastructure

- [x] T001 Add `SweepHit` and `OverlapResult` record types to BepuFSharp/Types.fsi per contracts/Types.fsi.patch
- [x] T002 Add `SweepHit` and `OverlapResult` record types to BepuFSharp/Types.fs matching the .fsi signatures
- [x] T003 Add Stride.BepuPhysics 4.3.0.2507 PackageReference to BepuFSharp/BepuFSharp.fsproj
- [x] T004 Add StrideInterop.fsi and StrideInterop.fs entries to BepuFSharp/BepuFSharp.fsproj compile list (after Callbacks.fs, before PhysicsWorld.fsi)
- [x] T005 Add StrideInteropTests.fs entry to BepuFSharp.Tests/BepuFSharp.Tests.fsproj compile list
- [x] T006 Verify `dotnet build BepuFSharp/BepuFSharp.fsproj` succeeds with new Stride.BepuPhysics dependency (compatibility check for beta.25 vs beta.28)

**Checkpoint**: Project builds with new types and Stride dependency. No behavioral changes yet.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Internal helpers reused across multiple user stories

**⚠️ CRITICAL**: Query handler filter logic and constraint type registry must be in place before story implementation.

- [x] T007 Add a `checkFilter` helper function to BepuFSharp/Queries.fs that performs the bidirectional collision mask check (replicating logic from Callbacks.fs line 50) given a `CollisionFilter` and a `CollidableReference` with access to the filter lookup tables
- [x] T008 Add internal `constraintTypeRegistry` (`Dictionary<int, int>`) field to PhysicsWorld internal state in BepuFSharp/PhysicsWorld.fs — populated at `addConstraint`, cleaned at `removeConstraint` and `removeBody` auto-removal path
- [x] T009 Expose internal mutation methods on `DefaultNarrowPhaseCallbacks` for filter and material table updates in BepuFSharp/Callbacks.fs (`updateFilter`, `updateMaterial` taking handle key and new value)

**Checkpoint**: Foundation ready — handler filter checks, constraint tracking, and callback mutation are available for story implementations.

---

## Phase 3: User Story 1 - Sweep Cast Queries (Priority: P1) 🎯 MVP

**Goal**: Developers can test whether a shape moving along a path would collide with anything, returning the first hit.

**Independent Test**: Create a world with known bodies, sweep-cast a sphere along a path, verify correct first hit body/position/normal/distance.

### Tests for User Story 1

- [x] T010 [P] [US1] Add sweep cast test: sphere sweep hits static box — verify contact point, normal, distance in BepuFSharp.Tests/QueryTests.fs
- [x] T011 [P] [US1] Add sweep cast test: sweep along empty path returns None in BepuFSharp.Tests/QueryTests.fs
- [x] T012 [P] [US1] Add sweep cast test: sweep past multiple bodies returns only closest hit in BepuFSharp.Tests/QueryTests.fs
- [x] T013 [P] [US1] Add sweep cast test: sweep with collision mask ignores non-matching layers in BepuFSharp.Tests/QueryTests.fs
- [x] T014 [P] [US1] Add sweep cast edge case test: degenerate shape (zero radius sphere) returns None in BepuFSharp.Tests/QueryTests.fs

### Implementation for User Story 1

- [x] T015 [US1] Create `SweepHitHandler` struct implementing `ISweepHitHandler` in BepuFSharp/Queries.fs — tracks closest hit, supports optional `CollisionFilter` in `AllowTest`
- [x] T016 [US1] Implement shape-type dispatch helper in BepuFSharp/Queries.fs that calls `Simulation.Sweep<TShape, TSweepHitHandler>` for each of the 8 `PhysicsShape` DU cases
- [x] T017 [US1] Add `sweepCast` signature to BepuFSharp/PhysicsWorld.fsi per contracts/PhysicsWorld.fsi.patch
- [x] T018 [US1] Implement `sweepCast` function in BepuFSharp/PhysicsWorld.fs — converts PhysicsShape to concrete BepuPhysics shape, creates SweepHitHandler, dispatches sweep, converts result to SweepHit option
- [x] T019 [US1] Run sweep cast tests and verify all pass: `dotnet test --filter "SweepCast"`

**Checkpoint**: Sweep cast queries work for all 8 shape types with optional collision filtering.

---

## Phase 4: User Story 2 - Overlap Queries (Priority: P1)

**Goal**: Developers can find all bodies overlapping a given volume without modifying the simulation.

**Independent Test**: Create a world with 5 bodies, overlap-test with a large sphere, verify exactly the 3 intersecting bodies are returned.

### Tests for User Story 2

- [x] T020 [P] [US2] Add overlap test: 5 bodies, 3 inside sphere — verify exactly 3 returned in BepuFSharp.Tests/QueryTests.fs
- [x] T021 [P] [US2] Add overlap test: no bodies inside volume returns empty array in BepuFSharp.Tests/QueryTests.fs
- [x] T022 [P] [US2] Add overlap test: overlap with collision mask excludes non-matching layers in BepuFSharp.Tests/QueryTests.fs
- [x] T023 [P] [US2] Add overlap edge case test: zero-volume shape returns empty array in BepuFSharp.Tests/QueryTests.fs

### Implementation for User Story 2

- [x] T024 [US2] Create `OverlapEnumerator` struct implementing `IBreakableForEach<int>` in BepuFSharp/Queries.fs — collects CollidableReferences from broad-phase, supports optional `CollisionFilter` filtering
- [x] T025 [US2] Implement narrow-phase confirmation step in BepuFSharp/Queries.fs — for each broad-phase candidate, perform shape-vs-shape contact test to confirm true overlap
- [x] T026 [US2] Add `overlap` signature to BepuFSharp/PhysicsWorld.fsi per contracts/PhysicsWorld.fsi.patch
- [x] T027 [US2] Implement `overlap` function in BepuFSharp/PhysicsWorld.fs — computes bounding box for query shape, calls BroadPhase.GetOverlaps, runs narrow confirm, converts results to OverlapResult[]
- [x] T028 [US2] Run overlap tests and verify all pass: `dotnet test --filter "Overlap"`

**Checkpoint**: Overlap queries correctly identify all intersecting bodies with optional collision filtering.

---

## Phase 5: User Story 3 - Filtered Raycasting (Priority: P2)

**Goal**: Existing raycasts respect collision layer filtering via an optional mask parameter, with full backward compatibility.

**Independent Test**: Create bodies on different collision layers, cast ray with specific mask, verify only matching bodies are hit.

### Tests for User Story 3

- [x] T029 [P] [US3] Add filtered raycast test: bodies on layers 0 and 1, mask layer 0 only — verify only layer-0 hit in BepuFSharp.Tests/QueryTests.fs
- [x] T030 [P] [US3] Add filtered raycast test: no filter (default) hits all bodies — backward compatibility in BepuFSharp.Tests/QueryTests.fs
- [x] T031 [P] [US3] Add filtered raycastAll test: mask excludes specific layers from multi-hit results in BepuFSharp.Tests/QueryTests.fs

### Implementation for User Story 3

- [x] T032 [US3] Add optional `CollisionFilter voption` field to `SingleHitHandler` struct in BepuFSharp/Queries.fs — modify `AllowTest` to check filter when ValueSome, return true when ValueNone
- [x] T033 [US3] Add optional `CollisionFilter voption` field to `MultiHitHandler` struct in BepuFSharp/Queries.fs — same AllowTest logic as SingleHitHandler
- [x] T034 [US3] Update `raycast` signature in BepuFSharp/PhysicsWorld.fsi to add `?filter: CollisionFilter` optional parameter
- [x] T035 [US3] Update `raycastAll` signature in BepuFSharp/PhysicsWorld.fsi to add `?filter: CollisionFilter` optional parameter
- [x] T036 [US3] Update `raycast` and `raycastAll` implementations in BepuFSharp/PhysicsWorld.fs to pass filter to handlers
- [x] T037 [US3] Run ALL existing raycast tests to verify backward compatibility: `dotnet test --filter "Raycast"`

**Checkpoint**: Filtered raycasts work; all pre-existing raycast tests still pass unchanged.

---

## Phase 6: User Story 4 - Constraint Readback (Priority: P2)

**Goal**: Developers can retrieve constraint parameters and connected bodies for serialization and debugging.

**Independent Test**: Create a constraint, read back its description, verify parameters match creation values.

### Tests for User Story 4

- [x] T038 [P] [US4] Add constraint readback test: create Hinge, read back description, verify axis/offsets/spring match in BepuFSharp.Tests/ConstraintTests.fs
- [x] T039 [P] [US4] Add constraintExists test: true for active constraint, false after removal in BepuFSharp.Tests/ConstraintTests.fs
- [x] T040 [P] [US4] Add getConstraintBodies test: returns correct two BodyIds in BepuFSharp.Tests/ConstraintTests.fs
- [x] T041 [P] [US4] Add readback test for all 10 constraint types (BallSocket, Hinge, Weld, DistanceLimit, DistanceSpring, SwingLimit, TwistLimit, LinearAxisMotor, AngularMotor, PointOnLine) in BepuFSharp.Tests/ConstraintTests.fs
- [x] T042 [P] [US4] Add edge case test: readback after constraint removal returns None in BepuFSharp.Tests/ConstraintTests.fs

### Implementation for User Story 4

- [x] T043 [US4] Implement constraint type tag storage in `addConstraint` in BepuFSharp/PhysicsWorld.fs — extract DU case tag from ConstraintDesc and store in constraintTypeRegistry dictionary
- [x] T044 [US4] Implement constraint type tag cleanup in `removeConstraint` and `removeBody` auto-removal path in BepuFSharp/PhysicsWorld.fs
- [x] T045 [US4] Implement `getConstraintDescription` — dispatch on stored type tag to call correct `Solver.GetDescription<T>` for each of 10 constraint types, convert back to ConstraintDesc in BepuFSharp/PhysicsWorld.fs
- [x] T046 [US4] Add `getConstraintDescription`, `constraintExists`, `getConstraintBodies` signatures to BepuFSharp/PhysicsWorld.fsi
- [x] T047 [US4] Implement `constraintExists` (dictionary lookup) and `getConstraintBodies` (solver body reference extraction) in BepuFSharp/PhysicsWorld.fs
- [x] T048 [US4] Add Interop helpers `motorSettingsToBepu` and `bepuToMotorSettings` in BepuFSharp/Interop.fs for MotorSettings ↔ BepuPhysics conversion (needed by constraint readback)
- [x] T049 [US4] Run constraint readback tests: `dotnet test --filter "Constraint"`

**Checkpoint**: All 10 constraint types can be read back with matching parameters; existence and body queries work.

---

## Phase 7: User Story 5 - Runtime Filter and Material Modification (Priority: P3)

**Goal**: Developers can change collision filters and material properties at runtime without removing/re-adding bodies.

**Independent Test**: Create a body, change its collision mask, verify it no longer collides with excluded layers on next step.

### Tests for User Story 5

- [x] T050 [P] [US5] Add filter modification test: body on layer 0, change mask to exclude layer 0, verify pass-through on next step in BepuFSharp.Tests/BodyTests.fs
- [x] T051 [P] [US5] Add material modification test: change friction to zero, verify sliding behavior in BepuFSharp.Tests/BodyTests.fs
- [x] T052 [P] [US5] Add static filter modification test: change static body's filter, verify takes effect in BepuFSharp.Tests/BodyTests.fs
- [x] T053 [P] [US5] Add edge case test: setCollisionFilter on removed body raises InvalidBodyHandle in BepuFSharp.Tests/BodyTests.fs

### Implementation for User Story 5

- [x] T054 [US5] Add `setCollisionFilter`, `setStaticCollisionFilter`, `setMaterial`, `setStaticMaterial` signatures to BepuFSharp/PhysicsWorld.fsi per contracts/PhysicsWorld.fsi.patch
- [x] T055 [US5] Implement `setCollisionFilter` in BepuFSharp/PhysicsWorld.fs — validate body exists, call Callbacks.updateFilter with body handle key
- [x] T056 [US5] Implement `setStaticCollisionFilter` in BepuFSharp/PhysicsWorld.fs — validate static exists, call Callbacks.updateFilter with static handle key (offset-encoded)
- [x] T057 [US5] Implement `setMaterial` and `setStaticMaterial` in BepuFSharp/PhysicsWorld.fs — validate handle, call Callbacks.updateMaterial
- [x] T058 [US5] Run runtime modification tests: `dotnet test --filter "Filter|Material"`

**Checkpoint**: Collision filters and materials can be changed at runtime; changes take effect on next simulation step.

---

## Phase 8: User Story 6 - Stride.BepuPhysics Type Interop (Priority: P3)

**Goal**: Bidirectional conversion between BepuFSharp types and Stride.BepuPhysics types for seamless Stride3D integration.

**Independent Test**: Round-trip each type (BepuFSharp → Stride → BepuFSharp) and verify equivalence.

### Tests for User Story 6

- [x] T059 [P] [US6] Add shape round-trip tests for 5 supported shape types (Sphere, Box, Capsule, Cylinder, Triangle) plus unsupported-shape error tests for ConvexHull/Compound/Mesh in BepuFSharp.Tests/StrideInteropTests.fs
- [x] T060 [P] [US6] Add CollisionFilter ↔ CollisionLayer round-trip test in BepuFSharp.Tests/StrideInteropTests.fs
- [x] T061 Deferred: MaterialProperties has no standalone Stride equivalent (properties live on CollidableComponent)
- [x] T062 Deferred: ConstraintDesc conversions require Stride ECS constraint components not available standalone

### Implementation for User Story 6

- [x] T063 [US6] Create BepuFSharp/StrideInterop.fsi with actual Stride.BepuPhysics type paths (ColliderBase, CollisionLayer)
- [x] T064 [US6] Implement shape conversions (`toStrideCollider`, `fromStrideCollider`) in BepuFSharp/StrideInterop.fs — 5 convex types supported, ConvexHull/Compound/Mesh raise NotSupportedException
- [x] T065 [US6] Implement CollisionFilter ↔ CollisionLayer conversions in BepuFSharp/StrideInterop.fs
- [x] T066 Deferred: MaterialProperties/ConstraintDesc live on Stride ECS components, not standalone types
- [x] T067 Deferred: See T066
- [x] T068 [US6] Run Stride interop tests: `dotnet test --filter "StrideInterop"` — 7 tests pass

**Checkpoint**: All types round-trip through BepuFSharp ↔ Stride conversion without data loss.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Surface-area baselines, documentation, scripting, version bump, final validation

- [x] T069 [P] Update surface-area baseline tests for new public API surface in BepuFSharp.Tests/SurfaceAreaTests.fs
- [ ] T070 [P] Create example script scripts/examples/10-sweep-casts.fsx demonstrating sweep cast API usage
- [ ] T071 [P] Create example script scripts/examples/11-overlap-queries.fsx demonstrating overlap query API usage
- [ ] T072 [P] Create example script scripts/examples/12-constraint-readback.fsx demonstrating constraint readback API usage
- [ ] T073 [P] Run `api-doc` skill to review and update XML doc comments in BepuFSharp/Types.fsi, BepuFSharp/PhysicsWorld.fsi, and BepuFSharp/StrideInterop.fsi for all new public symbols
- [ ] T074 Run `doc-build` skill to verify documentation site builds successfully after API surface changes
- [x] T075 Update scripts/prelude.fsx to reference BepuPhysics 2.5.0-beta.28 (currently references 2.4.0)
- [x] T076 Bump package version to 0.2.0-beta.1 in Directory.Build.props (prerelease due to beta dependencies)
- [x] T077 Run full test suite: 100 tests, 100 passed, 0 failed (SC-007 backward compatibility verified)
- [x] T078 Pack and publish to local NuGet store: BepuFSharp.0.2.0-beta.1.nupkg at ~/.local/share/nuget-local/

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (T001-T002 for types, T003 for Stride dep)
- **User Stories (Phase 3-8)**: All depend on Phase 2 completion
  - US1 (Sweep Cast), US2 (Overlap), US3 (Filtered Ray) can proceed in parallel
  - US4 (Constraint Readback) can proceed in parallel with US1-US3
  - US5 (Runtime Modification) can proceed in parallel with US1-US4
  - US6 (Stride Interop) should start last (benefits from all types being finalized)
- **Polish (Phase 9)**: Depends on all story phases being complete

### User Story Dependencies

- **US1 (Sweep Cast)**: Depends on Phase 2 only — independent of other stories
- **US2 (Overlap)**: Depends on Phase 2 only — independent of other stories
- **US3 (Filtered Raycasting)**: Depends on Phase 2 only — independent of other stories
- **US4 (Constraint Readback)**: Depends on Phase 2 (T008 constraint registry) — independent of other stories
- **US5 (Runtime Modification)**: Depends on Phase 2 (T009 callback mutation) — independent of other stories
- **US6 (Stride Interop)**: Depends on Phase 1 (T003, T004 Stride package) — should run after US1-US5 to ensure types are stable

### Within Each User Story

- Tests written FIRST, verified to FAIL before implementation
- Handler/struct implementation before PhysicsWorld integration
- .fsi signature before .fs implementation
- Run story-specific tests at end of story phase

### Parallel Opportunities

- T001 + T003 + T004 + T005 can run in parallel (different files)
- T007 + T008 + T009 can run in parallel (different files: Queries.fs, PhysicsWorld.fs, Callbacks.fs)
- All test tasks within a story marked [P] can run in parallel
- US1, US2, US3, US4, US5 can all proceed in parallel after Phase 2
- T069 + T070 + T071 + T072 + T073 can run in parallel (different files)

---

## Parallel Example: User Story 1

```bash
# Launch all tests for US1 together:
Task: "T010 Sweep cast test: sphere hits static box in BepuFSharp.Tests/QueryTests.fs"
Task: "T011 Sweep cast test: empty path returns None in BepuFSharp.Tests/QueryTests.fs"
Task: "T012 Sweep cast test: multiple bodies returns closest in BepuFSharp.Tests/QueryTests.fs"
Task: "T013 Sweep cast test: collision mask filtering in BepuFSharp.Tests/QueryTests.fs"
Task: "T014 Sweep cast test: degenerate shape returns None in BepuFSharp.Tests/QueryTests.fs"
```

## Parallel Example: User Story 6

```bash
# Launch all tests for US6 together:
Task: "T059 Shape round-trip tests in BepuFSharp.Tests/StrideInteropTests.fs"
Task: "T060 CollisionFilter round-trip test in BepuFSharp.Tests/StrideInteropTests.fs"
Task: "T061 MaterialProperties round-trip test in BepuFSharp.Tests/StrideInteropTests.fs"
Task: "T062 ConstraintDesc round-trip tests in BepuFSharp.Tests/StrideInteropTests.fs"
```

---

## Implementation Strategy

### MVP First (User Story 1 — Sweep Cast)

1. Complete Phase 1: Setup (T001-T006)
2. Complete Phase 2: Foundational (T007-T009)
3. Complete Phase 3: User Story 1 — Sweep Cast (T010-T019)
4. **STOP and VALIDATE**: Run sweep cast tests independently
5. Package delivers P1 sweep cast capability to upstream PhysicsSandbox

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 (Sweep Cast) → Test → Delivers P1 capability (MVP!)
3. Add US2 (Overlap) → Test → Delivers second P1 capability
4. Add US3 (Filtered Ray) → Test → Backward-compatible raycast enhancement
5. Add US4 (Constraint Readback) → Test → Serialization support
6. Add US5 (Runtime Modification) → Test → Dynamic gameplay support
7. Add US6 (Stride Interop) → Test → Stride3D integration
8. Polish → Version bump → Pack → Publish 0.2.0

### Parallel Team Strategy

With multiple developers after Phase 2 completes:

- Developer A: US1 (Sweep Cast) + US2 (Overlap) — both P1, both in Queries.fs (sequential)
- Developer B: US3 (Filtered Ray) — modifies existing handlers in Queries.fs (coordinate with Dev A)
- Developer C: US4 (Constraint Readback) + US5 (Runtime Modification) — both in PhysicsWorld.fs
- Developer D: US6 (Stride Interop) — new files, no conflicts

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Verify tests fail before implementing (TDD per constitution Principle III)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- US1 and US2 both modify Queries.fs — if working in parallel, coordinate to avoid merge conflicts
- US4 and US5 both modify PhysicsWorld.fs — same coordination applies
- The Stride.BepuPhysics type paths in contracts/StrideInterop.fsi are preliminary — verify actual namespaces after T006 build verification
