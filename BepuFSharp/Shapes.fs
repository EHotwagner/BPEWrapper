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

[<RequireQualifiedAccess>]
module PhysicsShape =
    let describe (shape: PhysicsShape) : string =
        match shape with
        | PhysicsShape.Sphere radius ->
            sprintf "Sphere(r=%g)" radius
        | PhysicsShape.Box(width, height, length) ->
            sprintf "Box(w=%g, h=%g, l=%g)" width height length
        | PhysicsShape.Capsule(radius, length) ->
            sprintf "Capsule(r=%g, l=%g)" radius length
        | PhysicsShape.Cylinder(radius, length) ->
            sprintf "Cylinder(r=%g, l=%g)" radius length
        | PhysicsShape.Triangle(a, b, c) ->
            sprintf "Triangle((%g,%g,%g), (%g,%g,%g), (%g,%g,%g))" a.X a.Y a.Z b.X b.Y b.Z c.X c.Y c.Z
        | PhysicsShape.ConvexHull points ->
            sprintf "ConvexHull(%d points)" points.Length
        | PhysicsShape.Compound children ->
            sprintf "Compound(%d children)" children.Length
        | PhysicsShape.Mesh triangles ->
            sprintf "Mesh(%d triangles)" triangles.Length
