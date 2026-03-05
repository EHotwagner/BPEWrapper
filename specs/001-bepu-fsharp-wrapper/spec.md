# Feature Specification: BepuPhysics2 F# Wrapper

**Feature Branch**: `001-bepu-fsharp-wrapper`
**Created**: 2026-03-05
**Status**: Draft
**Input**: User description: "Create an F# wrapper for the current version of BepuPhysics2 for use in a custom data-oriented F# game engine"

## Clarifications

### Session 2026-03-05

- Q: How should contact events represent body-static collisions given that bodies and statics use different handle types? → A: Contact event includes both a body identifier and a separate optional static identifier field, so either side can be a body or a static.
- Q: When removing a body that is part of a constraint, should the wrapper auto-remove dependent constraints or return an error? → A: Auto-remove all constraints referencing the body before removing it.
- Q: When adding a body with zero or negative mass, should the wrapper reject or reinterpret? → A: Zero mass is treated as kinematic; negative mass is rejected with an error.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Initialize a Physics World (Priority: P1)

As a game engine developer, I want to create a physics simulation with sensible defaults (gravity, solver settings, material properties) using a single function call, so I can get a working physics world without understanding every BepuPhysics2 configuration detail.

**Why this priority**: This is the entry point for all physics functionality. Without it, nothing else works.

**Independent Test**: Create a physics world, step it once, verify it does not crash and returns a valid state.

**Acceptance Scenarios**:

1. **Given** no configuration, **When** I create a physics world with defaults, **Then** I get a world with Earth gravity (-9.81 Y), 8 velocity iterations, 1 substep, and a default buffer pool.
2. **Given** a custom config with gravity = (0, -20, 0) and substeps = 4, **When** I create a world, **Then** those values are reflected in the simulation.
3. **Given** a created world, **When** I destroy it, **Then** all native resources (buffer pool, thread dispatcher, simulation) are disposed.

---

### User Story 2 - Add and Remove Rigid Bodies (Priority: P1)

As a game engine developer, I want to add dynamic, kinematic, and static bodies to the simulation using simple descriptors, receiving opaque typed handles back, so I can manage physics entities in my ECS.

**Why this priority**: Bodies are the fundamental simulation objects -- everything depends on them.

**Independent Test**: Add a dynamic sphere, step the simulation, read back its position, verify it fell under gravity.

**Acceptance Scenarios**:

1. **Given** a physics world, **When** I add a dynamic body with shape=Sphere(1), mass=1, position=(0,5,0), **Then** I receive a body handle and the body exists in the simulation.
2. **Given** a physics world, **When** I add a static body with shape=Box(100,1,100) at origin, **Then** I receive a static handle.
3. **Given** a body handle, **When** I remove the body, **Then** the body is removed and the handle is invalidated.
4. **Given** a static handle, **When** I remove the static, **Then** the static is removed.

---

### User Story 3 - Read and Write Body State for ECS Integration (Priority: P1)

As a game engine developer, I want to read positions/orientations/velocities from physics bodies in bulk and write kinematic targets back, so my ECS transform system stays synchronized with physics.

**Why this priority**: The bridge between physics and rendering/gameplay is the most frequently called operation. It must be fast and data-oriented.

**Independent Test**: Add 1000 bodies, step once, bulk-read all positions into a flat array, verify they changed from initial values.

**Acceptance Scenarios**:

1. **Given** a body handle, **When** I read its pose, **Then** I receive a position and orientation.
2. **Given** a body handle, **When** I set its velocity, **Then** the body's linear and angular velocity are updated for the next step.
3. **Given** an array of body handles, **When** I bulk-read poses, **Then** I receive poses in the same order, populated without per-body allocation.
4. **Given** a kinematic body and a target pose, **When** I set its pose, **Then** the body teleports to that pose.

---

### User Story 4 - Define Shapes (Priority: P1)

As a game engine developer, I want to define physics shapes (sphere, box, capsule, cylinder, convex hull, mesh, compound) using a type-safe discriminated union, so shape creation is exhaustive and prevents invalid combinations.

**Why this priority**: Shapes are required to create any collidable body.

**Independent Test**: Create each shape variant, add it to the simulation, verify the returned shape identifier is valid.

