# Tasks: BepuPhysics2 F# Wrapper

**Input**: Design documents from `/specs/001-bepu-fsharp-wrapper/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/public-api.md

**Tests**: Required per Constitution III (Test Evidence Is Mandatory). Tests MUST fail before implementation and pass after.

**Organization**: Tasks grouped by user story. Stories US1+US4+US6 are combined into a single "Core World" phase because they are inseparable (a world needs shapes to create bodies, and stepping to verify anything works).

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, NuGet references, build configuration

- [x] T001 Create Directory.Build.props at repository root with PackageOutputPath (~/.local/share/nuget-local/), TreatWarningsAsErrors, warnon:1182, GenerateDocumentationFile, packaging metadata (id, version, authors, description) in Directory.Build.props
- [x] T002 Create F# library project BepuFSharp/BepuFSharp.fsproj targeting net8.0 with NuGet references to BepuPhysics 2.4.0 and BepuUtilities 2.4.0; configure F# compilation order for all planned source files in BepuFSharp/BepuFSharp.fsproj
- [x] T003 [P] Create F# test project BepuFSharp.Tests/BepuFSharp.Tests.fsproj with references to Expecto, FsCheck, and BepuFSharp project in BepuFSharp.Tests/BepuFSharp.Tests.fsproj
- [x] T004 [P] Create solution file BepuFSharp.sln linking both projects; verify `dotnet build` succeeds with empty source files

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core value types, interop helpers, and diagnostics shared by ALL user stories

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 Implement Types.fsi with all core types: BodyId, StaticId, ShapeId, ConstraintId (opaque struct DU wrappers), Pose, Velocity (struct records), SpringConfig, MaterialProperties, PhysicsConfig (with defaults), ContactEventType DU, ContactEvent (struct with voption body/static fields per clarification), RayHit (struct with voption body/static), MotorSettings, CollisionFilter; include XML doc comments with summaries for all public types and functions (FR-001/002/003/017/026/021) in BepuFSharp/Types.fsi
- [x] T006 Implement Types.fs matching the Types.fsi signature with all type definitions, Pose.create/ofPosition/identity, Velocity.zero/create, SpringConfig.create, MaterialProperties.defaults, PhysicsConfig.defaults constructors in BepuFSharp/Types.fs
- [x] T007 [P] Implement Diagnostics.fsi with PhysicsError DU (InvalidBodyHandle, InvalidStaticHandle, InvalidShapeHandle, InvalidConstraintHandle, NegativeMass, DegenerateShape, ShapeInUse, WorldDisposed, BufferPoolExhausted), PhysicsDiagnosticEvent DU (WorldCreated, WorldDisposed, ConstraintAutoRemoved, ShapeRemovalBlocked, BufferPoolWarning), and PhysicsError.describe function; XML doc comments in .fsi (FR-036) in BepuFSharp/Diagnostics.fsi
- [x] T008 [P] Implement Diagnostics.fs with error description strings and diagnostic event emission helpers in BepuFSharp/Diagnostics.fs
- [x] T009 Implement Interop.fs (internal, no .fsi) with inline conversion functions: Pose to/from RigidPose, Velocity to/from BodyVelocity, SpringConfig to/from SpringSettings, ShapeId to/from TypedIndex, BodyId to/from BodyHandle, StaticId to/from StaticHandle, ConstraintId to/from ConstraintHandle; apply AggressiveInlining to hot-path functions in BepuFSharp/Interop.fs

**Checkpoint**: Foundation ready -- all shared types and conversions available for story implementation

---

## Phase 3: User Stories 1+4+6 - Core World, Shapes, Stepping (Priority: P1)

**Goal**: Create a physics world with default or custom config, register shapes, step the simulation, and dispose resources. This is the atomic core -- these three stories are inseparable.

**Independent Test**: Create a world, add a sphere shape, step once, verify no crash and valid state. Create/destroy lifecycle works.

### Tests for US1+US4+US6

- [x] T010 [P] [US1] Write world creation tests in BepuFSharp.Tests/WorldTests.fs: default config creates world with gravity -9.81 Y and 8 velocity iterations; custom config with gravity (0,-20,0) and substeps=4 is reflected; destroy disposes all resources; step advances simulation; stepping with dt=0 is handled; deterministic mode produces identical results
- [x] T011 [P] [US4] Write shape registration tests in BepuFSharp.Tests/ShapeTests.fs: create and register each shape variant (Sphere, Box, Capsule, Cylinder, Triangle, ConvexHull, Compound, Mesh); verify returned ShapeId is valid; verify Box full-dimensions are halved internally; verify degenerate shapes (zero-radius sphere, zero-volume box) are rejected with DegenerateShape error; verify removeShape succeeds when no bodies reference it

### Implementation for US1+US4+US6

- [x] T012 [P] [US4] Implement Shapes.fsi with PhysicsShape DU (Sphere, Box, Capsule, Cylinder, Triangle, ConvexHull, Compound, Mesh) and CompoundChild struct type; XML doc comments for all cases (FR-004) in BepuFSharp/Shapes.fsi
- [x] T013 [P] [US4] Implement Shapes.fs with PhysicsShape DU definition and internal shape validation (positive dimensions, minimum point counts) in BepuFSharp/Shapes.fs
- [x] T014 [US1] Implement Callbacks.fs (internal, no .fsi) with default [<Struct>] NarrowPhaseCallbacks (AllowContactGeneration returns true, ConfigureContactManifold applies default material, Initialize/Dispose stubs) and [<Struct>] PoseIntegratorCallbacks (applies gravity from config, default linear/angular damping in IntegrateVelocity) (FR-013) in BepuFSharp/Callbacks.fs
- [x] T015 [US1] Implement PhysicsWorld.fsi with PhysicsWorld type (IDisposable), PhysicsWorld.create, PhysicsWorld.createCustom, PhysicsWorld.destroy, PhysicsWorld.step, PhysicsWorld.addShape, PhysicsWorld.removeShape, PhysicsWorld.simulation (escape hatch), PhysicsWorld.bufferPool (escape hatch); XML doc comments for all (FR-010/011/012/014) in BepuFSharp/PhysicsWorld.fsi
- [x] T016 [US1] Implement PhysicsWorld.fs: PhysicsWorld type holding Simulation, BufferPool, ThreadDispatcher, MaterialTable, CollisionFilterTable, ContactEventBuffer, Config; create function using Simulation.Create with SolveDescription from config, ThreadDispatcher from thread count (0=auto via Environment.ProcessorCount), BufferPool; destroy function disposing in order (Simulation, ThreadDispatcher, BufferPool); step function calling Simulation.Timestep(dt, threadDispatcher); addShape matching on PhysicsShape DU and calling Simulation.Shapes.Add (halving Box dimensions); removeShape calling Simulation.Shapes.RemoveAndDispose; IDisposable forwarding to destroy; edge cases: dt<=0 no-op, WorldDisposed error on use after dispose (FR-010/012/028/029/036) in BepuFSharp/PhysicsWorld.fs

**Checkpoint**: Can create a world, add shapes, step, and destroy. All three stories (US1, US4, US6) independently testable.

---

## Phase 4: User Story 2 - Add and Remove Rigid Bodies (Priority: P1)

**Goal**: Add dynamic, kinematic, and static bodies to the simulation using descriptors with sensible defaults, receive opaque typed handles back, and remove bodies.

**Independent Test**: Add a dynamic sphere, step the simulation, verify the body exists and has a valid handle. Remove it and verify invalidation.

### Tests for US2

- [x] T017 [P] [US2] Write body lifecycle tests in BepuFSharp.Tests/BodyTests.fs: add dynamic body with Sphere(1)/mass=1/position=(0,5,0) returns BodyId; add static body with Box(100,1,100) at origin returns StaticId; removeBody removes body and handle is invalid; removeStatic removes static; add body with zero mass creates kinematic; add body with negative mass raises NegativeMass error; add kinematic body returns BodyId with infinite mass behavior

### Implementation for US2

- [x] T018 [P] [US2] Implement Bodies.fsi with DynamicBodyDesc, KinematicBodyDesc, StaticBodyDesc struct types and DynamicBodyDesc.create (shape, pose, mass), KinematicBodyDesc.create (shape, pose), StaticBodyDesc.create (shape, pose) factory functions with sensible defaults (velocity=zero, material=defaults, collisionGroup=0, collisionMask=all, sleepThreshold=0.01, CCD=false); XML doc comments (FR-006/007/008/009) in BepuFSharp/Bodies.fsi
- [x] T019 [P] [US2] Implement Bodies.fs with body descriptor types and factory functions filling defaults in BepuFSharp/Bodies.fs
- [x] T020 [US2] Add body functions to PhysicsWorld.fsi: addBody, addKinematicBody, addStatic, removeBody, removeStatic in BepuFSharp/PhysicsWorld.fsi
- [x] T021 [US2] Implement body functions in PhysicsWorld.fs: addBody computes inertia via shape.ComputeInertia(mass) for convex shapes (handle Compound/Mesh separately), constructs BodyDescription, calls Simulation.Bodies.Add; zero mass -> kinematic via BodyDescription.CreateKinematic; negative mass -> raise with NegativeMass error; addStatic constructs StaticDescription and calls Simulation.Statics.Add; register material and collision filter in internal tables; removeBody auto-removes all constraints referencing the body (iterate Simulation.Bodies constraints, emit ConstraintAutoRemoved diagnostic per removal), then calls Simulation.Bodies.Remove and cleans material/filter tables; removeStatic calls Simulation.Statics.Remove (FR-005/006/009/028/029) in BepuFSharp/PhysicsWorld.fs

**Checkpoint**: Can add/remove dynamic, kinematic, and static bodies. A sphere falls under gravity when stepped.

---

## Phase 5: User Story 3 - Read and Write Body State for ECS Integration (Priority: P1)

**Goal**: Read positions/orientations/velocities from bodies (single and bulk) and write poses/velocities back, enabling ECS synchronization.

**Independent Test**: Add 1000 bodies, step once, bulk-read all poses, verify they changed from initial values with no per-body allocation.

### Tests for US3

- [x] T022 [P] [US3] Write state access tests in BepuFSharp.Tests/BulkOperationTests.fs: getBodyPose returns correct position/orientation after step; setBodyVelocity updates velocity for next step; round-trip pose (set then get) preserves values; readPoses with 1000 bodies populates pre-allocated array in correct order; readVelocities bulk read works; writePoses teleports kinematic bodies; writeVelocities bulk set works; verify no GC allocations in bulk read path using GC.GetAllocatedBytesForCurrentThread delta check

### Implementation for US3

- [x] T023 [US3] Add state access functions to PhysicsWorld.fsi: getBodyPose, getBodyVelocity, setBodyPose, setBodyVelocity (single entity); readPoses, readVelocities, writePoses, writeVelocities (bulk, pre-allocated arrays) (FR-022/023/024/025) in BepuFSharp/PhysicsWorld.fsi
- [x] T024 [US3] Implement state access in PhysicsWorld.fs: getBodyPose reads from Simulation.Bodies[handle].Pose and converts via Interop; setBodyPose writes directly; getBodyVelocity/setBodyVelocity same pattern; bulk readPoses iterates handle array and populates Pose[] without allocation (direct struct copy via Interop); bulk readVelocities same; bulk writePoses/writeVelocities same; validate handle existence and raise InvalidBodyHandle error on stale handles (FR-022/023/024/025/036) in BepuFSharp/PhysicsWorld.fs

**Checkpoint**: ECS integration path works. Bulk operations are zero-allocation.

---

## Phase 6: User Story 5 - Add Constraints Between Bodies (Priority: P1)

**Goal**: Connect bodies with constraints (ball socket, hinge, weld, etc.) using a DU of constraint descriptors. Add and remove constraints.

**Independent Test**: Create two bodies, connect with BallSocket, step, verify they remain connected (distance between bodies stays within constraint tolerance).

### Tests for US5

- [x] T025 [P] [US5] Write constraint tests in BepuFSharp.Tests/ConstraintTests.fs: add BallSocket between two bodies returns ConstraintId and bodies stay connected after stepping; removeConstraint removes the constraint; Hinge constraint respects angular limits; Weld constraint rigidly attaches bodies; add constraint with invalid BodyId raises InvalidBodyHandle error; removeBody with active constraint auto-removes constraint first and emits ConstraintAutoRemoved diagnostic

### Implementation for US5

- [x] T026 [P] [US5] Implement Constraints.fsi with ConstraintDesc DU (BallSocket, Hinge, Weld, DistanceLimit, DistanceSpring, SwingLimit, TwistLimit, LinearAxisMotor, AngularMotor, PointOnLine) with per-case struct fields per data-model.md; XML doc comments for all cases (FR-015/016) in BepuFSharp/Constraints.fsi
- [x] T027 [P] [US5] Implement Constraints.fs with ConstraintDesc DU definition in BepuFSharp/Constraints.fs
- [x] T028 [US5] Add addConstraint and removeConstraint to PhysicsWorld.fsi (FR-018) in BepuFSharp/PhysicsWorld.fsi
- [x] T029 [US5] Implement addConstraint in PhysicsWorld.fs: match on ConstraintDesc DU, construct appropriate BepuPhysics constraint struct (BallSocket, Hinge, etc.) using Interop conversions for SpringSettings/offsets/axes, call Simulation.Solver.Add(bodyHandleA, bodyHandleB, description) and wrap result as ConstraintId; implement removeConstraint calling Simulation.Solver.Remove (FR-015/018) in BepuFSharp/PhysicsWorld.fs

**Checkpoint**: Constraints work. Ball socket, hinge, weld verified. Ragdolls and mechanisms are possible.

---

## Phase 7: User Story 7 - FSI Scripting Accessibility (Priority: P1)

**Goal**: Load the BepuFSharp library in F# Interactive via a single #load directive and prototype physics scenarios interactively.

**Independent Test**: Run scripts/prelude.fsx in FSI, call PhysicsWorld.create, add a body, step, read pose -- all succeed.

### Implementation for US7

- [x] T030 [US7] Verify dotnet pack produces valid .nupkg to ~/.local/share/nuget-local/ by running `dotnet pack BepuFSharp/BepuFSharp.fsproj` and checking output (FR-033)
- [x] T031 [US7] Create scripts/prelude.fsx with #r directives referencing packed NuGet output, open BepuFSharp, and ergonomic helper functions for interactive use (FR-034) in scripts/prelude.fsx
- [x] T032 [P] [US7] Create scripts/examples/01-hello-physics.fsx: sphere falling onto a floor with gravity (demonstrates US1+US2+US4+US6 in <10 lines) in scripts/examples/01-hello-physics.fsx
- [x] T033 [P] [US7] Create scripts/examples/02-body-management.fsx: adding/removing dynamic, kinematic, static bodies (demonstrates US2) in scripts/examples/02-body-management.fsx
- [x] T034 [P] [US7] Create scripts/examples/03-bulk-ecs-sync.fsx: bulk pose read/write for ECS integration (demonstrates US3) in scripts/examples/03-bulk-ecs-sync.fsx
- [x] T035 [P] [US7] Create scripts/examples/04-constraints.fsx: ball socket, hinge, weld joints (demonstrates US5) in scripts/examples/04-constraints.fsx

**Checkpoint**: Library is usable from FSI. Prelude and 4 example scripts work. Constitution V satisfied for P1 stories.

---

## Phase 8: User Story 8 - Collision Events / Contact Queries (Priority: P2)

**Goal**: Receive collision events (began, persisted, ended) as a flat buffer of struct records after each step, supporting body-body and body-static contacts.

**Independent Test**: Drop a sphere onto a static floor, step, verify a ContactEvent with Began type is produced with correct body/static identifiers and contact normal.

### Tests for US8

- [x] T036 [P] [US8] Write contact event tests in BepuFSharp.Tests/ContactEventTests.fs: sphere drops onto static floor produces Began event with correct BodyA/StaticB and upward normal; continued overlap next step produces Persisted event; separation produces Ended event; two dynamic bodies colliding produces event with BodyA and BodyB (no statics); body-static contact has BodyA=ValueSome and StaticB=ValueSome per clarification

### Implementation for US8

- [x] T037 [P] [US8] Reserve -- merged into T038 (ContactEvents is internal, no .fsi needed)
- [x] T038 [P] [US8] Implement ContactEvents.fs (internal, no .fsi) with double-buffered ContactEventBuffer: write buffer collects events during step (thread-safe append), swap buffers after step completes, getContactEvents reads from completed buffer; IMPORTANT: must also track previous-frame contact pairs (by CollidablePair hash set) and emit Ended events for pairs present in the previous frame but absent in the current frame, since BepuPhysics2 callbacks only fire for active contacts (FR-027) in BepuFSharp/ContactEvents.fs
- [x] T039 [US8] Update Callbacks.fs to integrate ContactEventBuffer into NarrowPhaseCallbacks: in ConfigureContactManifold, append ContactEvent to write buffer with correct body/static identification using CollidableReference.Mobility to distinguish bodies from statics (FR-013/026) in BepuFSharp/Callbacks.fs
- [x] T040 [US8] Add getContactEvents to PhysicsWorld.fsi and implement in PhysicsWorld.fs: return completed buffer contents as ContactEvent[]; update step function to swap buffers after Simulation.Timestep returns (FR-027) in BepuFSharp/PhysicsWorld.fsi and BepuFSharp/PhysicsWorld.fs
- [x] T041 [P] [US8] Create scripts/examples/06-collision-events.fsx demonstrating contact event processing in scripts/examples/06-collision-events.fsx

**Checkpoint**: Contact events work for body-body and body-static collisions with began/persisted/ended lifecycle.

---

## Phase 9: User Story 9 - Collision Filtering (Priority: P2)

**Goal**: Assign collision groups and masks to bodies to control which pairs generate contacts using a 32-layer bitmask system.

**Independent Test**: Create two bodies in the same non-colliding group, verify no contacts are generated when they overlap.

### Tests for US9

- [x] T042 [P] [US9] Write collision filter tests in BepuFSharp.Tests/ContactEventTests.fs (or new file): body with group=1/mask=0b10 and body with group=2/mask=0b01 overlap and contacts ARE generated; two bodies both in group=1 with mask excluding group 1 overlap and contacts are NOT generated; default mask (0xFFFFFFFF) collides with everything

### Implementation for US9

- [x] T043 [US9] Update Callbacks.fs NarrowPhaseCallbacks.AllowContactGeneration to implement bitmask filter: look up CollisionFilter for each CollidableReference from PhysicsWorld's CollisionFilterTable, evaluate (a.mask &&& (1u <<< b.group)) <> 0u && (b.mask &&& (1u <<< a.group)) <> 0u, return false if filter rejects (FR-013) in BepuFSharp/Callbacks.fs
- [x] T044 [P] [US9] Create scripts/examples/07-collision-filtering.fsx demonstrating layer-based collision groups/masks in scripts/examples/07-collision-filtering.fsx

**Checkpoint**: Collision filtering works. Game-typical layer setups (player/environment/projectile) are possible.

---

## Phase 10: User Story 10 - Ray and Shape Casting (Priority: P2)

**Goal**: Cast rays against the simulation to query for hits, supporting single-hit and multi-hit modes.

**Independent Test**: Cast a ray downward above a static floor, verify it reports a hit with correct distance and upward normal.

### Tests for US10

- [x] T045 [P] [US10] Write raycast tests in BepuFSharp.Tests/QueryTests.fs: ray hitting static floor returns RayHit with StaticId, correct distance and upward normal; ray missing all bodies returns None; raycastAll returns multiple hits sorted by distance; ray hitting a dynamic body returns RayHit with BodyId

### Implementation for US10

- [x] T046 [P] [US10] Implement Queries.fs (internal, no .fsi) with [<Struct>] SingleHitHandler implementing IRayHitHandler (stores closest hit, updates maximumT), [<Struct>] MultiHitHandler implementing IRayHitHandler (collects all hits into list, sorts by distance); both handlers resolve CollidableReference to BodyId/StaticId using Mobility check (FR-019/020/021) in BepuFSharp/Queries.fs
- [x] T048 [US10] Add raycast and raycastAll to PhysicsWorld.fsi and implement in PhysicsWorld.fs: raycast creates SingleHitHandler, calls Simulation.RayCast, returns RayHit option; raycastAll creates MultiHitHandler, calls Simulation.RayCast, returns sorted RayHit[] (FR-019/020) in BepuFSharp/PhysicsWorld.fsi and BepuFSharp/PhysicsWorld.fs
- [x] T049 [P] [US10] Create scripts/examples/05-raycasting.fsx demonstrating ray queries and hit processing in scripts/examples/05-raycasting.fsx

**Checkpoint**: Raycasting works for single and multi-hit queries against bodies and statics.

---

## Phase 11: User Story 11 - Material Properties (Priority: P2)

**Goal**: Define surface materials (friction, spring settings) and assign them per-body so different surfaces behave differently on contact.

**Independent Test**: Create a high-friction body and a low-friction body on a slope, verify the low-friction body slides faster after stepping.

### Tests for US11

- [x] T050 [P] [US11] Write material tests in BepuFSharp.Tests/BodyTests.fs (append) or new file: bodies with different friction values on an angled surface slide at different rates; MaterialProperties.create constructs correct values; per-body material is used in contact response (not just default)

### Implementation for US11

- [x] T051 [P] [US11] Implement Materials.fsi with MaterialProperties.create function in BepuFSharp/Materials.fsi
- [x] T052 [P] [US11] Implement Materials.fs with MaterialProperties.create and any material table management helpers in BepuFSharp/Materials.fs
- [x] T053 [US11] Update Callbacks.fs NarrowPhaseCallbacks.ConfigureContactManifold to look up per-body MaterialProperties from PhysicsWorld's MaterialTable (keyed by body storage index), combine materials for the contact pair (e.g., use minimum friction), and apply friction/spring settings to the contact manifold (FR-013) in BepuFSharp/Callbacks.fs
- [x] T054 [P] [US11] Create scripts/examples/08-materials.fsx demonstrating surface material properties in scripts/examples/08-materials.fsx

**Checkpoint**: Materials work. Different surfaces have distinct friction/bounciness behavior.

---

## Phase 12: User Story 12 - Debug Visualization Data (Priority: P3, Optional)

**Goal**: Extract shape wireframes, contact points, constraint anchors, and bounding boxes as vertex data for debug rendering.

**Independent Test**: Create a world with bodies and constraints, extract debug vertex data, verify it contains expected geometry.

### Tests for US12

- [ ] T055a [P] [US12] Write debug visualization tests in BepuFSharp.Tests/WorldTests.fs (append): create world with sphere body, extract debug data, verify vertex array is non-empty and contains expected wireframe vertex count for sphere

### Implementation for US12

- [ ] T055 [US12] Design and implement debug visualization data extraction: shape wireframe generation (sphere -> line segments, box -> 12 edges, etc.), contact point extraction, constraint anchor positions, AABB extraction; expose as PhysicsWorld.getDebugData returning vertex arrays in BepuFSharp/PhysicsWorld.fsi and BepuFSharp/PhysicsWorld.fs

**Checkpoint**: Debug visualization data available for rendering.

---

## Phase 13: User Story 13 - Serialization / Snapshot (Priority: P3, Optional)

**Goal**: Snapshot the entire physics world state to a byte array and restore it for save/load and network rollback.

**Independent Test**: Create a world, add bodies, step, snapshot, restore, verify restored world matches original.

### Tests for US13

- [ ] T056a [P] [US13] Write snapshot/restore tests in BepuFSharp.Tests/WorldTests.fs (append): create world, add bodies, step, snapshot to byte array, restore to new world, verify body poses match original within tolerance

### Implementation for US13

- [ ] T056 [US13] Design and implement world snapshot/restore: serialize all body poses/velocities/materials/filters/constraints/shapes to byte array; restore by creating a new world and re-adding all entities; expose as PhysicsWorld.snapshot and PhysicsWorld.restore in BepuFSharp/PhysicsWorld.fsi and BepuFSharp/PhysicsWorld.fs

**Checkpoint**: World state can be serialized and deserialized.

---

## Phase 14: Polish & Cross-Cutting Concerns

**Purpose**: Surface-area baselines, documentation, performance validation, final quality

### Surface Area & Signatures

- [x] T057 Generate surface-area baseline files for all public modules (Types, Shapes, Bodies, Constraints, Materials, PhysicsWorld, Diagnostics) and store as baseline text files in BepuFSharp.Tests/baselines/
- [x] T058 Implement SurfaceAreaTests.fs that loads each .fsi file, extracts public API surface, and compares against baseline files; test fails if surface diverges from baseline (FR-032) in BepuFSharp.Tests/SurfaceAreaTests.fs

### Property-Based Tests

- [x] T059 [P] Implement PropertyTests.fs with FsCheck property tests: pose round-trip preserves values (set/get identity); velocity round-trip preserves values; bulk readPoses/writePoses is idempotent; removing a body invalidates its handle; SpringConfig frequency/dampingRatio round-trip through BepuPhysics SpringSettings in BepuFSharp.Tests/PropertyTests.fs

### API Documentation (Constitution VI)

- [x] T060 Run api-doc skill: ensure all .fsi files have XML doc comments with `<summary>` for every public module, type, function, and DU case; add usage examples to key functions (FR-031)
- [ ] T061 Run doc-setup skill: install fsdocs-tool, create docs/ directory, configure MSBuild properties in Directory.Build.props (FR-037)
- [ ] T062 [P] Run doc-examples skill: create docs/index.fsx (landing page), docs/getting-started.fsx (quick start tutorial), docs/ecs-integration.fsx (data-oriented ECS sync patterns) as literate scripts (FR-037)
- [ ] T063 [P] Run doc-technical skill: create ADRs docs/adr/001-mutable-world.md, docs/adr/002-du-shapes-constraints.md, docs/adr/003-opaque-handles.md, docs/adr/004-callback-strategy.md (FR-038)
- [ ] T064 Run doc-build skill: verify `dotnet fsdocs build` succeeds without errors (FR-037)

### Performance Validation

- [x] T065 Profile hot paths: verify zero managed allocations in step/readPoses/writePoses/getBodyPose/setBodyVelocity loop using GC.GetAllocatedBytesForCurrentThread; verify bulk readback for 10K bodies completes in <1ms; verify wrapper overhead is <5% vs raw BepuPhysics2 (SC-002/003/006)

### Final Validation

- [x] T066 Verify `dotnet build` succeeds with zero warnings under TreatWarningsAsErrors + warnon:1182 (SC-004)
- [x] T067 Verify `dotnet pack` produces valid .nupkg to ~/.local/share/nuget-local/ (SC-010)
- [ ] T068 Verify all numbered example scripts under scripts/examples/ run without error against latest packed build (SC-011)
- [ ] T069 Run quickstart.md validation: execute the "Hello Physics" code from quickstart.md and verify it works end-to-end (SC-001)
- [x] T070 Update surface-area baselines for any modules changed during polish phase in BepuFSharp.Tests/baselines/

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies -- can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion -- BLOCKS all user stories
- **US1+US4+US6 (Phase 3)**: Depends on Foundational -- core world is the first deliverable
- **US2 (Phase 4)**: Depends on Phase 3 (needs world + shapes + step)
- **US3 (Phase 5)**: Depends on Phase 4 (needs bodies to read/write state)
- **US5 (Phase 6)**: Depends on Phase 4 (needs bodies to constrain)
- **US7 (Phase 7)**: Depends on Phases 3-6 (needs packable library with core API)
- **US8 (Phase 8)**: Depends on Phase 4 (needs bodies that collide) -- can parallel with US5/US7
- **US9 (Phase 9)**: Depends on Phase 8 (builds on callback infrastructure)
- **US10 (Phase 10)**: Depends on Phase 4 (needs bodies to raycast against) -- can parallel with US5/US7/US8
- **US11 (Phase 11)**: Depends on Phase 8 (builds on callback infrastructure) -- can parallel with US9/US10
- **US12/US13 (Phases 12-13)**: Optional stretch goals, depend on all P1 stories
- **Polish (Phase 14)**: Depends on all desired user stories being complete

### User Story Dependencies

```
Phase 1 (Setup)
    │
