namespace BepuFSharp

open System.Numerics

/// <summary>
/// BepuFSharp — an idiomatic F# wrapper for BepuPhysics2 v2.
/// Provides typed handles, discriminated union shapes and constraints,
/// pipeline-style world management, bulk ECS operations,
/// collision events, raycasting, and per-body materials.
/// </summary>
[<System.Runtime.CompilerServices.CompilerGenerated>]
module NamespaceDoc = begin end

/// Opaque identifier for a dynamic or kinematic body in the physics world.
[<Struct>]
type BodyId = BodyId of int

/// Opaque identifier for a static body in the physics world.
[<Struct>]
type StaticId = StaticId of int

/// Opaque identifier for a registered shape, composed of a type discriminator and index.
[<Struct>]
type ShapeId =
    { /// Shape type discriminator.
      TypeId: int
      /// Index within the type-specific shape batch.
      Index: int }

/// Opaque identifier for a constraint connecting two bodies.
[<Struct>]
type ConstraintId = ConstraintId of int

/// Position and orientation in world space.
[<Struct>]
type Pose =
    { /// World-space position.
      Position: Vector3
      /// Rotation quaternion.
      Orientation: Quaternion }

/// Linear and angular velocity.
[<Struct>]
type Velocity =
    { /// Linear velocity in units per second.
      Linear: Vector3
      /// Angular velocity in radians per second.
      Angular: Vector3 }

/// Spring configuration for constraints and contacts.
[<Struct>]
type SpringConfig =
    { /// Oscillation frequency in Hz.
      Frequency: float32
      /// Damping ratio (1.0 = critically damped).
      DampingRatio: float32 }

/// Surface material properties controlling contact response behavior.
[<Struct>]
type MaterialProperties =
    { /// Coulomb friction coefficient.
      Friction: float32
      /// Maximum bounce recovery speed.
      MaxRecoveryVelocity: float32
      /// Contact spring frequency in Hz.
      SpringFrequency: float32
      /// Contact spring damping ratio.
      SpringDampingRatio: float32 }

/// Physics world configuration.
type PhysicsConfig =
    { /// World gravity vector.
      Gravity: Vector3
      /// Velocity solver iteration count per substep.
      SolverIterations: int
      /// Number of simulation substeps per step call.
      SubstepCount: int
      /// Worker thread count (0 = auto-detect).
      ThreadCount: int
      /// Enable deterministic simulation.
      Deterministic: bool }

/// Contact event classification.
type ContactEventType =
    /// First frame of contact.
    | Began
    /// Ongoing contact from previous frame.
    | Persisted
    /// Contact ceased this frame.
    | Ended

/// A contact event produced during simulation stepping.
[<Struct>]
type ContactEvent =
    { /// Body identifier for side A (ValueNone if static-only).
      BodyA: BodyId voption
      /// Static identifier for side A (ValueNone if body-only).
      StaticA: StaticId voption
      /// Body identifier for side B (ValueNone if static-only).
      BodyB: BodyId voption
      /// Static identifier for side B (ValueNone if body-only).
      StaticB: StaticId voption
      /// Contact normal from A to B.
      Normal: Vector3
      /// Penetration depth.
      Depth: float32
      /// Contact event classification.
      EventType: ContactEventType }

/// Result of a ray query against the physics world.
[<Struct>]
type RayHit =
    { /// Body hit (ValueNone if static was hit).
      Body: BodyId voption
      /// Static hit (ValueNone if body was hit).
      Static: StaticId voption
      /// World-space hit point.
      Position: Vector3
      /// Surface normal at hit point.
      Normal: Vector3
      /// Ray parameter at hit.
      Distance: float32 }

/// Motor configuration for motor-type constraints.
[<Struct>]
type MotorSettings =
    { /// Maximum force the motor can apply.
      MaxForce: float32
      /// Motor damping coefficient.
      Damping: float32 }

/// Collision filter controlling which body pairs generate contacts.
[<Struct>]
type CollisionFilter =
    { /// Collision layer (0-31).
      Group: uint32
      /// Collision layer bitmask.
      Mask: uint32 }

/// Functions for creating and working with Pose values.
[<RequireQualifiedAccess>]
module Pose =
    /// Create a pose from position and orientation.
    val create: position: Vector3 -> orientation: Quaternion -> Pose

    /// Create a pose from position with identity orientation.
    val ofPosition: position: Vector3 -> Pose

    /// The identity pose at the origin with no rotation.
    val identity: Pose

/// Functions for creating and working with Velocity values.
[<RequireQualifiedAccess>]
module Velocity =
    /// Zero velocity (no linear or angular motion).
    val zero: Velocity

    /// Create a velocity from linear and angular components.
    val create: linear: Vector3 -> angular: Vector3 -> Velocity

/// Functions for creating SpringConfig values.
[<RequireQualifiedAccess>]
module SpringConfig =
    /// Create a spring configuration from frequency and damping ratio.
    val create: frequency: float32 -> dampingRatio: float32 -> SpringConfig

/// Functions for creating MaterialProperties values.
[<RequireQualifiedAccess>]
module MaterialProperties =
    /// Default material properties (friction=1, maxRecovery=2, springFreq=30, springDamping=1).
    val defaults: MaterialProperties

    /// Create material properties with specific values.
    val create: friction: float32 -> maxRecoveryVelocity: float32 -> springFrequency: float32 -> springDampingRatio: float32 -> MaterialProperties

/// Functions for creating PhysicsConfig values.
[<RequireQualifiedAccess>]
module PhysicsConfig =
    /// Default configuration: gravity=(0,-9.81,0), 8 solver iterations, 1 substep, auto threads, non-deterministic.
    val defaults: PhysicsConfig