**Acceptance Scenarios**:

1. **Given** a sphere shape definition with radius 1.0, **When** I add it to the world, **Then** I get a shape identifier and the shape is registered.
2. **Given** a box shape definition with dimensions (2, 1, 3), **When** I add it, **Then** the box dimensions are correct.
3. **Given** a convex hull shape defined by a point cloud, **When** I add it, **Then** a convex hull is computed from the points.
4. **Given** a compound shape with child offsets, **When** I add it, **Then** the compound shape is correctly assembled.
5. **Given** a shape identifier, **When** I remove the shape, **Then** it is deallocated (only if no bodies reference it).

---

### User Story 5 - Add Constraints Between Bodies (Priority: P1)

As a game engine developer, I want to connect bodies with constraints (joints) using a discriminated union of constraint descriptors, so I can build ragdolls, vehicles, and mechanisms.

**Why this priority**: Constraints are essential for any non-trivial physics scenario.

**Independent Test**: Create two bodies, connect them with a ball-socket constraint, step, verify they remain connected.

**Acceptance Scenarios**:

1. **Given** two body handles and a ball-socket constraint descriptor, **When** I add the constraint, **Then** I get a constraint identifier and the bodies are joined.
2. **Given** a constraint identifier, **When** I remove it, **Then** the constraint is removed.
3. **Given** a hinge constraint descriptor with angular limits, **When** I add it, **Then** the hinge axis and limits are respected during simulation.
4. **Given** a weld constraint, **When** I add it, **Then** the two bodies are rigidly attached.

---

### User Story 6 - Step the Simulation (Priority: P1)

As a game engine developer, I want to advance the simulation by a time delta, optionally with multithreading, so physics runs as part of my game loop.

**Why this priority**: Without stepping, the simulation is inert.

**Independent Test**: Step a world with a single falling sphere, verify position changes by the expected amount.

**Acceptance Scenarios**:

1. **Given** a world and dt=1/60, **When** I step the simulation, **Then** the simulation advances one fixed timestep.
2. **Given** a world and a thread count > 1, **When** I step, **Then** the simulation uses parallel execution.
3. **Given** a world configured for deterministic mode, **When** I step twice with identical state, **Then** the results are bit-identical.

---

### User Story 7 - Use the Library from F# Interactive (Priority: P1)

As a game engine developer, I want to load the wrapper in an FSI script via a single `#load` directive and interactively prototype physics scenarios, so I can experiment without a full build cycle.

**Why this priority**: The project constitution requires FSI scripting accessibility for all public APIs.

**Independent Test**: Run the prelude script in FSI, create a world, add a body, step, read pose -- all succeed without error.

**Acceptance Scenarios**:

1. **Given** the packed library output, **When** I load the prelude script, **Then** all wrapper modules are available with ergonomic helpers.
2. **Given** example scripts, **When** I run each one in FSI, **Then** they complete without error and demonstrate the documented scenario.

---

### User Story 8 - Collision Events / Contact Queries (Priority: P2)

As a game engine developer, I want to receive collision events (contact began, contact ended, contact persisted) as a flat buffer of event records after each step, so my ECS can process them without callbacks.

**Why this priority**: Game logic (damage, triggers, sound effects) depends on knowing what collided.

**Independent Test**: Drop a sphere onto a floor, step, verify a contact event is produced with correct body pair and contact normal.

**Acceptance Scenarios**:

1. **Given** a world after stepping, **When** I query contact events, **Then** I receive events with body pairs, normals, penetration depths, and event types (began/persisted/ended).
2. **Given** two bodies with collision filtering set to ignore each other, **When** they overlap, **Then** no contact event is produced.

---

### User Story 9 - Collision Filtering (Priority: P2)

As a game engine developer, I want to assign collision groups and masks to bodies so I can control which pairs generate contacts, using a simple layer-based system.

**Why this priority**: Most games need collision filtering (player vs environment, projectiles vs allies, etc.).

**Independent Test**: Create two bodies in the same non-colliding group, verify no contacts are generated.

**Acceptance Scenarios**:

