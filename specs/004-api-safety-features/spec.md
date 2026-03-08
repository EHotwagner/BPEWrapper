# Feature Specification: API Safety & Missing Features

**Feature Branch**: `004-api-safety-features`
**Created**: 2026-03-08
**Status**: Draft
**Input**: User description: "Implement the fixes described in BPImprovements.md — bugs, missing features, and dependency issues discovered while building FsBepuViewer on top of BepuFSharp 0.1.0."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Safe Body Access After Removal (Priority: P1)

A library consumer removes a physics body from the world and later attempts to read its pose or velocity — either due to stale references, delayed cleanup, or programmer error. Instead of crashing the process with an unrecoverable `AccessViolationException`, the library returns a safe result indicating the body no longer exists.

**Why this priority**: This is a crash-severity bug. An `AccessViolationException` terminates the process and cannot be caught. Any application holding stale `BodyId` references is at risk of silent, unrecoverable failure.

**Independent Test**: Create a body, remove it, then call each body accessor function. Verify no crash occurs and the appropriate safe result is returned.

**Acceptance Scenarios**:

1. **Given** a body has been removed from the world, **When** `tryGetBodyPose` is called with the removed body's ID, **Then** the function returns `ValueNone` without crashing.
2. **Given** a body has been removed from the world, **When** `tryGetBodyVelocity` is called with the removed body's ID, **Then** the function returns `ValueNone` without crashing.
3. **Given** a body has been removed from the world, **When** `trySetBodyPose` is called with the removed body's ID, **Then** the function returns `false` (or equivalent failure signal) without crashing.
4. **Given** a body has been removed from the world, **When** `trySetBodyVelocity` is called with the removed body's ID, **Then** the function returns `false` (or equivalent failure signal) without crashing.
5. **Given** a valid body exists in the world, **When** `bodyExists` is called with its ID, **Then** the function returns `true`.
6. **Given** a body has been removed, **When** `bodyExists` is called with its former ID, **Then** the function returns `false`.
7. **Given** a static body has been removed, **When** `staticExists` is called with its former ID, **Then** the function returns `false`.

---

### User Story 2 - Apply Forces and Impulses to Bodies (Priority: P1)

A library consumer needs to apply physics forces and impulses to bodies — for example, pushing objects via user input, simulating wind, or scripting interactions from the REPL. The library provides direct functions for this without requiring the escape hatch.

**Why this priority**: Force/impulse application is fundamental to interactive physics. Without it, consumers must use the raw escape hatch for the most basic interactive operations, defeating the purpose of the wrapper.

**Independent Test**: Create a body at rest, apply an impulse, step the simulation, and verify the body has moved in the expected direction.

**Acceptance Scenarios**:

1. **Given** a dynamic body at rest, **When** `applyImpulse` is called with a directional impulse and offset, **Then** the body gains both linear and angular velocity consistent with the impulse.
2. **Given** a dynamic body at rest, **When** `applyLinearImpulse` is called, **Then** the body gains linear velocity in the impulse direction without angular change.
3. **Given** a dynamic body at rest, **When** `applyAngularImpulse` is called, **Then** the body gains angular velocity without linear change.
4. **Given** a removed body, **When** any impulse function is called with its ID, **Then** the operation fails safely (no crash).

---

### User Story 3 - Enumerate All Active Bodies and Statics (Priority: P2)

A library consumer needs to discover all active body and static IDs in the world — for example, to render all objects, run a simulation inspector, or perform bulk operations without maintaining a separate tracking list.

**Why this priority**: Enumeration is essential for visualization and debugging tools. Without it, consumers must maintain a parallel tracking list that can drift out of sync with the physics world.

**Independent Test**: Add several bodies and statics, remove some, then call the enumeration functions. Verify the returned lists contain exactly the active IDs.

**Acceptance Scenarios**:

1. **Given** a world with 5 dynamic bodies (2 removed), **When** `getAllBodyIds` is called, **Then** exactly 3 body IDs are returned, all corresponding to active bodies.
2. **Given** a world with 3 statics (1 removed), **When** `getAllStaticIds` is called, **Then** exactly 2 static IDs are returned.
3. **Given** an empty world, **When** `getAllBodyIds` is called, **Then** an empty array is returned.

