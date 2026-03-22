# Feature Specification: Stride BepuPhysics Integration & Query Extensions

**Feature Branch**: `006-stride-bepu-integration`
**Created**: 2026-03-22
**Status**: Draft
**Input**: User description: "Stride BepuPhysics Integration & Query Extensions"

## User Scenarios & Testing

### User Story 1 - Sweep Cast Queries (Priority: P1)

A developer tests whether a shape moving along a path would collide with anything in the physics world, without actually moving it. This enables predictive collision detection for projectile paths, character movement validation, and obstacle avoidance.

**Why this priority**: Sweep casts are required by the upstream PhysicsSandbox feature for its SweepCast RPC. No workaround exists — raycasts only test infinitely thin lines, not volumetric shapes.

**Independent Test**: Can be fully tested by creating a world with known bodies, issuing a sweep cast with a sphere along a path, and verifying it reports the correct first hit body, position, normal, and distance.

**Acceptance Scenarios**:

1. **Given** a world with a static box, **When** a sphere is sweep-cast toward it, **Then** the first hit is returned with the correct contact point, normal, and distance.
2. **Given** a world with no obstacles along the sweep path, **When** a sweep cast is issued, **Then** no hit is returned.
3. **Given** a world with multiple bodies along the path, **When** a sweep cast is issued, **Then** only the closest hit is returned.
4. **Given** a sweep cast with a collision mask, **When** bodies on non-matching layers exist along the path, **Then** those bodies are ignored.

---

### User Story 2 - Overlap Queries (Priority: P1)

A developer tests which bodies overlap a given volume (shape at a position) without modifying the simulation. This enables area-of-effect detection, sensor zones, and proximity checks.

**Why this priority**: Overlap queries are required by the upstream PhysicsSandbox feature for its Overlap RPC. No workaround exists — raycasts and sweep casts test along lines, not volumes.

**Independent Test**: Can be fully tested by creating a world with known bodies, issuing an overlap query with a large sphere, and verifying it returns exactly the bodies that intersect the test volume.

**Acceptance Scenarios**:

1. **Given** a world with 5 bodies, 3 of which are inside a test sphere volume, **When** an overlap query is issued, **Then** exactly those 3 body/static IDs are returned.
2. **Given** a world with no bodies inside the test volume, **When** an overlap query is issued, **Then** an empty list is returned.
3. **Given** an overlap query with a collision mask, **When** bodies on non-matching layers exist inside the volume, **Then** those bodies are excluded from results.

---

### User Story 3 - Filtered Raycasting (Priority: P2)

A developer casts rays that respect collision layer filtering, so rays can ignore specific categories of bodies (e.g., cast through triggers, ignore ghost objects). Currently raycasts hit all bodies regardless of collision layers.

**Why this priority**: The existing raycast functions already work but always test against all bodies. Adding optional collision mask filtering is a small, backward-compatible enhancement that unlocks layer-aware queries.

**Independent Test**: Can be fully tested by creating bodies on different collision layers, casting a ray with a specific mask, and verifying only bodies matching the mask are returned.

**Acceptance Scenarios**:

1. **Given** bodies on layers 0 and 1, **When** a raycast is issued with a mask matching only layer 0, **Then** only layer-0 bodies are returned as hits.
2. **Given** a raycast without a collision mask (default), **When** cast through the scene, **Then** all bodies are hit (backward-compatible behavior).

---

### User Story 4 - Constraint Readback (Priority: P2)

A developer retrieves the current parameters and connected bodies of an existing constraint for serialization, debugging, or state inspection. Currently constraints can be created and removed but their state cannot be read back.

**Why this priority**: The upstream PhysicsSandbox needs to include constraint state in its simulation state stream. Without readback, constraint data cannot be serialized for viewers and clients.

**Independent Test**: Can be fully tested by creating a constraint, reading back its description, and verifying the returned parameters match what was provided at creation.

**Acceptance Scenarios**:

1. **Given** a hinge constraint was created between two bodies, **When** its description is read back, **Then** the returned constraint description matches the original parameters (axis, offsets, spring settings).
2. **Given** a constraint ID, **When** a constraint existence check is performed, **Then** it returns true for valid constraints and false for removed or invalid ones.
3. **Given** a constraint ID, **When** the connected bodies are queried, **Then** the two connected body identifiers are returned.

---

### User Story 5 - Runtime Filter and Material Modification (Priority: P3)

A developer changes a body's collision filter (group/mask) or material properties after creation, without removing and re-adding the body. This enables dynamic gameplay scenarios like toggling collision layers or changing surface friction at runtime.

**Why this priority**: The upstream PhysicsSandbox has a SetCollisionFilter command that requires runtime modification. Currently filters and materials are set only at body creation time.

**Independent Test**: Can be fully tested by creating a body, changing its collision mask at runtime, and verifying it no longer collides with bodies on the excluded layer.

**Acceptance Scenarios**:

1. **Given** a body on layer 0 colliding with a layer-0 wall, **When** its collision mask is changed to exclude layer 0, **Then** it passes through the wall on the next step.
2. **Given** a body with high friction, **When** its material friction is changed to zero, **Then** it slides freely on the next contact.
3. **Given** a static body, **When** its collision filter is changed, **Then** the change takes effect for subsequent collision checks.

---

### User Story 6 - Stride.BepuPhysics Type Interop (Priority: P3)

A developer converts between BepuFSharp types and Stride.BepuPhysics component types, enabling seamless integration when the wrapper is used inside a Stride3D application. This includes shape-to-collider, material, constraint, and collision filter conversions in both directions.