1. **Given** a body with collisionGroup=1 and collisionMask targeting group 2, and another body with group=2 and mask targeting group 1, **When** they overlap, **Then** contacts are generated.
2. **Given** two bodies both in group=1 with masks excluding group 1, **When** they overlap, **Then** no contacts are generated.

---

### User Story 10 - Ray and Shape Casting (Priority: P2)

As a game engine developer, I want to cast rays and shapes against the simulation to query for hits, so I can implement line-of-sight, ground detection, and projectile trajectories.

**Why this priority**: Raycasting is the most common physics query after stepping.

**Independent Test**: Cast a ray downward above a static floor, verify it reports a hit with correct distance and normal.

**Acceptance Scenarios**:

1. **Given** a ray origin and direction, **When** I perform a raycast, **Then** I receive either a hit result with body identifier, distance, normal, and hit position, or no result if nothing was hit.
2. **Given** a ray that misses all bodies, **When** I perform a raycast, **Then** I receive no result.
3. **Given** a ray and a max distance, **When** I perform a multi-hit raycast, **Then** I receive all hits sorted by distance.

---

### User Story 11 - Material Properties (Priority: P2)

As a game engine developer, I want to define surface materials (friction, restitution-like spring settings) and assign them to bodies, so different surfaces behave differently on contact.

**Why this priority**: Without material variety, all surfaces feel identical.

**Independent Test**: Create a high-friction body and a low-friction body on a slope, verify the low-friction body slides faster.

**Acceptance Scenarios**:

1. **Given** material properties with specific friction and spring settings, **When** two bodies with these materials collide, **Then** the contact uses those parameters.
2. **Given** per-body material assignment, **When** contacts are generated, **Then** the correct material properties are applied for each body pair.

---

### User Story 12 - Debug Visualization Data (Priority: P3)

As a game engine developer, I want to extract shape wireframes, contact points, constraint anchors, and bounding boxes as vertex data, so my debug renderer can visualize physics state.

**Why this priority**: Essential for development but not for shipping.

**Independent Test**: Create a world with bodies and constraints, extract debug vertex data, verify it contains expected geometry.

**Acceptance Scenarios**:

1. **Given** a world with bodies, **When** I request debug visualization data, **Then** I receive vertex arrays representing shape wireframes, contact points, and bounding boxes.

---

### User Story 13 - Serialization / Snapshot (Priority: P3)

As a game engine developer, I want to snapshot the entire physics world state to a byte array and restore it, so I can implement save/load and network rollback.

**Why this priority**: Important for multiplayer and save systems but not the core simulation loop.

**Independent Test**: Create a world, add bodies, step, snapshot, restore, verify the restored world matches the original.

**Acceptance Scenarios**:

1. **Given** a populated physics world, **When** I snapshot it, **Then** I receive a byte array capturing the complete state.
2. **Given** a snapshot byte array, **When** I restore it, **Then** I get a physics world identical to the original at the time of snapshot.

---

### Edge Cases

- Adding a body with zero mass (treated as kinematic) or negative mass (rejected with an error)
- Removing a body that is part of a constraint (all dependent constraints are auto-removed before the body is removed)
- Stepping with dt<=0 (no-op; simulation does not advance)
- Creating shapes with degenerate dimensions (zero-radius sphere, zero-volume box)
- Using a handle after its body has been removed (should return a clear error, not crash)
- Buffer pool exhaustion under extreme body counts (must fail fast with actionable error context)
- Thread dispatcher with 0 or 1 threads (should fall back to single-threaded)

## Requirements *(mandatory)*

### Functional Requirements

#### Core Types

- **FR-001**: The wrapper MUST define body, static, shape, and constraint identifiers as opaque typed wrappers around underlying engine handles, preventing accidental handle misuse across entity types.
- **FR-002**: The wrapper MUST define a pose type as a value type with position (3D vector) and orientation (quaternion), directly compatible with the physics engine's internal representation.
- **FR-003**: The wrapper MUST define a velocity type as a value type with linear and angular components (3D vectors each).

#### Shape System

- **FR-004**: The wrapper MUST define a shape type as a discriminated union with cases for: sphere (radius), box (width/height/length), capsule (radius/length), cylinder (radius/length), triangle (three vertices), convex hull (point cloud), compound (child shapes with offsets), and mesh (triangle list).
- **FR-005**: The wrapper MUST compute inertia from shape geometry and mass automatically when creating dynamic bodies.

