namespace BepuFSharp

[<Struct>]
type DynamicBodyDesc =
    { Shape: ShapeId
      Pose: Pose
      Velocity: Velocity
      Mass: float32
      Material: MaterialProperties
      CollisionGroup: uint32
      CollisionMask: uint32
      SleepThreshold: float32
      ContinuousDetection: bool }

[<Struct>]
type KinematicBodyDesc =
    { Shape: ShapeId
      Pose: Pose
      Velocity: Velocity
      Material: MaterialProperties
      CollisionGroup: uint32
      CollisionMask: uint32
      SleepThreshold: float32
      ContinuousDetection: bool }

[<Struct>]
type StaticBodyDesc =
    { Shape: ShapeId
      Pose: Pose
      Material: MaterialProperties
      CollisionGroup: uint32
      CollisionMask: uint32 }

[<RequireQualifiedAccess>]
module DynamicBodyDesc =
    let create (shape: ShapeId) (pose: Pose) (mass: float32) : DynamicBodyDesc =
        { Shape = shape
          Pose = pose
          Velocity = Velocity.zero
          Mass = mass
          Material = MaterialProperties.defaults
          CollisionGroup = 0u
          CollisionMask = 0xFFFFFFFFu
          SleepThreshold = 0.01f
          ContinuousDetection = false }

[<RequireQualifiedAccess>]
module KinematicBodyDesc =
    let create (shape: ShapeId) (pose: Pose) : KinematicBodyDesc =
        { Shape = shape
          Pose = pose
          Velocity = Velocity.zero
          Material = MaterialProperties.defaults
          CollisionGroup = 0u
          CollisionMask = 0xFFFFFFFFu
          SleepThreshold = 0.01f
          ContinuousDetection = false }

[<RequireQualifiedAccess>]
module StaticBodyDesc =
    let create (shape: ShapeId) (pose: Pose) : StaticBodyDesc =
        { Shape = shape
          Pose = pose
          Material = MaterialProperties.defaults
          CollisionGroup = 0u
          CollisionMask = 0xFFFFFFFFu }
