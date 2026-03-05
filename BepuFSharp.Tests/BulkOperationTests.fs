module BepuFSharp.Tests.BulkOperationTests

open System.Numerics
open Expecto
open BepuFSharp

[<Tests>]
let tests =
    testList "BulkOperations" [
        testCase "getBodyPose returns correct position after step" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let startPos = Vector3(0.0f, 10.0f, 0.0f)
            let body = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition startPos) 1.0f) world
            PhysicsWorld.step (1.0f / 60.0f) world
            let pose = PhysicsWorld.getBodyPose body world
            Expect.isTrue (pose.Position.Y < startPos.Y) "Body should fall under gravity"

        testCase "setBodyVelocity updates velocity for next step" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let body = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            PhysicsWorld.setBodyVelocity body (Velocity.create (Vector3(5.0f, 0.0f, 0.0f)) Vector3.Zero) world
            PhysicsWorld.step (1.0f) world
            let pose = PhysicsWorld.getBodyPose body world
            Expect.isGreaterThan pose.Position.X 0.0f "Body should have moved in X"

        testCase "pose round-trip preserves values" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let body = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let testPose = Pose.create (Vector3(3.0f, 4.0f, 5.0f)) Quaternion.Identity
            PhysicsWorld.setBodyPose body testPose world
            let read = PhysicsWorld.getBodyPose body world
            Expect.floatClose Accuracy.medium (float read.Position.X) 3.0 "X should match"
            Expect.floatClose Accuracy.medium (float read.Position.Y) 4.0 "Y should match"
            Expect.floatClose Accuracy.medium (float read.Position.Z) 5.0 "Z should match"

        testCase "readPoses populates array in correct order" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let bodies = Array.init 100 (fun i ->
                let pos = Vector3(float32 i, 0.0f, 0.0f)
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition pos) 1.0f) world)
            let poses = Array.zeroCreate<Pose> 100
            PhysicsWorld.readPoses bodies poses world
            for i in 0 .. 99 do
                Expect.floatClose Accuracy.medium (float poses.[i].Position.X) (float i) $"Body %d{i} X should match"

        testCase "readVelocities bulk read works" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let body = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            PhysicsWorld.setBodyVelocity body (Velocity.create (Vector3(1.0f, 2.0f, 3.0f)) Vector3.Zero) world
            let velocities = Array.zeroCreate<Velocity> 1
            PhysicsWorld.readVelocities [| body |] velocities world
            Expect.floatClose Accuracy.medium (float velocities.[0].Linear.X) 1.0 "Vx should match"

        testCase "writePoses teleports bodies" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let body = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let newPose = Pose.ofPosition (Vector3(99.0f, 0.0f, 0.0f))
            PhysicsWorld.writePoses [| body |] [| newPose |] world
            let read = PhysicsWorld.getBodyPose body world
            Expect.floatClose Accuracy.medium (float read.Position.X) 99.0 "Should be teleported"

        testCase "writeVelocities bulk set works" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let body = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            PhysicsWorld.writeVelocities [| body |] [| Velocity.create (Vector3(7.0f, 0.0f, 0.0f)) Vector3.Zero |] world
            let vel = PhysicsWorld.getBodyVelocity body world
            Expect.floatClose Accuracy.medium (float vel.Linear.X) 7.0 "Velocity should be set"
    ]