#### Body Descriptors

- **FR-006**: The wrapper MUST define a dynamic body descriptor with fields for: shape identifier, pose, velocity, mass, material properties, collision group, collision mask, sleep threshold, and continuous detection flag.
- **FR-007**: The wrapper MUST define a kinematic body descriptor with the same fields minus mass (implicit infinite mass).
- **FR-008**: The wrapper MUST define a static body descriptor with fields for: shape identifier, pose, material properties, collision group, and collision mask.
- **FR-009**: Sensible defaults MUST be provided for body descriptors so that users only need to specify shape, pose, and mass for the common case.

#### World Management

- **FR-010**: The physics world MUST encapsulate the simulation, memory management, threading, and internal state (material table, collision filter table, contact event buffer).
- **FR-011**: All world-mutating functions MUST follow idiomatic F# pipeline conventions (entity flowing as the last parameter).
- **FR-012**: The step function MUST advance the simulation by a given time delta. The wrapper acknowledges the simulation is inherently mutable and world functions mutate in place.

#### Callback System

- **FR-013**: The wrapper MUST provide default narrow phase and pose integration callbacks that support per-body material lookup, collision filtering via group/mask bitmask comparison, gravity and damping application, and contact event collection into a per-frame buffer.
- **FR-014**: Advanced users MUST be able to supply their own callback implementations, bypassing the wrapper's defaults entirely.

#### Constraints

- **FR-015**: The wrapper MUST define a constraint descriptor as a discriminated union covering at minimum: ball socket, hinge, weld, distance limit, distance spring, swing limit, twist limit, linear axis motor, angular motor, and point on line.
- **FR-016**: Each constraint case MUST carry the relevant parameters (offsets, axes, limits, spring settings) as value-type fields.
- **FR-017**: A spring configuration type MUST be defined with frequency and damping ratio fields.
- **FR-018**: Adding a constraint MUST require two body handles and a constraint descriptor and return a constraint identifier.

#### Queries

- **FR-019**: A single-hit raycast MUST be provided that takes origin, direction, and max distance, returning an optional hit result.
- **FR-020**: A multi-hit raycast MUST be provided that returns all hits sorted by distance.
- **FR-021**: A ray hit result MUST include the hit entity identifier, position, normal, and distance.

#### Bulk Operations (ECS-friendly)

- **FR-022**: Bulk pose reading MUST populate a pre-allocated array for all given handles, avoiding per-body allocation.
- **FR-023**: Bulk velocity reading MUST do the same for velocities.
- **FR-024**: Bulk pose writing MUST set poses for multiple bodies at once (for kinematic targets or teleportation).
- **FR-025**: Bulk velocity writing MUST set velocities for multiple bodies at once.

#### Contact Events

- **FR-026**: A contact event type MUST be defined as a value type with fields for: an optional body identifier and an optional static identifier for each of the two colliding entities (so body-body, body-static, and static-static contacts are all representable), plus contact normal, penetration depth, and event type (began/persisted/ended).
- **FR-027**: Querying contact events after a step MUST return all events from the most recent step.

#### Resource Management

- **FR-028**: The physics world MUST implement disposable semantics, disposing the simulation, thread dispatcher, and buffer pool in correct order.
- **FR-029**: The wrapper MUST NOT leak memory -- all shapes, bodies, and constraints removed via wrapper functions MUST properly release their resources.

#### Signature Files (Constitution II)

- **FR-030**: Every public module MUST have a corresponding signature file that declares its public API surface. Symbols omitted from signature files become module-private automatically.
- **FR-031**: Documentation comments MUST be authored in signature files, not in implementation files. Every public module, type, function, and discriminated union case MUST have a summary.
- **FR-032**: Surface-area baseline files MUST exist for each public module, validated in CI. Baseline changes require review.

#### Packaging

- **FR-033**: The project MUST be packable via standard tooling. Output package to local store for use by other local projects and scripts.

#### Scripting Accessibility (Constitution V)

