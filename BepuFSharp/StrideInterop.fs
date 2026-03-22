namespace BepuFSharp

open System.Numerics
open Stride.BepuPhysics.Definitions.Colliders

[<RequireQualifiedAccess>]
module StrideInterop =

    let toStrideCollider (shape: PhysicsShape) : ColliderBase =
        match shape with
        | PhysicsShape.Sphere radius ->
            let c = SphereCollider()
            c.Radius <- radius
            c :> ColliderBase
        | PhysicsShape.Box(width, height, length) ->
            let c = BoxCollider()
            c.Size <- Vector3(width, height, length)
            c :> ColliderBase
        | PhysicsShape.Capsule(radius, length) ->
            let c = CapsuleCollider()
            c.Radius <- radius
            c.Length <- length
            c :> ColliderBase
        | PhysicsShape.Cylinder(radius, length) ->
            let c = CylinderCollider()
            c.Radius <- radius
            c.Length <- length
            c :> ColliderBase
        | PhysicsShape.Triangle(a, b, c) ->
            let toSV (v: Vector3) = Stride.Core.Mathematics.Vector3(v.X, v.Y, v.Z)
            let tc = TriangleCollider()
            tc.A <- toSV a
            tc.B <- toSV b
            tc.C <- toSV c
            tc :> ColliderBase
        | PhysicsShape.ConvexHull _ ->
            raise (System.NotSupportedException "ConvexHull conversion to Stride requires DecomposedHulls asset, not supported in pure conversion")
        | PhysicsShape.Compound _ ->
            raise (System.NotSupportedException "Compound conversion to Stride not supported in pure conversion")
        | PhysicsShape.Mesh _ ->
            raise (System.NotSupportedException "Mesh conversion to Stride requires Model asset, not supported in pure conversion")

    let fromStrideCollider (collider: ColliderBase) : PhysicsShape =
        match collider with
        | :? SphereCollider as c ->
            PhysicsShape.Sphere c.Radius
        | :? BoxCollider as c ->
            PhysicsShape.Box(c.Size.X, c.Size.Y, c.Size.Z)
        | :? CapsuleCollider as c ->
            PhysicsShape.Capsule(c.Radius, c.Length)
        | :? CylinderCollider as c ->
            PhysicsShape.Cylinder(c.Radius, c.Length)
        | :? TriangleCollider as c ->
            let toSNV (v: Stride.Core.Mathematics.Vector3) = Vector3(v.X, v.Y, v.Z)
            PhysicsShape.Triangle(toSNV c.A, toSNV c.B, toSNV c.C)
        | _ ->
            raise (System.NotSupportedException (sprintf "Unrecognized Stride collider type: %s" (collider.GetType().Name)))

    let toStrideCollisionLayer (filter: CollisionFilter) : Stride.BepuPhysics.CollisionLayer =
        LanguagePrimitives.EnumOfValue filter.Group

    let fromStrideCollisionLayer (layer: Stride.BepuPhysics.CollisionLayer) : CollisionFilter =
        let layerValue: uint32 = LanguagePrimitives.EnumToValue layer
        { Group = layerValue; Mask = 0xFFFFFFFFu }

    let applyMaterialToComponent (mat: MaterialProperties) (comp: Stride.BepuPhysics.CollidableComponent) : unit =
        comp.SpringFrequency <- mat.SpringFrequency
        comp.SpringDampingRatio <- mat.SpringDampingRatio
        comp.MaximumRecoveryVelocity <- mat.MaxRecoveryVelocity

    let readMaterialFromComponent (comp: Stride.BepuPhysics.CollidableComponent) : MaterialProperties =
        { Friction = 1.0f // Stride does not expose per-component friction
          MaxRecoveryVelocity = comp.MaximumRecoveryVelocity
          SpringFrequency = comp.SpringFrequency
          SpringDampingRatio = comp.SpringDampingRatio }