Phase 2 (Foundational)
    │
Phase 3 (US1+US4+US6: Core World)
    │
Phase 4 (US2: Bodies)
    │
    ├── Phase 5 (US3: State Access)
    ├── Phase 6 (US5: Constraints) ──── Phase 7 (US7: FSI Scripting)*
    ├── Phase 8 (US8: Contact Events)
    │       │
    │       ├── Phase 9 (US9: Collision Filtering)
    │       └── Phase 11 (US11: Materials)
    └── Phase 10 (US10: Raycasting)

* US7 depends on all P1 stories for full example coverage
```

### Parallel Opportunities After Phase 4

Once bodies work (Phase 4), these can proceed in parallel:
- US3 (State Access) -- different functions in PhysicsWorld
- US5 (Constraints) -- different module + PhysicsWorld functions
- US8 (Contact Events) -- different module + Callbacks changes
- US10 (Raycasting) -- different module + PhysicsWorld functions

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- .fsi signature files before .fs implementation files
- PhysicsWorld.fsi additions before PhysicsWorld.fs implementation
- Core implementation before example scripts

---

## Parallel Example: Phase 3 (US1+US4+US6)

```bash
# These test files can be written in parallel:
Task T010: "Write world creation tests in BepuFSharp.Tests/WorldTests.fs"
Task T011: "Write shape registration tests in BepuFSharp.Tests/ShapeTests.fs"

