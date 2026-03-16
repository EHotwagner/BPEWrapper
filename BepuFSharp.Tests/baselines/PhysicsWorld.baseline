namespace BepuFSharp

open System
open System.Numerics
open BepuPhysics
open BepuUtilities.Memory

/// A mutable physics world wrapping a BepuPhysics2 Simulation.
[<Sealed>]
type PhysicsWorld =
    interface IDisposable

/// Functions for creating and managing physics worlds.
[<RequireQualifiedAccess>]
module PhysicsWorld =
    /// Create a new physics world with the given configuration.
    val create: PhysicsConfig -> PhysicsWorld

    /// Destroy a physics world and release all resources.
    val destroy: PhysicsWorld -> unit

    /// Advance the simulation by the given time step in seconds.
    val step: dt: float32 -> PhysicsWorld -> unit

    /// Register a shape with the simulation and return its identifier.
    val addShape: PhysicsShape -> PhysicsWorld -> ShapeId

    /// Remove a shape from the simulation.
    val removeShape: ShapeId -> PhysicsWorld -> unit

    /// Get the current pose of a body.
    val getBodyPose: BodyId -> PhysicsWorld -> Pose

    /// Get the current velocity of a body.
    val getBodyVelocity: BodyId -> PhysicsWorld -> Velocity

    /// Set the pose of a body.
    val setBodyPose: BodyId -> Pose -> PhysicsWorld -> unit

    /// Set the velocity of a body.
    val setBodyVelocity: BodyId -> Velocity -> PhysicsWorld -> unit

    /// Bulk read poses for multiple bodies into a pre-allocated array.
    val readPoses: BodyId[] -> Pose[] -> PhysicsWorld -> unit

    /// Bulk read velocities for multiple bodies into a pre-allocated array.
    val readVelocities: BodyId[] -> Velocity[] -> PhysicsWorld -> unit

    /// Bulk write poses for multiple bodies from a source array.
    val writePoses: BodyId[] -> Pose[] -> PhysicsWorld -> unit

    /// Bulk write velocities for multiple bodies from a source array.
    val writeVelocities: BodyId[] -> Velocity[] -> PhysicsWorld -> unit

    /// Add a dynamic body to the simulation.
    val addBody: DynamicBodyDesc -> PhysicsWorld -> BodyId

    /// Add a kinematic body to the simulation.
    val addKinematicBody: KinematicBodyDesc -> PhysicsWorld -> BodyId

    /// Add a static body to the simulation.
    val addStatic: StaticBodyDesc -> PhysicsWorld -> StaticId

    /// Remove a dynamic or kinematic body (auto-removes constraints).
    val removeBody: BodyId -> PhysicsWorld -> unit

    /// Remove a static body.
    val removeStatic: StaticId -> PhysicsWorld -> unit

    /// Add a constraint between two bodies.
    val addConstraint: BodyId -> BodyId -> ConstraintDesc -> PhysicsWorld -> ConstraintId

    /// Remove a constraint.
    val removeConstraint: ConstraintId -> PhysicsWorld -> unit

    /// Cast a ray and return the closest hit.
    val raycast: origin: Vector3 -> direction: Vector3 -> maxDistance: float32 -> PhysicsWorld -> RayHit option

    /// Cast a ray and return all hits sorted by distance.
    val raycastAll: origin: Vector3 -> direction: Vector3 -> maxDistance: float32 -> PhysicsWorld -> RayHit[]

    /// Get contact events from the last completed simulation step.
    val getContactEvents: PhysicsWorld -> ContactEvent[]

    /// Get the underlying BepuPhysics2 Simulation (escape hatch).
    val simulation: PhysicsWorld -> Simulation

    /// Get the underlying BufferPool (escape hatch).
    val bufferPool: PhysicsWorld -> BufferPool

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

    /// Apply a continuous force to a dynamic body for one timestep.
    /// Internally converts to impulse: linearImpulse = force * dt.
    /// Automatically wakes sleeping bodies. No effect on kinematic bodies.
    val applyForce: bodyId: BodyId -> force: Vector3 -> dt: float32 -> PhysicsWorld -> unit

    /// Apply a continuous torque to a dynamic body for one timestep.
    /// Internally converts to angular impulse: angularImpulse = torque * dt.
    /// Automatically wakes sleeping bodies. No effect on kinematic bodies.
    val applyTorque: bodyId: BodyId -> torque: Vector3 -> dt: float32 -> PhysicsWorld -> unit

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
