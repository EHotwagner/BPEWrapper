# Tasks: Literate Physics Tutorial Book

**Input**: Design documents from `/specs/002-literate-physics-tutorial/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Not explicitly requested. Build verification via `dotnet fsdocs build --properties Configuration=Release` serves as the test gate (SC-002). Each chapter is validated by successful compilation and evaluation during the doc build.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- All tutorial files: `docs/tutorial/`
- DLL reference from tutorial: `../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll`
- Chapter contract: `specs/002-literate-physics-tutorial/contracts/chapter-template.md`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create tutorial directory and verify the doc build pipeline handles the new category

- [x] T001 Create the `docs/tutorial/` directory
- [x] T002 Create a minimal skeleton chapter at `docs/tutorial/01-what-is-physics.fsx` with YAML frontmatter (`category: Tutorial`, `categoryindex: 3`, `index: 1`), NuGet `#r` directives, hidden DLL reference (`../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll`), and a single `PhysicsWorld.create`/`destroy` round-trip to verify the build pipeline works
- [x] T003 Run `dotnet build BepuFSharp/BepuFSharp.fsproj -c Release && dotnet fsdocs build --properties Configuration=Release` and verify the skeleton chapter appears under a "Tutorial" category in `output/`

**Checkpoint**: Tutorial directory exists, FSharp.Formatting discovers files in the new category, and the build pipeline works end-to-end

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: No blocking infrastructure tasks — the setup phase is sufficient. The BepuFSharp library and FSharp.Formatting tooling are already configured. Proceed directly to user stories.

---

## Phase 3: User Story 1 - Beginner Learns Physics Fundamentals (Priority: P1) MVP

**Goal**: A newcomer reads chapters 1-4 and understands what a physics engine does, how shapes/bodies work, what a simulation step is, and how to read results.

**Independent Test**: Run `dotnet fsdocs build --properties Configuration=Release`. All 4 scripts compile and evaluate. A reader with no physics experience can follow chapters 1-4, run the scripts, and predict what happens when a sphere is dropped onto a floor.

### Implementation for User Story 1

- [x] T004 [US1] Author Chapter 1: What Is a Physics Engine at `docs/tutorial/01-what-is-physics.fsx`. Replace the skeleton from T002 with full content. Include: YAML frontmatter (title: "What Is a Physics Engine", category: Tutorial, categoryindex: 3, index: 1), prerequisites (None), F# syntax primer section covering pipe operator, records, discriminated unions, and let bindings per FR-012, links to further F# learning materials, plain-language explanation of what a physics engine does and why games need one (real-world analogies per FR-008), ASCII diagram of a game loop showing where physics fits, minimal code demo creating a world with `PhysicsWorld.create PhysicsConfig.defaults`, stepping once, and destroying, experiment suggestion (e.g., "Try changing gravity to zero or doubling it"), summary linking to Chapter 2. API covered: `PhysicsWorld.create`, `PhysicsConfig.defaults`, `PhysicsWorld.step`, `PhysicsWorld.destroy`
- [x] T005 [US1] Author Chapter 2: Shapes at `docs/tutorial/02-shapes.fsx`. Include: YAML frontmatter (title: "Shapes", index: 2), prerequisites (Chapter 1), plain-language explanation of how physics engines represent geometry (analogy: cookie cutters for collision boundaries), ASCII diagram showing coordinate axes and box dimensions (width/height/length as full extents not half-extents), code demonstrating all 8 PhysicsShape variants (Sphere, Box, Capsule, Cylinder, Triangle, ConvexHull, Compound, Mesh), `addShape`/`removeShape` lifecycle, experiment (e.g., "Create a Capsule with different radius/length values and observe the shape"), summary linking to Chapter 3. API covered: `PhysicsShape` (all 8 variants), `PhysicsWorld.addShape`, `PhysicsWorld.removeShape`
- [x] T006 [US1] Author Chapter 3: Bodies at `docs/tutorial/03-bodies.fsx`. Include: YAML frontmatter (title: "Bodies", index: 3), prerequisites (Chapters 1-2), plain-language explanation of the three body types using real-world analogies (dynamic=thrown ball, static=floor, kinematic=elevator), ASCII diagram showing a scene with all three body types labeled, code creating one of each type using `DynamicBodyDesc.create`, `StaticBodyDesc.create`, `KinematicBodyDesc.create`, explaining mass, pose, and default values, `removeBody`/`removeStatic` lifecycle, experiment (e.g., "Change the mass of the dynamic body from 1 to 100 — does it fall differently?"), summary linking to Chapter 4. API covered: `DynamicBodyDesc`, `KinematicBodyDesc`, `StaticBodyDesc`, `PhysicsWorld.addBody`, `PhysicsWorld.addKinematicBody`, `PhysicsWorld.addStatic`, `PhysicsWorld.removeBody`, `PhysicsWorld.removeStatic`, `Pose.ofPosition`
- [x] T007 [US1] Author Chapter 4: The Simulation Loop at `docs/tutorial/04-simulation-loop.fsx`. Include: YAML frontmatter (title: "The Simulation Loop", index: 4), prerequisites (Chapters 1-3), plain-language explanation of timesteps, fixed vs variable dt, and what happens during a single step (broadphase, narrowphase, solver, integration — explained simply), ASCII diagram of the simulation pipeline stages, code creating a sphere above a floor, stepping 60 times at 1/60s, reading pose after each step to show the sphere falling, using `(*** include-value: ... ***)` to display the final pose, reading velocity with `getBodyVelocity`, setting pose/velocity with `setBodyPose`/`setBodyVelocity`, experiment (e.g., "Step with dt=1/30 instead of 1/60 — how does accuracy change?"), summary linking to Chapter 5. API covered: `PhysicsWorld.step`, `PhysicsWorld.getBodyPose`, `PhysicsWorld.getBodyVelocity`, `PhysicsWorld.setBodyPose`, `PhysicsWorld.setBodyVelocity`, `Pose`, `Velocity`
- [x] T008 [US1] Run `dotnet build BepuFSharp/BepuFSharp.fsproj -c Release && dotnet fsdocs build --properties Configuration=Release` and verify chapters 01-04 all compile, evaluate, and appear in the Tutorial category in correct order