---

### User Story 4 - Runtime Gravity Modification (Priority: P2)

A library consumer needs to change gravity at runtime — for example, to simulate zero-G environments, gravity wells, or toggling gravity for gameplay mechanics — without restarting the physics world.

**Why this priority**: Runtime gravity changes are a common requirement for interactive physics applications. The current workaround (reflection via escape hatch) is fragile and version-dependent.

**Independent Test**: Create a world with default gravity, change gravity to zero, step a body, and verify it does not accelerate downward.

**Acceptance Scenarios**:

1. **Given** a world with default gravity, **When** `setGravity` is called with `Vector3.Zero`, **Then** subsequent simulation steps apply no gravitational acceleration.
2. **Given** a world with modified gravity, **When** `getGravity` is called, **Then** the currently active gravity vector is returned.
3. **Given** a world with default gravity and a body at rest at height Y, **When** gravity is reversed to `(0, +9.81, 0)` and the simulation is stepped, **Then** the body moves upward.

---

### User Story 5 - Query Body Shape (Priority: P3)

A library consumer needs to determine what shape is associated with a body — for example, to select the correct rendering mesh for visualization.

**Why this priority**: Shape queries are needed for visualization but consumers can work around this by maintaining their own shape-to-body mapping during creation.

**Independent Test**: Create bodies with different shapes, query each body's shape, and verify the returned shape matches what was used during creation.

**Acceptance Scenarios**:

1. **Given** a body created with a `Sphere(0.5f)` shape, **When** `getBodyShape` is called, **Then** `Some (Sphere 0.5f)` is returned.
2. **Given** a removed body, **When** `getBodyShape` is called, **Then** `None` is returned without crashing.
3. **Given** a body created with a `Box(1,2,3)` shape, **When** `getBodyShape` is called, **Then** the returned shape matches the original box dimensions.

---

### User Story 6 - Dependency Version Pinning (Priority: P2)

A library consumer uses BepuFSharp alongside other packages that also depend on BepuPhysics (e.g., Stride3D). NuGet must not silently upgrade BepuPhysics to an incompatible version, which causes runtime data corruption.

**Why this priority**: Silent version upgrades cause hard-to-diagnose runtime failures (incorrect `BodyExists` results, corrupted body data). This is a correctness issue that undermines trust in the library.

**Independent Test**: Inspect the package specification and verify the BepuPhysics dependency is pinned to an exact version that prevents unintended upgrades.

**Acceptance Scenarios**:

1. **Given** the BepuFSharp package is published, **When** a consumer's project also references a package depending on BepuPhysics 2.5.0-beta, **Then** NuGet does not silently upgrade BepuFSharp's BepuPhysics dependency beyond 2.4.x.

---

### User Story 7 - Shape Description for Debugging (Priority: P3)

A library consumer wants human-readable descriptions of physics shapes for logging, debugging output, and REPL exploration — e.g., `"Sphere(r=0.5)"` rather than the default F# DU string.

**Why this priority**: Nice-to-have for developer experience. The default F# `ToString()` output is functional but not optimized for readability.

**Independent Test**: Create each shape type and verify `describe` returns a readable, parameterized string.

**Acceptance Scenarios**:

1. **Given** a `Sphere(0.5f)` shape, **When** `describe` is called, **Then** the output includes the radius in a readable format (e.g., `"Sphere(r=0.5)"`).
2. **Given** a `Box(1,2,3)` shape, **When** `describe` is called, **Then** the output includes width, height, and length.

---

### Edge Cases