# These .fsi/.fs pairs can be written in parallel (different modules):
Task T012: "Implement Shapes.fsi"
Task T013: "Implement Shapes.fs"
# (in parallel with Callbacks.fs since different files)
```

## Parallel Example: After Phase 4

```bash
# These can all proceed in parallel since they touch different modules:
Task T022 (US3): "Write state access tests in BulkOperationTests.fs"
Task T025 (US5): "Write constraint tests in ConstraintTests.fs"
Task T036 (US8): "Write contact event tests in ContactEventTests.fs"
Task T045 (US10): "Write raycast tests in QueryTests.fs"
```

---

## Implementation Strategy

### MVP First (Phases 1-4)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (Types, Interop, Diagnostics)
3. Complete Phase 3: Core World (US1+US4+US6)
4. Complete Phase 4: Bodies (US2)
5. **STOP and VALIDATE**: A sphere falls under gravity, bodies can be added/removed
6. This is a usable physics wrapper for basic scenarios

### Incremental Delivery

1. Phases 1-4 -> MVP: world + shapes + bodies + stepping
2. Add Phase 5 (US3) -> ECS integration works
3. Add Phase 6 (US5) -> Constraints work -> Phase 7 (US7) -> FSI scripts
4. Add Phases 8-11 (P2 stories) -> Contact events, filtering, raycasting, materials
5. Add Phases 12-13 (P3 stories) -> Debug viz, serialization (optional)
6. Phase 14 -> Polish, docs, baselines, performance validation

### Suggested MVP Scope

**Phases 1-4** (T001-T021): 21 tasks delivering a functional physics wrapper with world creation, shape registration, body lifecycle, and simulation stepping. This covers US1, US2, US4, US6 and validates the core architecture.

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Constitution III requires tests -- test tasks are included for all stories
- Callbacks.fs is progressively enhanced: Phase 3 (basic), Phase 8 (events), Phase 9 (filtering), Phase 11 (materials)
- PhysicsWorld.fsi/.fs grow incrementally as new story functions are added
- US12/US13 are optional stretch goals per spec assumptions
