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

        testCase "getBodyShape returns Some Sphere for sphere body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            match PhysicsWorld.getBodyShape bodyId world with
            | Some (PhysicsShape.Sphere r) ->
                Expect.floatClose Accuracy.medium (float r) 0.5 "Radius should match"
            | other -> failtestf "Expected Some Sphere, got %A" other

        testCase "getBodyShape returns Some Box for box body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Box(1.0f, 2.0f, 3.0f)) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            match PhysicsWorld.getBodyShape bodyId world with
            | Some (PhysicsShape.Box(w, h, l)) ->
                Expect.floatClose Accuracy.medium (float w) 1.0 "Width should match"
                Expect.floatClose Accuracy.medium (float h) 2.0 "Height should match"
                Expect.floatClose Accuracy.medium (float l) 3.0 "Length should match"
            | other -> failtestf "Expected Some Box, got %A" other

        testCase "getBodyShape returns None for removed body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.removeBody bodyId world
            Expect.isNone (PhysicsWorld.getBodyShape bodyId world) "Should return None for removed body"

        testCase "describe Sphere formats correctly" <| fun _ ->
            let result = PhysicsShape.describe (PhysicsShape.Sphere 0.5f)
            Expect.stringContains result "Sphere" "Should contain Sphere"
            Expect.stringContains result "0.5" "Should contain radius"

        testCase "describe Box formats correctly" <| fun _ ->
            let result = PhysicsShape.describe (PhysicsShape.Box(1.0f, 2.0f, 3.0f))
            Expect.stringContains result "Box" "Should contain Box"
            Expect.stringContains result "w=" "Should contain width label"
            Expect.stringContains result "h=" "Should contain height label"
            Expect.stringContains result "l=" "Should contain length label"

        testCase "describe Capsule formats correctly" <| fun _ ->
            let result = PhysicsShape.describe (PhysicsShape.Capsule(0.3f, 1.2f))
            Expect.stringContains result "Capsule" "Should contain Capsule"
            Expect.stringContains result "0.3" "Should contain radius"

        testCase "describe Cylinder formats correctly" <| fun _ ->
            let result = PhysicsShape.describe (PhysicsShape.Cylinder(1.0f, 3.0f))
            Expect.stringContains result "Cylinder" "Should contain Cylinder"

        testCase "describe ConvexHull formats correctly" <| fun _ ->
            let result = PhysicsShape.describe (PhysicsShape.ConvexHull [| Vector3.Zero; Vector3.UnitX; Vector3.UnitY; Vector3.UnitZ |])
            Expect.stringContains result "ConvexHull" "Should contain ConvexHull"
            Expect.stringContains result "4" "Should contain point count"

        testCase "describe Compound formats correctly" <| fun _ ->
            let result = PhysicsShape.describe (PhysicsShape.Compound [||])
            Expect.stringContains result "Compound" "Should contain Compound"

        testCase "describe Mesh formats correctly" <| fun _ ->
            let tri = (Vector3.Zero, Vector3.UnitX, Vector3.UnitY)
            let result = PhysicsShape.describe (PhysicsShape.Mesh [| tri |])
            Expect.stringContains result "Mesh" "Should contain Mesh"
            Expect.stringContains result "1" "Should contain triangle count"

        testCase "describe Triangle formats correctly" <| fun _ ->
            let result = PhysicsShape.describe (PhysicsShape.Triangle(Vector3.Zero, Vector3.UnitX, Vector3.UnitY))
            Expect.stringContains result "Triangle" "Should contain Triangle"
    ]
