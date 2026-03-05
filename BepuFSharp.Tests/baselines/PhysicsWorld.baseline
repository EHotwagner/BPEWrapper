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
