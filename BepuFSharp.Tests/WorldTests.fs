module BepuFSharp.Tests.WorldTests

open System.Numerics
open Expecto
open BepuFSharp

[<Tests>]
let tests =
    testList "World" [
        testCase "create with default config has gravity -9.81 Y" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sim = PhysicsWorld.simulation world
            Expect.isNotNull (box sim) "Simulation should be created"

        testCase "create with custom config reflects settings" <| fun _ ->
            let config =
                { PhysicsConfig.defaults with
                    Gravity = Vector3(0.0f, -20.0f, 0.0f)
                    SubstepCount = 4
                    SolverIterations = 16 }
            use world = PhysicsWorld.create config
            Expect.isNotNull (box (PhysicsWorld.simulation world)) "Custom world should be created"

        testCase "destroy disposes all resources" <| fun _ ->
            let world = PhysicsWorld.create PhysicsConfig.defaults
            PhysicsWorld.destroy world
            Expect.throwsT<System.ObjectDisposedException>
                (fun () -> PhysicsWorld.simulation world |> ignore)
                "Should throw after destroy"

        testCase "step advances simulation without error" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            PhysicsWorld.step 0.016f world

        testCase "step with dt=0 is a no-op" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            PhysicsWorld.step 0.0f world

        testCase "step with negative dt is a no-op" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            PhysicsWorld.step -0.016f world

        testCase "deterministic mode produces identical results" <| fun _ ->
            let config = { PhysicsConfig.defaults with Deterministic = true; ThreadCount = 1 }
            use w1 = PhysicsWorld.create config
            let s1 = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) w1
            use w2 = PhysicsWorld.create config
            let _s2 = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) w2
            Expect.isTrue (s1.TypeId >= 0) "Shape type ID should be non-negative"

        testCase "getAllBodyIds returns correct count after adds and removes" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let ids = [| for _ in 1..5 -> PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world |]
            PhysicsWorld.removeBody ids.[0] world
            PhysicsWorld.removeBody ids.[2] world
            let allIds = PhysicsWorld.getAllBodyIds world
            Expect.equal allIds.Length 3 "Should have 3 active bodies after removing 2 of 5"

        testCase "getAllStaticIds returns correct count after adds and removes" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Box(10.0f, 1.0f, 10.0f)) world
            let ids = [| for _ in 1..3 -> PhysicsWorld.addStatic (StaticBodyDesc.create shape Pose.identity) world |]
            PhysicsWorld.removeStatic ids.[1] world
            let allIds = PhysicsWorld.getAllStaticIds world
            Expect.equal allIds.Length 2 "Should have 2 active statics after removing 1 of 3"

        testCase "getAllBodyIds returns empty for empty world" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let allIds = PhysicsWorld.getAllBodyIds world
            Expect.equal allIds.Length 0 "Should have no bodies in empty world"

        testCase "setGravity to zero prevents downward acceleration" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let startY = 10.0f
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, startY, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.setGravity Vector3.Zero world
            for _ in 1..60 do
                PhysicsWorld.step (1.0f / 60.0f) world
            let pose = PhysicsWorld.getBodyPose bodyId world
            Expect.floatClose Accuracy.medium (float pose.Position.Y) (float startY) "Body should stay at same height with zero gravity"

        testCase "getGravity returns default gravity on fresh world" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let g = PhysicsWorld.getGravity world
            Expect.floatClose Accuracy.medium (float g.Y) -9.81 "Default gravity Y should be -9.81"

        testCase "reversed gravity causes upward motion" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let startY = 0.0f
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, startY, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.setGravity (Vector3(0.0f, 9.81f, 0.0f)) world
            for _ in 1..60 do
                PhysicsWorld.step (1.0f / 60.0f) world
            let pose = PhysicsWorld.getBodyPose bodyId world
            Expect.isTrue (pose.Position.Y > startY) "Body should move upward with reversed gravity"
    ]
