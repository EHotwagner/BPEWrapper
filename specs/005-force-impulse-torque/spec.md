# Feature Specification: Force, Impulse, and Torque Application

**Feature Branch**: `005-force-impulse-torque`
**Created**: 2026-03-16
**Status**: Draft
**Input**: User description: "add applyforces/applyimpulse/applytorque to the wrapper"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Apply Instantaneous Impulse to a Body (Priority: P1)

A developer wants to simulate an explosion, projectile hit, or jump by applying an instantaneous impulse to a dynamic body. They call a single function with the body identifier and an impulse vector, and the body's velocity changes immediately according to its mass.

**Why this priority**: Impulses are the most common and fundamental way to affect bodies in gameplay — jumps, knockbacks, projectile impacts, and pickups all rely on instantaneous velocity changes. This is the highest-value capability.

**Independent Test**: Can be fully tested by creating a dynamic body at rest, applying a known impulse, stepping the simulation, and verifying the resulting velocity matches the expected value (impulse / mass).

**Acceptance Scenarios**:

1. **Given** a dynamic body at rest with known mass, **When** a linear impulse is applied, **Then** the body's linear velocity equals the impulse divided by its mass.
2. **Given** a sleeping dynamic body, **When** an impulse is applied, **Then** the body wakes up and its velocity changes accordingly.
3. **Given** a kinematic body, **When** an impulse is applied, **Then** the operation has no effect, since kinematic bodies have infinite mass.

---

### User Story 2 - Apply Continuous Force to a Body (Priority: P2)

A developer wants to simulate a thruster, wind, or magnetic attraction by applying a continuous force to a dynamic body each frame. They call a function with the body identifier and a force vector. The force is accumulated and integrated during the next simulation step, producing gradual acceleration.

**Why this priority**: Continuous forces enable environmental effects (wind, gravity zones, buoyancy) and player-controlled thrust. They are the second most common force-application pattern after impulses.

**Independent Test**: Can be fully tested by applying a known force to a body with known mass for one timestep and verifying the resulting velocity change equals force * dt / mass.

**Acceptance Scenarios**:

1. **Given** a dynamic body at rest with known mass, **When** a force is applied and the simulation steps by dt, **Then** the body's velocity change equals force * dt / mass.
2. **Given** a dynamic body with a force applied, **When** the simulation steps multiple times without reapplying the force, **Then** the force does not persist — it applies only for the step in which it was set.
3. **Given** a sleeping dynamic body, **When** a force is applied, **Then** the body wakes up and accelerates accordingly.

---

### User Story 3 - Apply Torque or Angular Impulse to a Body (Priority: P2)

A developer wants to spin a body — for example, applying rotational kick to a ball or spinning a wheel. They call a function with the body identifier and a torque or angular impulse vector. The body's angular velocity changes according to its rotational inertia.

**Why this priority**: Angular effects complete the force-application surface. Without torque/angular impulse, developers cannot create spinning projectiles, rotating platforms under force, or realistic collision responses with off-center hits.

**Independent Test**: Can be fully tested by creating a symmetric body (e.g., sphere) with known inertia, applying a known angular impulse, and verifying the resulting angular velocity matches expectations.

**Acceptance Scenarios**:

1. **Given** a dynamic body at rest, **When** an angular impulse is applied, **Then** the body's angular velocity changes according to its inverse inertia tensor.
2. **Given** a dynamic body at rest, **When** a continuous torque is applied and the simulation steps by dt, **Then** the angular velocity change is proportional to torque * dt scaled by inverse inertia.
3. **Given** a sleeping dynamic body, **When** a torque or angular impulse is applied, **Then** the body wakes up.

---

### User Story 4 - Apply Impulse at a Specific World Point (Priority: P3)

A developer wants to simulate a hit at a specific contact point on a body (e.g., shooting the edge of a box). They specify a world-space point and an impulse vector. The function decomposes this into both a linear impulse and an angular impulse based on the offset from the body's center of mass.

**Why this priority**: Point impulses are important for realistic physics interactions but can be approximated by combining linear and angular impulses manually. This is a convenience that builds on P1 and P2.