- **FR-034**: The project MUST provide a prelude script that loads the compiled library with ergonomic helper functions, loadable via a single directive.
- **FR-035**: Numbered example scripts MUST accompany the prelude, covering core API scenarios end-to-end and remaining runnable against the latest build.

#### Observability (Constitution IV)

- **FR-036**: Operationally significant events (world creation, disposal, buffer pool exhaustion, invalid handle access, shape removal while referenced) MUST emit structured diagnostics with actionable context. Silent failures and swallowed exceptions are prohibited in critical paths.

#### Documentation (Constitution VI)

- **FR-037**: Documentation MUST be set up via FSharp.Formatting. A docs directory with literate scripts teaching library usage through executable examples MUST be provided.
- **FR-038**: Architecture overview and design decision records for key decisions (mutable world, discriminated union shapes, handle opacity, callback strategy) MUST be documented.

### Key Entities

- **PhysicsWorld**: The central simulation container. Holds the simulation, memory pool, threading, material table, collision filter table, and contact event buffer. Contains many bodies, statics, shapes, and constraints.
- **BodyId**: An opaque typed handle referencing one dynamic/kinematic body in the world. May have materials, shapes, and constraints.
- **StaticId**: An opaque typed handle referencing one static body in the world. Has a shape.
- **ShapeId**: An opaque typed identifier for a registered shape. Referenced by bodies and statics.
- **ConstraintId**: An opaque typed handle for a constraint connecting two bodies.
- **Pose**: Position (3D vector) and orientation (quaternion) of a body or static. Value type.
- **Velocity**: Linear velocity and angular velocity (3D vectors each) of a body. Value type.
- **MaterialProperties**: Surface properties including friction, max recovery velocity, spring frequency, and spring damping ratio. Assigned to bodies/statics.
- **ContactEvent**: Records a collision between two entities (body-body, body-static, or static-static), with optional body and static identifiers for each side, plus normal, depth, and event type (began/persisted/ended). Produced each step.
- **PhysicsShape**: Discriminated union of shape variants (sphere, box, capsule, cylinder, triangle, convex hull, compound, mesh). Registered in world, referenced by bodies.
- **ConstraintDesc**: Discriminated union of constraint types (ball socket, hinge, weld, distance limit, etc.) with per-case parameters. Connects two body handles.
- **PhysicsConfig**: Configuration for world creation including gravity, solver iterations, substep count, thread count, and deterministic mode flag.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can go from zero to a working physics simulation (sphere falling onto a floor with gravity) in under 10 lines of code, excluding import statements.
- **SC-002**: Bulk pose readback for 10,000 bodies completes in under 1ms on a modern desktop (no per-body allocation).
- **SC-003**: Single-threaded simulation step time for 10,000 bodies is within 5% of equivalent direct BepuPhysics2 usage (wrapper overhead is negligible).
- **SC-004**: The wrapper compiles with no warnings under strict warning settings including unused variable detection.
- **SC-005**: All public functions have documentation comments with summaries and usage examples.
- **SC-006**: The wrapper has zero managed heap allocations in the step-read-write hot path.
- **SC-007**: 100% of P1 user stories pass acceptance tests. 90%+ of P2 stories pass.
- **SC-008**: The wrapper is usable without reading BepuPhysics2 documentation for common scenarios (adding bodies, stepping, reading state, basic constraints).
- **SC-009**: Every public module has a corresponding signature file. The build enforces signature-to-implementation conformance.
- **SC-010**: Standard packaging produces a valid package to the local store.
- **SC-011**: The prelude script loads successfully in FSI and all numbered example scripts run without error against the latest build.
- **SC-012**: Documentation site builds without errors.

### Assumptions

- The target BepuPhysics2 version is 2.4.x (latest stable).
- The target runtime is .NET 8.0 or later.
- The wrapper is designed for a single-process game engine (not distributed physics).
- Collision filtering supports up to 32 layers via bitmask, which is sufficient for most games.
- The physics world is mutable (matching BepuPhysics2's design); the wrapper uses a functional-style API surface but acknowledges in-place mutation.
- Default material properties and solver settings follow BepuPhysics2 recommended defaults.
- Debug visualization (Story 12) and serialization (Story 13) are considered optional stretch goals.