- What happens when `applyImpulse` is called on a kinematic body? (Kinematic bodies have infinite inertia in the physics engine — the function should no-op silently.)
- What happens when `getAllBodyIds` is called on a destroyed world? (Should follow the same behavior as other functions on destroyed worlds.)
- What happens when `setGravity` is called with `NaN` or infinity components? (Any Vector3 is accepted — invalid values are a caller error, consistent with BepuPhysics2's own behavior.)
- What happens when `getBodyShape` is called for a body whose shape was removed via `removeShape`? (The body still holds a shape index reference — should return the shape if retrievable, or `None`.)
- What happens when bulk operations (`readPoses`) include a mix of valid and removed body IDs? (Existing behavior — out of scope for this feature.)

## Requirements *(mandatory)*

### Functional Requirements

**Safe Body Access (Bug Fixes)**

- **FR-001**: The library MUST provide `bodyExists` and `staticExists` functions that check whether an ID refers to a currently active body or static, returning a boolean.
- **FR-002**: The library MUST provide `tryGetBodyPose` and `tryGetBodyVelocity` functions that return `ValueNone` for removed bodies instead of crashing.
- **FR-003**: The library MUST provide `trySetBodyPose` and `trySetBodyVelocity` functions that return a boolean success indicator for removed bodies instead of crashing.
- **FR-004**: The existing `getBodyPose`, `getBodyVelocity`, `setBodyPose`, and `setBodyVelocity` functions MUST remain available for backward compatibility.

**Force and Impulse Application**

- **FR-005**: The library MUST provide `applyImpulse` (with offset), `applyLinearImpulse`, and `applyAngularImpulse` functions that apply forces to dynamic bodies.
- **FR-006**: Impulse functions called on removed bodies MUST fail safely without crashing.

**Body and Static Enumeration**

- **FR-007**: The library MUST provide `getAllBodyIds` returning all active dynamic and kinematic body IDs.
- **FR-008**: The library MUST provide `getAllStaticIds` returning all active static body IDs.

**Runtime Gravity**

- **FR-009**: The library MUST provide `setGravity` and `getGravity` functions for modifying gravity at runtime without recreating the world.

**Shape Query**

- **FR-010**: The library MUST provide `getBodyShape` that returns the shape associated with a body, or `None` for removed bodies.

**Dependency Pinning**

- **FR-011**: The BepuPhysics and BepuUtilities package dependencies MUST be pinned to exact versions to prevent silent upgrades in consumer dependency graphs.

**Shape Description**

- **FR-012**: The library MUST provide a `describe` function for `PhysicsShape` that returns human-readable parameterized strings.

### Key Entities

- **BodyId**: Opaque handle to a dynamic or kinematic body; must now support existence checks.
- **StaticId**: Opaque handle to a static body; must now support existence checks.
- **PhysicsShape**: Discriminated union representing shape geometry; gains a description function and body-to-shape query path.
- **PhysicsWorld**: Central simulation container; gains gravity accessors, enumeration, and impulse functions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All body accessor functions (pose, velocity, impulse) can be called with a removed body ID without crashing the process.
- **SC-002**: Consumers can check body/static existence and receive a safe result in a single function call (no escape hatch required).
- **SC-003**: Consumers can apply impulses and forces to bodies without using the escape hatch.
- **SC-004**: Consumers can enumerate all active bodies and statics without maintaining a separate tracking list.
- **SC-005**: Consumers can modify gravity at runtime without reflection or escape hatch workarounds.
- **SC-006**: The library's physics engine dependency cannot be silently upgraded to an incompatible version by NuGet resolution.
- **SC-007**: All new functions have corresponding automated tests that verify both success and failure (removed-body) paths.

## Assumptions

- The `try*` variants use `voption` (value option) to match the existing codebase convention (e.g., `ContactEvent` fields use `voption`).
- The `try*` set functions return `bool` rather than `voption<unit>` for ergonomics.
- Impulse functions on kinematic bodies are a no-op (kinematic bodies have infinite mass in BepuPhysics2 and ignore applied forces).
- The `getBodyShape` function reconstructs a `PhysicsShape` DU case from the underlying Bepu shape data, which requires reading the shape type index from the body reference.
- Dependency pinning uses NuGet's exact version constraint syntax `[x.y.z]` in the project file.
- The existing non-try accessor functions (`getBodyPose`, etc.) are preserved as-is with no behavior change, for backward compatibility.
- Items 10 (typed error results) and 11 (async step) from BPImprovements.md are excluded from this feature scope as they represent larger architectural changes.