**Checkpoint**: Chapters 1-4 are complete and independently buildable. A reader can learn physics fundamentals from scratch. MVP is deliverable.

---

## Phase 4: User Story 2 - Intermediate Learner Explores Interactions (Priority: P2)

**Goal**: A reader who completed the fundamentals learns how objects interact: collisions, materials, and constraints.

**Independent Test**: Run `dotnet fsdocs build --properties Configuration=Release`. Chapters 05-07 compile and evaluate. A reader can set up contact events, configure materials, and connect bodies with joints.

### Implementation for User Story 2

- [x] T009 [US2] Author Chapter 5: Collisions and Contact Events at `docs/tutorial/05-collisions.fsx`. Include: YAML frontmatter (title: "Collisions and Contact Events", index: 5), prerequisites (Chapters 1-4), plain-language explanation of how physics engines detect collisions (broadphase bounding boxes then narrowphase exact geometry), the contact event lifecycle (began/persisted/ended), ASCII diagram showing two bodies overlapping with contact normal and depth labeled, code creating two bodies that collide, stepping, calling `getContactEvents`, inspecting `ContactEvent` fields (BodyA, BodyB, Normal, Depth, EventType), demonstrating the lifecycle across multiple steps, experiment (e.g., "Add a third body and observe which contact pairs appear"), summary linking to Chapter 6. API covered: `PhysicsWorld.getContactEvents`, `ContactEvent`, `ContactEventType`
- [x] T010 [US2] Author Chapter 6: Materials at `docs/tutorial/06-materials.fsx`. Include: YAML frontmatter (title: "Materials: Friction and Restitution", index: 6), prerequisites (Chapters 1-5), plain-language explanation of friction (ice vs sandpaper analogy) and restitution/bounciness (tennis ball vs bowling ball), spring-based contact model explanation, code creating bodies with different `MaterialProperties` (high friction vs low, high spring frequency for bouncy vs low), showing how `MaterialProperties.defaults` works and how to customize, comparing behavior of a "rubber ball" vs "heavy crate" on a slope, experiment (e.g., "Set friction to 0 and watch objects slide forever"), summary linking to Chapter 7. API covered: `MaterialProperties`, `MaterialProperties.defaults`, `MaterialProperties.create`, `DynamicBodyDesc` with custom material
- [x] T011 [US2] Author Chapter 7: Constraints and Joints at `docs/tutorial/07-constraints.fsx`. Include: YAML frontmatter (title: "Constraints and Joints", index: 7), prerequisites (Chapters 1-6), plain-language explanation of constraints as invisible connections between bodies (analogy: joints in a skeleton, chain links, hinged door), `SpringConfig` explanation (frequency = stiffness, damping ratio = how quickly oscillations stop), ASCII diagram showing BallSocket attachment points on two bodies, code demonstrating at least 4 constraint types with real-world analogues: BallSocket (shoulder joint), Hinge (door hinge), Weld (glued objects), DistanceSpring (bungee cord), `addConstraint`/`removeConstraint` lifecycle, experiment (e.g., "Change the spring frequency from 30 to 5 — how does the joint feel different?"), summary linking to Chapter 8. API covered: `ConstraintDesc` (demonstrate BallSocket, Hinge, Weld, DistanceSpring, DistanceLimit, and at least mention remaining variants), `SpringConfig`, `MotorSettings`, `PhysicsWorld.addConstraint`, `PhysicsWorld.removeConstraint`
- [x] T012 [US2] Run `dotnet build BepuFSharp/BepuFSharp.fsproj -c Release && dotnet fsdocs build --properties Configuration=Release` and verify chapters 05-07 all compile, evaluate, and appear in the Tutorial category in correct order

