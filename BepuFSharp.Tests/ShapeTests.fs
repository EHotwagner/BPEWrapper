module BepuFSharp.Tests.ShapeTests

open System.Numerics
open Expecto
open BepuFSharp

[<Tests>]
let tests =
    testList "Shapes" [
        testCase "register Sphere returns valid ShapeId" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sid = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            Expect.isTrue (sid.Index >= 0) "Sphere index should be valid"

        testCase "register Box returns valid ShapeId" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sid = PhysicsWorld.addShape (PhysicsShape.Box(2.0f, 3.0f, 4.0f)) world
            Expect.isTrue (sid.Index >= 0) "Box index should be valid"

        testCase "register Capsule returns valid ShapeId" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sid = PhysicsWorld.addShape (PhysicsShape.Capsule(0.5f, 2.0f)) world
            Expect.isTrue (sid.Index >= 0) "Capsule index should be valid"

        testCase "register Cylinder returns valid ShapeId" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sid = PhysicsWorld.addShape (PhysicsShape.Cylinder(1.0f, 3.0f)) world
            Expect.isTrue (sid.Index >= 0) "Cylinder index should be valid"

        testCase "register Triangle returns valid ShapeId" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sid = PhysicsWorld.addShape (PhysicsShape.Triangle(Vector3.Zero, Vector3.UnitX, Vector3.UnitY)) world
            Expect.isTrue (sid.Index >= 0) "Triangle index should be valid"

        testCase "register ConvexHull returns valid ShapeId" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let points = [|
                Vector3(0.0f, 0.0f, 0.0f)
                Vector3(1.0f, 0.0f, 0.0f)
                Vector3(0.0f, 1.0f, 0.0f)
                Vector3(0.0f, 0.0f, 1.0f)
            |]
            let sid = PhysicsWorld.addShape (PhysicsShape.ConvexHull points) world
            Expect.isTrue (sid.Index >= 0) "ConvexHull index should be valid"

        testCase "degenerate sphere (zero radius) is rejected" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            Expect.throws
                (fun () -> PhysicsWorld.addShape (PhysicsShape.Sphere 0.0f) world |> ignore)
                "Zero-radius sphere should be rejected"

        testCase "degenerate box (zero dimension) is rejected" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            Expect.throws
                (fun () -> PhysicsWorld.addShape (PhysicsShape.Box(0.0f, 1.0f, 1.0f)) world |> ignore)
                "Zero-dimension box should be rejected"

        testCase "removeShape succeeds for unreferenced shape" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sid = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            PhysicsWorld.removeShape sid world

        testCase "different shape types get different TypeIds" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sphere = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let box = PhysicsWorld.addShape (PhysicsShape.Box(1.0f, 1.0f, 1.0f)) world
            Expect.notEqual sphere.TypeId box.TypeId "Sphere and Box should have different TypeIds"
    ]
