namespace BepuFSharp

open System.Numerics

/// A child shape within a compound shape.
[<Struct>]
type CompoundChild =
    { /// Registered shape for this child.
      Shape: ShapeId
      /// Offset from compound origin.
      LocalPose: Pose }

/// Physics shape variants that can be registered with the simulation.
type PhysicsShape =
    /// Sphere with the given radius.
    | Sphere of radius: float32
    /// Axis-aligned box with full width, height, and length dimensions.
    | Box of width: float32 * height: float32 * length: float32
    /// Capsule with the given radius and length.
    | Capsule of radius: float32 * length: float32
    /// Cylinder with the given radius and length.
    | Cylinder of radius: float32 * length: float32
    /// Single triangle defined by three vertices.
    | Triangle of a: Vector3 * b: Vector3 * c: Vector3
    /// Convex hull computed from a point cloud (minimum 4 points).
    | ConvexHull of points: Vector3[]
    /// Compound shape composed of child shapes with local offsets.
    | Compound of children: CompoundChild[]
    /// Triangle mesh shape.
    | Mesh of triangles: (Vector3 * Vector3 * Vector3)[]

/// Functions for working with physics shapes.
[<RequireQualifiedAccess>]
module PhysicsShape =
    /// Return a human-readable description of the shape with its parameters.
    val describe: PhysicsShape -> string
