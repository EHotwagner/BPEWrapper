// Contract: New PhysicsWorld public API additions
// These signatures will be appended to BepuFSharp/PhysicsWorld.fsi

namespace BepuFSharp

open System.Numerics

module PhysicsWorld =

    // --- Existence checks (FR-001) ---

    /// Returns true if the body ID refers to an active body in the world.
    val bodyExists: BodyId -> PhysicsWorld -> bool

    /// Returns true if the static ID refers to an active static in the world.
    val staticExists: StaticId -> PhysicsWorld -> bool

    // --- Safe body accessors (FR-002, FR-003) ---

    /// Returns the pose of the body, or ValueNone if the body has been removed.
    val tryGetBodyPose: BodyId -> PhysicsWorld -> Pose voption

    /// Returns the velocity of the body, or ValueNone if the body has been removed.
    val tryGetBodyVelocity: BodyId -> PhysicsWorld -> Velocity voption

    /// Sets the pose of the body. Returns true if successful, false if the body has been removed.
    val trySetBodyPose: BodyId -> Pose -> PhysicsWorld -> bool

    /// Sets the velocity of the body. Returns true if successful, false if the body has been removed.
    val trySetBodyVelocity: BodyId -> Velocity -> PhysicsWorld -> bool

    // --- Impulse application (FR-005) ---

    /// Applies an impulse at the given offset from the body's center of mass.
    /// No-op if the body has been removed.
    val applyImpulse: BodyId -> impulse: Vector3 -> offset: Vector3 -> PhysicsWorld -> unit

    /// Applies a linear impulse to the body's center of mass.
    /// No-op if the body has been removed.
    val applyLinearImpulse: BodyId -> impulse: Vector3 -> PhysicsWorld -> unit

    /// Applies an angular impulse (torque) to the body.
    /// No-op if the body has been removed.
    val applyAngularImpulse: BodyId -> impulse: Vector3 -> PhysicsWorld -> unit

    // --- Body enumeration (FR-007, FR-008) ---

    /// Returns all active body IDs (dynamic and kinematic) in the world.
    val getAllBodyIds: PhysicsWorld -> BodyId[]

    /// Returns all active static IDs in the world.
    val getAllStaticIds: PhysicsWorld -> StaticId[]

    // --- Runtime gravity (FR-009) ---

    /// Sets the gravity vector for the physics world. Takes effect on the next step.
    val setGravity: Vector3 -> PhysicsWorld -> unit

    /// Returns the current gravity vector.
    val getGravity: PhysicsWorld -> Vector3

    // --- Shape query (FR-010) ---

    /// Returns the shape associated with a body, or None if the body has been removed.
    /// Note: ConvexHull, Compound, and Mesh shapes return simplified representations.
    val getBodyShape: BodyId -> PhysicsWorld -> PhysicsShape option
