namespace BepuFSharp

open System.Numerics

[<Struct>]
type CompoundChild =
    { Shape: ShapeId
      LocalPose: Pose }

type PhysicsShape =
    | Sphere of radius: float32
    | Box of width: float32 * height: float32 * length: float32
    | Capsule of radius: float32 * length: float32
    | Cylinder of radius: float32 * length: float32
    | Triangle of a: Vector3 * b: Vector3 * c: Vector3
    | ConvexHull of points: Vector3[]
    | Compound of children: CompoundChild[]
    | Mesh of triangles: (Vector3 * Vector3 * Vector3)[]