**Why this priority**: The upstream PhysicsSandbox viewer uses Stride3D and needs to map between BepuFSharp's types and Stride's component types. Without interop, every consumer must write its own conversion code.

**Independent Test**: Can be fully tested by round-tripping each type: BepuFSharp to Stride to BepuFSharp and verifying equivalence.

**Acceptance Scenarios**:

1. **Given** a BepuFSharp sphere shape with radius 0.5, **When** converted to a Stride collider, **Then** the result is a sphere collider with radius 0.5.
2. **Given** BepuFSharp material properties, **When** converted to Stride material properties and back, **Then** the values are preserved.
3. **Given** a BepuFSharp collision filter, **When** converted to a Stride collision group, **Then** the group and mask bits are preserved.
4. **Given** all 8 shape types, **When** each is converted to a Stride collider, **Then** the 7 supported types produce the correct Stride collider with matching dimensions, and Triangle raises an unsupported-shape error.

---

### Edge Cases

- What happens when a sweep cast shape is degenerate (zero radius sphere)? Return no hit.
- What happens when an overlap query shape has zero volume? Return empty results.
- What happens when constraint readback is called for a constraint that was just removed? Return None.
- What happens when runtime filter modification is called on a removed body? Raise an invalid body handle error.
- What happens when Stride interop converts a compound shape with nested shape references? Conversion works with pre-registered shapes only.

## Requirements

### Functional Requirements

**Sweep Cast**:
- **FR-001**: The physics world MUST expose a sweep cast function that tests a shape along a linear path and returns the first hit (body/static ID, position, normal, distance).
- **FR-002**: Sweep cast MUST accept an optional collision mask to filter which bodies are tested.
- **FR-003**: Sweep cast MUST support all 8 shape types as the swept shape.

**Overlap**:
- **FR-004**: The physics world MUST expose an overlap function that tests which bodies intersect a given shape at a given pose.
- **FR-005**: Overlap MUST return a list of body/static identifiers for all intersecting objects.
- **FR-006**: Overlap MUST accept an optional collision mask to filter results.

**Filtered Raycasting**:
- **FR-007**: The existing raycast and raycast-all functions MUST accept an optional collision mask parameter.
- **FR-008**: When no mask is provided, behavior MUST be identical to current behavior (all bodies tested) for backward compatibility.

**Constraint Readback**:
- **FR-009**: The physics world MUST expose a function to retrieve the constraint description for a given constraint identifier.
- **FR-010**: The physics world MUST expose a function to check whether a constraint exists, returning a boolean.
- **FR-011**: The physics world MUST expose a function to retrieve the two connected body identifiers for a given constraint.

**Runtime Modification**:
- **FR-012**: The physics world MUST expose a function to change a dynamic or kinematic body's collision group and mask after creation.
- **FR-013**: The physics world MUST expose a function to change a static body's collision filter after creation.
- **FR-014**: The physics world MUST expose functions to change material properties of dynamic, kinematic, and static bodies after creation.

**Stride Interop**:
- **FR-015**: A new interop module MUST provide bidirectional conversion between BepuFSharp shapes and Stride.BepuPhysics collider types.
- **FR-016**: The interop module MUST convert material properties, collision filters, and constraint descriptions.
- **FR-017**: Adding Stride.BepuPhysics as a transitive dependency MUST NOT break the existing API surface or build of consumers that do not use the interop module. Consumers are not required to reference Stride.BepuPhysics separately.

### Key Entities

- **SweepHit**: Result of a sweep cast — body/static ID (optional), position, normal, distance.
- **OverlapResult**: Result of an overlap query — body/static ID (optional).
- **StrideInterop**: Module providing type conversion functions between BepuFSharp and Stride.BepuPhysics types.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Sweep casts correctly detect the first collision along a path for all 8 shape types.
- **SC-002**: Overlap queries correctly identify all bodies intersecting a test volume.
- **SC-003**: Filtered raycasts exclude bodies on non-matching collision layers while maintaining backward compatibility for unfiltered calls.
- **SC-004**: Constraint descriptions can be read back and match the parameters provided at creation time for all constraint types.
- **SC-005**: Collision filters and material properties can be changed at runtime and take effect on the next simulation step.
- **SC-006**: All 7 shape types with direct Stride equivalents round-trip through BepuFSharp to Stride to BepuFSharp conversion without data loss. Triangle (no Stride equivalent) raises an explicit unsupported-shape error on conversion.
- **SC-007**: Existing tests continue to pass without modification (backward compatibility).
- **SC-008**: Package version is bumped to 0.2.0 and published to the local feed.

## Clarifications

### Session 2026-03-22

- Q: Should Stride interop be a separate package or bundled in the main BepuFSharp package? → A: Bundled in the same package. Stride.BepuPhysics is a required transitive dependency; consumers do not need to reference it separately. FR-017 means the existing API surface and build must remain intact, not that no new transitive dependencies are added.

## Assumptions

- Stride.BepuPhysics v4.3.0.2507 is compatible with the project's BepuPhysics 2.5.0-beta.28 dependency (minor beta version difference from Stride's 2.5.0-beta.25 requirement — verify at build time).
- The StrideInterop module adds Stride.BepuPhysics as a direct package dependency, making BepuFSharp a Stride-aware library. This is an intentional coupling per upstream decision.
- Sweep cast uses the physics engine's built-in sweep functionality with a sweep hit handler.
- Overlap uses the physics engine's broad phase overlap detection.
- Filtered raycasting modifies the existing allow-test callback in hit handlers to check collision masks.
- Constraint readback requires knowing the constraint type — the implementation will need to try each type or store the type alongside the handle.
