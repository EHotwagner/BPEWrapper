module BepuFSharp.Tests.StrideInteropTests

open System.Numerics
open Expecto
open BepuFSharp

[<Tests>]
let strideInteropTests =
    testList "StrideInterop" [
        testCase "Sphere round-trip" <| fun _ ->
            let shape = PhysicsShape.Sphere 0.5f
            let collider = StrideInterop.toStrideCollider shape
            let roundTripped = StrideInterop.fromStrideCollider collider
            match roundTripped with
            | PhysicsShape.Sphere r -> Expect.floatClose Accuracy.medium (float r) 0.5 "Radius"
            | _ -> failwith "Expected Sphere"

        testCase "Box round-trip" <| fun _ ->
            let shape = PhysicsShape.Box(1.0f, 2.0f, 3.0f)
            let collider = StrideInterop.toStrideCollider shape
            let roundTripped = StrideInterop.fromStrideCollider collider
            match roundTripped with
            | PhysicsShape.Box(w, h, l) ->
                Expect.floatClose Accuracy.medium (float w) 1.0 "Width"
                Expect.floatClose Accuracy.medium (float h) 2.0 "Height"
                Expect.floatClose Accuracy.medium (float l) 3.0 "Length"
            | _ -> failwith "Expected Box"

        testCase "Capsule round-trip" <| fun _ ->
            let shape = PhysicsShape.Capsule(0.3f, 1.5f)
            let collider = StrideInterop.toStrideCollider shape
            let roundTripped = StrideInterop.fromStrideCollider collider
            match roundTripped with
            | PhysicsShape.Capsule(r, l) ->
                Expect.floatClose Accuracy.medium (float r) 0.3 "Radius"
                Expect.floatClose Accuracy.medium (float l) 1.5 "Length"
            | _ -> failwith "Expected Capsule"

        testCase "Cylinder round-trip" <| fun _ ->
            let shape = PhysicsShape.Cylinder(0.4f, 2.0f)
            let collider = StrideInterop.toStrideCollider shape
            let roundTripped = StrideInterop.fromStrideCollider collider
            match roundTripped with
            | PhysicsShape.Cylinder(r, l) ->
                Expect.floatClose Accuracy.medium (float r) 0.4 "Radius"
                Expect.floatClose Accuracy.medium (float l) 2.0 "Length"
            | _ -> failwith "Expected Cylinder"

        testCase "Triangle round-trip" <| fun _ ->
            let shape = PhysicsShape.Triangle(Vector3(1.0f, 0.0f, 0.0f), Vector3(0.0f, 1.0f, 0.0f), Vector3(0.0f, 0.0f, 1.0f))
            let collider = StrideInterop.toStrideCollider shape
            let roundTripped = StrideInterop.fromStrideCollider collider
            match roundTripped with
            | PhysicsShape.Triangle(a, b, c) ->
                Expect.floatClose Accuracy.medium (float a.X) 1.0 "A.X"
                Expect.floatClose Accuracy.medium (float b.Y) 1.0 "B.Y"
                Expect.floatClose Accuracy.medium (float c.Z) 1.0 "C.Z"
            | _ -> failwith "Expected Triangle"

        testCase "ConvexHull toStride raises NotSupportedException" <| fun _ ->
            let shape = PhysicsShape.ConvexHull [| Vector3.Zero; Vector3.UnitX; Vector3.UnitY; Vector3.UnitZ |]
            Expect.throws
                (fun () -> StrideInterop.toStrideCollider shape |> ignore)
                "ConvexHull should raise"

        testCase "CollisionFilter round-trip via CollisionLayer" <| fun _ ->
            let filter = { Group = 3u; Mask = 0xFFFFFFFFu }
            let layer = StrideInterop.toStrideCollisionLayer filter
            let roundTripped = StrideInterop.fromStrideCollisionLayer layer
            Expect.equal roundTripped.Group 3u "Group should be preserved"

        testCase "applyMaterialToComponent and readMaterialFromComponent are callable" <| fun _ ->
            // CollidableComponent is abstract and requires Stride engine runtime to instantiate.
            // Verify the functions exist and have the correct types by checking compilation.
            let _applyFn: MaterialProperties -> Stride.BepuPhysics.CollidableComponent -> unit =
                StrideInterop.applyMaterialToComponent
            let _readFn: Stride.BepuPhysics.CollidableComponent -> MaterialProperties =
                StrideInterop.readMaterialFromComponent
            Expect.isTrue true "Material interop functions compile with correct signatures"
    ]
