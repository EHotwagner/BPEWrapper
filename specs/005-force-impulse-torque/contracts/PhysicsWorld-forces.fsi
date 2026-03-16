// Contract: New PhysicsWorld force/impulse/torque API surface
// This file defines the public API additions to the PhysicsWorld module.
// These signatures will be added to BepuFSharp/PhysicsWorld.fsi during implementation.

namespace BepuFSharp

open System.Numerics

// Additions to [<RequireQualifiedAccess>] module PhysicsWorld:

module PhysicsWorld =

    // --- Single-body impulse operations (P1) ---

    /// Apply a linear impulse to a dynamic body at its center of mass.
    /// The body's linear velocity changes by impulse * inverseMass.
    /// Automatically wakes sleeping bodies. No effect on kinematic bodies.
    val applyLinearImpulse: bodyId: BodyId -> impulse: Vector3 -> PhysicsWorld -> unit

    /// Apply an angular impulse to a dynamic body.
    /// The body's angular velocity changes by angularImpulse * inverseInertiaTensor.
    /// Automatically wakes sleeping bodies. No effect on kinematic bodies.
    val applyAngularImpulse: bodyId: BodyId -> angularImpulse: Vector3 -> PhysicsWorld -> unit

    /// Apply an impulse at a world-space offset from the body's center of mass.
    /// Produces both linear and angular velocity changes based on the lever arm.
    /// Automatically wakes sleeping bodies. No effect on kinematic bodies.
    val applyImpulseAtPoint: bodyId: BodyId -> impulse: Vector3 -> offset: Vector3 -> PhysicsWorld -> unit

    // --- Single-body force operations (P2) ---

    /// Apply a continuous force to a dynamic body for one timestep.
    /// Internally converts to impulse: linearImpulse = force * dt.
    /// Automatically wakes sleeping bodies. No effect on kinematic bodies.
    val applyForce: bodyId: BodyId -> force: Vector3 -> dt: float32 -> PhysicsWorld -> unit

    /// Apply a continuous torque to a dynamic body for one timestep.
    /// Internally converts to angular impulse: angularImpulse = torque * dt.
    /// Automatically wakes sleeping bodies. No effect on kinematic bodies.
    val applyTorque: bodyId: BodyId -> torque: Vector3 -> dt: float32 -> PhysicsWorld -> unit

    // --- Bulk operations (FR-009) ---

    /// Apply linear impulses to multiple dynamic bodies.
    /// ids and impulses arrays must have the same length.
    /// Automatically wakes sleeping bodies. Skips kinematic bodies.
    val applyLinearImpulses: ids: BodyId[] -> impulses: Vector3[] -> PhysicsWorld -> unit

    /// Apply angular impulses to multiple dynamic bodies.
    /// ids and angularImpulses arrays must have the same length.
    /// Automatically wakes sleeping bodies. Skips kinematic bodies.
    val applyAngularImpulses: ids: BodyId[] -> angularImpulses: Vector3[] -> PhysicsWorld -> unit

    /// Apply forces to multiple dynamic bodies for one timestep.
    /// ids and forces arrays must have the same length.
    /// Automatically wakes sleeping bodies. Skips kinematic bodies.
    val applyForces: ids: BodyId[] -> forces: Vector3[] -> dt: float32 -> PhysicsWorld -> unit