**Independent Test**: Can be fully tested by applying an impulse at an offset point on a body and verifying both linear and angular velocity change correctly based on the lever arm.

**Acceptance Scenarios**:

1. **Given** a dynamic body at rest, **When** an impulse is applied at the body's center of mass, **Then** only linear velocity changes (no angular effect).
2. **Given** a dynamic body at rest, **When** an impulse is applied at a point offset from center of mass, **Then** both linear and angular velocity change appropriately.

---

### Edge Cases

- What happens when a force/impulse is applied to a body that has been removed from the simulation?
- What happens when a zero-magnitude force or impulse is applied?
- How does the system handle extremely large force/impulse values that could cause numerical instability?
- What happens when force or impulse is applied to a static body?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The wrapper MUST provide a function to apply a linear impulse (instantaneous velocity change) to a dynamic body.
- **FR-002**: The wrapper MUST provide a function to apply a continuous force to a dynamic body, effective for one simulation step.
- **FR-003**: The wrapper MUST provide a function to apply an angular impulse (instantaneous angular velocity change) to a dynamic body.
- **FR-004**: The wrapper MUST provide a function to apply a continuous torque to a dynamic body, effective for one simulation step.
- **FR-005**: The wrapper MUST provide a function to apply an impulse at a specific world-space point on a dynamic body, decomposing it into linear and angular components.
- **FR-006**: All force/impulse/torque functions MUST automatically wake sleeping bodies.
- **FR-007**: All force/impulse/torque functions MUST handle invalid body identifiers consistently with the existing wrapper error semantics (e.g., `getBodyPose`, `setBodyVelocity`), where invalid handles propagate the underlying engine's exception.
- **FR-008**: Force/impulse/torque functions applied to kinematic or static bodies MUST have no effect.
- **FR-009**: The wrapper MUST support bulk application of forces/impulses to multiple bodies in a single call, consistent with the existing bulk read/write pattern.

### Key Entities

- **Force**: A continuous vector (direction + magnitude) applied to a body's center of mass, integrated over one timestep. Resets after each step.
- **Impulse**: An instantaneous vector change applied directly to a body's velocity, scaled by inverse mass.
- **Torque**: A continuous angular vector applied to a body, integrated over one timestep. Resets after each step.
- **Angular Impulse**: An instantaneous angular vector change applied directly to a body's angular velocity, scaled by inverse inertia.
- **Point Impulse**: A combination of impulse and angular impulse derived from applying force at an offset from center of mass.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer can apply an impulse to a body and observe correct velocity change in a single function call, with no manual velocity calculation required.
- **SC-002**: A developer can apply a continuous force and observe correct acceleration over a simulation step, with no manual integration required.
- **SC-003**: Torque and angular impulse produce correct rotational effects consistent with the body's inertia properties.
- **SC-004**: All new functions follow the same naming conventions, parameter patterns, and error-handling approach as existing wrapper functions (e.g., `getBodyPose`, `setBodyVelocity`).
- **SC-005**: Bulk force/impulse operations handle 1,000+ bodies without requiring per-body function calls from the consumer.
- **SC-006**: Applying forces/impulses to removed or invalid bodies behaves consistently with existing wrapper functions (throws on invalid handle, does not corrupt simulation state).

## Assumptions

- Forces and torques are single-step by default (not persistent across steps). Developers who need persistent forces reapply them each frame, which is the standard pattern in physics engines.
- The wrapper targets dynamic bodies only for force/impulse/torque. Kinematic and static bodies are unaffected by design.
- Inertia tensors are computed automatically from body shape and mass (as is already the case in the existing wrapper).
- The existing PhysicsWorld module is the correct location for these new functions, consistent with the current API design.

## Scope Boundaries

**In scope**:
- Linear force, linear impulse, angular impulse, torque, and point impulse application
- Single-body and bulk variants
- Wake-on-apply behavior
- Graceful handling of invalid/kinematic/static bodies

**Out of scope**:
- Persistent force fields or gravity overrides (these are callback-level concerns)
- Force generators or declarative force descriptions (e.g., "spring force between two bodies" — use constraints instead)
- Drag, damping, or friction modification (these are material/callback properties)
