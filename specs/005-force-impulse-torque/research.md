# Research: Force, Impulse, and Torque Application

**Branch**: `005-force-impulse-torque` | **Date**: 2026-03-16

## R1: BepuPhysics2 Impulse API Surface

**Decision**: Use `BodyReference.ApplyLinearImpulse`, `ApplyAngularImpulse`, and `ApplyImpulse` directly from the underlying BepuPhysics2 API.

**Rationale**: BepuPhysics2 v2.5.0-beta.28 provides first-class impulse methods on `BodyReference`:
- `ApplyLinearImpulse(Vector3 impulse)` — modifies linear velocity by `impulse * inverseMass`
- `ApplyAngularImpulse(Vector3 angularImpulse)` — modifies angular velocity by `angularImpulse * inverseInertiaTensor`
- `ApplyImpulse(Vector3 impulse, Vector3 impulseOffset)` — decomposes into linear + angular based on offset from center of mass

These methods handle the physics math (inverse mass, inverse inertia tensor transformation) internally, which is exactly what the wrapper needs to abstract.

**Alternatives considered**:
- Manual velocity arithmetic in the wrapper: rejected because it duplicates BepuPhysics2's optimized inertia-aware math and risks correctness errors with non-trivial inertia tensors.
- Custom force accumulator callback: rejected because it requires modifying `IPoseIntegratorCallbacks`, which is more invasive. Impulse-based approach achieves the same result without callback changes.

## R2: Force Application Strategy (Continuous Forces)

**Decision**: Implement continuous forces as impulses scaled by dt — i.e., `applyForce` internally calls `ApplyLinearImpulse(force * dt)`. The wrapper does NOT need to store force accumulators or modify the pose integrator callbacks.

**Rationale**: BepuPhysics2's impulse API is the canonical way to apply external forces. The relationship is `impulse = force * dt`. By having the wrapper accept a `dt` parameter (or using the world's configured timestep), continuous forces become a thin convenience over impulses. This avoids:
- Persistent state management (force accumulator dictionaries)
- Thread-safety concerns with concurrent force application
- Callback modification that would break the wrapper's sealed callback design

**Alternatives considered**:
- Per-body force accumulator dictionary cleared each step: rejected because it adds mutable state management, requires coordination with the step function, and provides no benefit over the impulse-scaled approach.
- Modify `DefaultPoseIntegratorCallbacks.IntegrateVelocity` to read per-body forces: rejected because it's invasive, requires Wide/SIMD per-body lookups inside the hot integration loop, and couples force application to the callback architecture.

## R3: Wake-on-Apply Behavior

**Decision**: Call `simulation.Awakener.AwakenBody(handle)` before applying impulses to sleeping bodies.

**Rationale**: BepuPhysics2's impulse methods do NOT automatically wake sleeping bodies. The wrapper must explicitly wake bodies before applying forces/impulses. The `Awakener.AwakenBody` method handles island-aware waking (wakes connected constrained bodies too).

**Alternatives considered**:
- Check `bodyRef.Awake` first and only wake if sleeping: this is a minor optimization but adds branching. Since `AwakenBody` is a no-op for already-awake bodies, unconditional waking is simpler and safe.
- Set `bodyRef.Awake = true`: this also works but `Awakener.AwakenBody` is the documented canonical approach and handles island propagation.

## R4: Kinematic Body Detection

**Decision**: Check `bodyRef.Kinematic` property to detect kinematic bodies and skip impulse application silently.

**Rationale**: Kinematic bodies have `InverseMass = 0`, so impulse methods would have no effect anyway. However, explicitly checking and skipping is cleaner — it avoids the unnecessary wake call and makes the no-op behavior intentional rather than incidental.

## R5: Bulk Operations Pattern

**Decision**: Follow the existing `readPoses`/`writePoses` array-based pattern: `applyImpulses(ids: BodyId[], impulses: Vector3[], world)`.

**Rationale**: The existing wrapper uses parallel arrays (`BodyId[]` + data `[]`) for all bulk operations. This is zero-allocation, cache-friendly, and consistent with the established API surface.

**Alternatives considered**:
- Span-based API: rejected because the current wrapper doesn't use Spans anywhere, and F# Span ergonomics are still limited.
- Struct tuple array `(BodyId * Vector3)[]`: rejected for consistency with existing parallel-array pattern.

## R6: Error Handling for Invalid Bodies

**Decision**: Follow the existing pattern — let BepuPhysics2 throw on invalid handles, which the wrapper does not catch. This is consistent with how `getBodyPose`, `setBodyVelocity`, etc. work today.

**Rationale**: The existing wrapper functions (e.g., `getBodyPose`, `setBodyVelocity`) access `sim.Bodies.[handle]` without bounds checking. Adding special error handling only for force/impulse functions would be inconsistent. The `ThrowIfDisposed()` guard is the only safety check applied.

**Alternatives considered**:
- Add `Bodies.BodyExists(handle)` check: rejected for consistency with the rest of the API. If the project later wants safe-by-default access, it should be a cross-cutting change.

## R7: dt Parameter for Force Functions

**Decision**: Require `dt: float32` as an explicit parameter to `applyForce` and `applyTorque`, rather than storing it on the world or inferring it.

**Rationale**: The `PhysicsWorld` type does not store the last timestep, and inferring it would require mutable state. An explicit `dt` parameter is pure, testable, and gives the caller full control (important for fixed-timestep vs variable-timestep games).

**Alternatives considered**:
- Store `lastDt` on PhysicsWorld: rejected because it adds mutable state to a type that currently only has `disposed` as mutable.
- Omit force functions entirely, only provide impulse: considered but rejected — `applyForce` is a common enough pattern that the convenience wrapper is worth providing.
