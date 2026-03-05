namespace BepuFSharp

open System.Numerics

[<Struct>]
type BodyId = BodyId of int

[<Struct>]
type StaticId = StaticId of int

[<Struct>]
type ShapeId = { TypeId: int; Index: int }

[<Struct>]
type ConstraintId = ConstraintId of int

[<Struct>]
type Pose = { Position: Vector3; Orientation: Quaternion }

[<Struct>]
type Velocity = { Linear: Vector3; Angular: Vector3 }

[<Struct>]
type SpringConfig = { Frequency: float32; DampingRatio: float32 }

[<Struct>]
type MaterialProperties =
    { Friction: float32
      MaxRecoveryVelocity: float32
      SpringFrequency: float32
      SpringDampingRatio: float32 }

type PhysicsConfig =
    { Gravity: Vector3
      SolverIterations: int
      SubstepCount: int
      ThreadCount: int
      Deterministic: bool }

type ContactEventType =
    | Began
    | Persisted
    | Ended

[<Struct>]
type ContactEvent =
    { BodyA: BodyId voption
      StaticA: StaticId voption
      BodyB: BodyId voption
      StaticB: StaticId voption
      Normal: Vector3
      Depth: float32
      EventType: ContactEventType }

[<Struct>]
type RayHit =
    { Body: BodyId voption
      Static: StaticId voption
      Position: Vector3
      Normal: Vector3
      Distance: float32 }

[<Struct>]
type MotorSettings = { MaxForce: float32; Damping: float32 }

[<Struct>]
type CollisionFilter = { Group: uint32; Mask: uint32 }

[<RequireQualifiedAccess>]
module Pose =
    let create (position: Vector3) (orientation: Quaternion) : Pose =
        { Position = position; Orientation = orientation }

    let ofPosition (position: Vector3) : Pose =
        { Position = position; Orientation = Quaternion.Identity }

    let identity: Pose =
        { Position = Vector3.Zero; Orientation = Quaternion.Identity }

[<RequireQualifiedAccess>]
module Velocity =
    let zero: Velocity =
        { Linear = Vector3.Zero; Angular = Vector3.Zero }

    let create (linear: Vector3) (angular: Vector3) : Velocity =
        { Linear = linear; Angular = angular }

[<RequireQualifiedAccess>]
module SpringConfig =
    let create (frequency: float32) (dampingRatio: float32) : SpringConfig =
        { Frequency = frequency; DampingRatio = dampingRatio }

[<RequireQualifiedAccess>]
module MaterialProperties =
    let defaults: MaterialProperties =
        { Friction = 1.0f
          MaxRecoveryVelocity = 2.0f
          SpringFrequency = 30.0f
          SpringDampingRatio = 1.0f }

    let create (friction: float32) (maxRecoveryVelocity: float32) (springFrequency: float32) (springDampingRatio: float32) : MaterialProperties =
        { Friction = friction
          MaxRecoveryVelocity = maxRecoveryVelocity
          SpringFrequency = springFrequency
          SpringDampingRatio = springDampingRatio }

[<RequireQualifiedAccess>]
module PhysicsConfig =
    let defaults: PhysicsConfig =
        { Gravity = Vector3(0.0f, -9.81f, 0.0f)
          SolverIterations = 8
          SubstepCount = 1
          ThreadCount = 0
          Deterministic = false }
