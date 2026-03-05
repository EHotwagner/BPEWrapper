namespace BepuFSharp

/// Description for creating a dynamic body.
[<Struct>]
type DynamicBodyDesc =
    { /// Registered shape for this body.
      Shape: ShapeId
      /// Initial position and orientation.
      Pose: Pose
      /// Initial velocity.
      Velocity: Velocity
      /// Mass in kg.
      Mass: float32
      /// Surface material properties.
      Material: MaterialProperties
      /// Collision layer (0-31).
      CollisionGroup: uint32
      /// Collision layer bitmask.
      CollisionMask: uint32
      /// Activity threshold for sleep.
      SleepThreshold: float32
      /// Enable continuous collision detection.
      ContinuousDetection: bool }

/// Description for creating a kinematic body (infinite mass).
[<Struct>]
type KinematicBodyDesc =
    { /// Registered shape for this body.
      Shape: ShapeId
      /// Initial position and orientation.
      Pose: Pose
      /// Initial velocity.
      Velocity: Velocity
      /// Surface material properties.
      Material: MaterialProperties
      /// Collision layer (0-31).
      CollisionGroup: uint32
      /// Collision layer bitmask.
      CollisionMask: uint32
      /// Activity threshold for sleep.
      SleepThreshold: float32
      /// Enable continuous collision detection.
      ContinuousDetection: bool }

/// Description for creating a static body.
[<Struct>]
type StaticBodyDesc =
    { /// Registered shape for this body.
      Shape: ShapeId
      /// Position and orientation.
      Pose: Pose
      /// Surface material properties.
      Material: MaterialProperties
      /// Collision layer (0-31).
      CollisionGroup: uint32
      /// Collision layer bitmask.
      CollisionMask: uint32 }

/// Functions for creating body descriptors.
[<RequireQualifiedAccess>]
module DynamicBodyDesc =
    /// Create a dynamic body descriptor with sensible defaults.
    val create: shape: ShapeId -> pose: Pose -> mass: float32 -> DynamicBodyDesc

/// Functions for creating kinematic body descriptors.
[<RequireQualifiedAccess>]
module KinematicBodyDesc =
    /// Create a kinematic body descriptor with sensible defaults.
    val create: shape: ShapeId -> pose: Pose -> KinematicBodyDesc

/// Functions for creating static body descriptors.
[<RequireQualifiedAccess>]
module StaticBodyDesc =
    /// Create a static body descriptor with sensible defaults.
    val create: shape: ShapeId -> pose: Pose -> StaticBodyDesc