**Checkpoint**: Chapters 5-7 are complete. A reader understands collisions, materials, and constraints. Incremental delivery point.

---

## Phase 5: User Story 3 - Advanced Reader Tackles Queries and Performance (Priority: P3)

**Goal**: A reader learns collision filtering, raycasting, and bulk operations for ECS integration.

**Independent Test**: Run `dotnet fsdocs build --properties Configuration=Release`. Chapters 08-10 compile and evaluate. A reader can set up layer-based filtering, perform raycasts, and use bulk pose operations.

### Implementation for User Story 3

- [x] T013 [US3] Author Chapter 8: Collision Filtering at `docs/tutorial/08-collision-filtering.fsx`. Include: YAML frontmatter (title: "Collision Filtering with Layers", index: 8), prerequisites (Chapters 1-5), plain-language explanation of why you need filtering (player shouldn't collide with own projectiles, triggers vs solid walls), bitwise group/mask logic explained with truth table, ASCII diagram showing layer groups (Player=1, Enemy=2, Projectile=4) and which pairs collide, code creating bodies with different `CollisionGroup`/`CollisionMask` values, demonstrating that filtered pairs produce no contact events while unfiltered pairs do, experiment (e.g., "Add a 'Trigger' layer that detects overlap but doesn't cause physical response"), summary linking to Chapter 9. API covered: `CollisionFilter`, `CollisionGroup`/`CollisionMask` fields on body descriptors
- [x] T014 [US3] Author Chapter 9: Raycasting and Spatial Queries at `docs/tutorial/09-raycasting.fsx`. Include: YAML frontmatter (title: "Raycasting and Spatial Queries", index: 9), prerequisites (Chapters 1-5), plain-language explanation of raycasting (laser pointer analogy — fire a ray, see what it hits first), use cases (line of sight, bullet traces, ground detection), ASCII diagram showing a ray from origin in a direction hitting objects at different distances, code demonstrating `PhysicsWorld.raycast` (single hit) for line-of-sight check, `PhysicsWorld.raycastAll` (multi hit) for penetrating rays, inspecting `RayHit` fields (Body/Static, Position, Normal, Distance), handling the `option` result for misses, experiment (e.g., "Cast a ray straight down from a character to detect ground distance"), summary linking to Chapter 10. API covered: `PhysicsWorld.raycast`, `PhysicsWorld.raycastAll`, `RayHit`
- [x] T015 [US3] Author Chapter 10: Bulk Operations and ECS Integration at `docs/tutorial/10-bulk-operations.fsx`. Include: YAML frontmatter (title: "Bulk Operations and ECS Integration", index: 10), prerequisites (Chapters 1-4), plain-language explanation of why per-body reads are slow in a game with thousands of entities (ECS architecture overview — components as flat arrays, systems processing batches), ASCII diagram showing ECS sync loop: ECS positions -> writePoses -> step -> readPoses -> ECS positions, code creating 100 bodies, allocating pose/velocity arrays, using `readPoses`/`writePoses`/`readVelocities`/`writeVelocities` for bulk sync, comparing bulk read to individual `getBodyPose` calls, experiment (e.g., "Increase body count to 1000 — bulk operations scale while per-body reads don't"), summary linking to Chapter 11. API covered: `PhysicsWorld.readPoses`, `PhysicsWorld.writePoses`, `PhysicsWorld.readVelocities`, `PhysicsWorld.writeVelocities`
- [x] T016 [US3] Run `dotnet build BepuFSharp/BepuFSharp.fsproj -c Release && dotnet fsdocs build --properties Configuration=Release` and verify chapters 08-10 all compile, evaluate, and appear in the Tutorial category in correct order

**Checkpoint**: Chapters 8-10 are complete. A reader can use advanced features. Incremental delivery point.

---

## Phase 6: User Story 4 - Capstone and Glossary (Priority: P4)

**Goal**: The capstone chapter ties everything together. The glossary provides a quick-reference for all physics terms.

**Independent Test**: Run `dotnet fsdocs build --properties Configuration=Release`. Chapters 11-12 compile/render. The capstone demonstrates all major API areas in a single coherent scene. The glossary covers all physics terms introduced across chapters 1-11.

### Implementation for User Story 4

- [x] T017 [US4] Author Chapter 11: Capstone at `docs/tutorial/11-capstone.fsx`. Include: YAML frontmatter (title: "Putting It All Together", index: 11), prerequisites (Chapters 1-10), brief intro explaining this chapter combines everything learned, then build a complete physics scenario step by step: create world with custom config (Ch 1), register multiple shapes including Box, Sphere, Capsule (Ch 2), add a static floor, dynamic objects with varying masses, and a kinematic platform (Ch 3), configure materials — bouncy ball, heavy crate, icy surface (Ch 6), set collision filtering layers for player/environment/projectile (Ch 8), add BallSocket and Hinge constraints between some bodies (Ch 7), run simulation loop for 120 steps reading poses each frame (Ch 4), query contact events and print collisions (Ch 5), perform raycasts for line-of-sight between two positions (Ch 9), demonstrate bulk pose read for all dynamic bodies (Ch 10), use `(*** include-value: ... ***)` to show key results, annotate each section with a brief comment referencing the original chapter ("As we learned in Chapter 6..."), experiment (e.g., "Modify the scene — add a pendulum chain using DistanceSpring constraints"), summary congratulating the reader and linking to the glossary. API covered: all major PhysicsWorld functions demonstrated in a single script
- [x] T018 [P] [US4] Author Chapter 12: Glossary at `docs/tutorial/12-glossary.md`. Include: YAML frontmatter (title: "Glossary", category: Tutorial, categoryindex: 3, index: 12), alphabetically sorted table of all physics terms introduced in chapters 1-11 with columns: Term, Definition (plain language), First Appears (linked to chapter). Terms must include at minimum: body (dynamic/static/kinematic), broadphase, capsule, collision filter, collision group/mask, compound shape, constraint, contact event, contact manifold, contact normal, convex hull, damping ratio, depth (penetration), discriminated union, ECS, friction, gravity, hinge, inertia, joint, kinematic body, mass, material properties, mesh, narrowphase, orientation, physics engine, physics world, pipe operator, pose, quaternion, raycast, ray hit, restitution, rigid body, shape, simulation step, solver, spring config, spring frequency, static body, substep, sweep test, timestep, torque, velocity (linear/angular), weld
- [x] T019 [US4] Run `dotnet build BepuFSharp/BepuFSharp.fsproj -c Release && dotnet fsdocs build --properties Configuration=Release` and verify chapters 11-12 compile/render and appear in the Tutorial category in correct order

**Checkpoint**: All 12 chapters are complete. Full tutorial book is delivered.

---

## Phase 7: Polish and Cross-Cutting Concerns

**Purpose**: Site integration and final validation

- [x] T020 Add a "Tutorial" link to the documentation section of `docs/index.fsx` pointing to `tutorial/01-what-is-physics.html` with a brief description like "Step-by-step tutorial covering 3D physics from scratch"
- [x] T021 Run full documentation build (`dotnet build BepuFSharp/BepuFSharp.fsproj -c Release && dotnet fsdocs build --properties Configuration=Release`) and verify: all 12 tutorial pages render, Tutorial category appears in site navigation at position 4 (after Architecture Decisions), chapters are listed in sequential order (1-12), all chapter cross-links work, existing Guides and ADR pages are unaffected
- [x] T022 Review all chapters for consistency: each has prerequisites stated (FR-006), each begins with concept-first prose (FR-003), each uses real-world analogies (FR-008), each has at least one experiment (FR-010), each has ASCII diagrams where spatial concepts are involved (FR-011), each creates and destroys its own PhysicsWorld (FR-004), no raw BepuPhysics2 API usage (FR-007)

---

## Dependencies and Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Skipped — no blocking prerequisites
- **US1 (Phase 3)**: Depends on Setup (Phase 1) — T004-T007 are sequential (content builds pedagogically)
- **US2 (Phase 4)**: Depends on US1 completion (chapters reference fundamentals content for consistency)
- **US3 (Phase 5)**: Depends on US1 completion (chapters reference fundamentals content for consistency)
- **US4 (Phase 6)**: Depends on US1, US2, US3 completion (capstone references all prior chapters)
- **Polish (Phase 7)**: Depends on all user stories complete

### User Story Dependencies

- **US1 (P1)**: Can start after Setup — no other story dependencies
- **US2 (P2)**: Depends on US1 — references fundamentals content for consistency, but technically compilable independently
- **US3 (P3)**: Depends on US1 — references fundamentals content for consistency, but technically compilable independently
- **US4 (P4)**: Depends on US1 + US2 + US3 — capstone references all chapters, glossary indexes all terms

### Within Each User Story

- Chapters are authored sequentially within a story (pedagogical ordering)
- Build verification runs after all chapters in a story are complete
- Each story's build verification confirms independent testability

### Parallel Opportunities

- **US2 and US3 can run in parallel** after US1 is complete (different files, no content overlap)
- **T018 (glossary) can run in parallel** with T017 (capstone) since they are different files
- Within each story, chapters are sequential due to pedagogical dependencies

---

## Parallel Example: User Stories 2 and 3

```text
# After US1 (Phase 3) is complete, launch US2 and US3 in parallel:

# Stream A (US2 - Interactions):
Task T009: "Author Chapter 5: Collisions at docs/tutorial/05-collisions.fsx"
Task T010: "Author Chapter 6: Materials at docs/tutorial/06-materials.fsx"
Task T011: "Author Chapter 7: Constraints at docs/tutorial/07-constraints.fsx"
Task T012: "Build verification for chapters 05-07"

# Stream B (US3 - Advanced):
Task T013: "Author Chapter 8: Collision Filtering at docs/tutorial/08-collision-filtering.fsx"
Task T014: "Author Chapter 9: Raycasting at docs/tutorial/09-raycasting.fsx"
Task T015: "Author Chapter 10: Bulk Operations at docs/tutorial/10-bulk-operations.fsx"
Task T016: "Build verification for chapters 08-10"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T003)
2. Complete Phase 3: User Story 1 — Chapters 1-4 (T004-T008)
3. **STOP and VALIDATE**: Build succeeds, 4 chapters render in Tutorial category
4. Deliverable: A beginner can learn physics fundamentals

### Incremental Delivery

1. Setup (T001-T003) -> Foundation ready
2. US1: Chapters 1-4 (T004-T008) -> MVP: Physics fundamentals
3. US2: Chapters 5-7 (T009-T012) -> Add: Object interactions
4. US3: Chapters 8-10 (T013-T016) -> Add: Queries and performance
5. US4: Chapters 11-12 (T017-T019) -> Add: Capstone and glossary
6. Polish (T020-T022) -> Final: Site integration and consistency review

Each increment adds value without breaking previous chapters.

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- No test tasks generated — build verification via `dotnet fsdocs build` serves as the test gate per SC-002
- Each chapter is a single file (~200-400 lines) following the contract in `specs/002-literate-physics-tutorial/contracts/chapter-template.md`
- Commit after each chapter or logical group
- Stop at any checkpoint to validate independently
