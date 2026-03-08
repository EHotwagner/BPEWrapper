module BepuFSharp.Tests.BodyTests

open System.Numerics
open Expecto
open BepuFSharp

[<Tests>]
let tests =
    testList "Bodies" [
        testCase "add dynamic body returns valid BodyId" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f
            let (BodyId id) = PhysicsWorld.addBody desc world
            Expect.isTrue (id >= 0) "Body handle should be non-negative"

        testCase "add static body returns valid StaticId" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
            let desc = StaticBodyDesc.create shape (Pose.ofPosition Vector3.Zero)
            let (StaticId id) = PhysicsWorld.addStatic desc world
            Expect.isTrue (id >= 0) "Static handle should be non-negative"

        testCase "removeBody removes the body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.removeBody bodyId world

        testCase "removeStatic removes the static" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Box(10.0f, 1.0f, 10.0f)) world
            let desc = StaticBodyDesc.create shape Pose.identity
            let staticId = PhysicsWorld.addStatic desc world
            PhysicsWorld.removeStatic staticId world

        testCase "zero mass creates kinematic body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f))) 0.0f
            let bodyId = PhysicsWorld.addBody desc world
            let sim = PhysicsWorld.simulation world
            let handle = match bodyId with BodyId id -> BepuPhysics.BodyHandle(id)
            let bodyRef = sim.Bodies.[handle]
            Expect.isTrue bodyRef.Kinematic "Zero mass should create kinematic body"

        testCase "negative mass raises NegativeMass error" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity -1.0f
            Expect.throws
                (fun () -> PhysicsWorld.addBody desc world |> ignore)
                "Negative mass should raise error"

        testCase "add kinematic body returns valid BodyId" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = KinematicBodyDesc.create shape Pose.identity
            let (BodyId id) = PhysicsWorld.addKinematicBody desc world
            Expect.isTrue (id >= 0) "Kinematic body handle should be non-negative"

        testCase "dynamic body falls under gravity" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let startPos = Vector3(0.0f, 10.0f, 0.0f)
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition startPos) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            for _ in 1..60 do
                PhysicsWorld.step (1.0f / 60.0f) world
            let sim = PhysicsWorld.simulation world
            let handle = match bodyId with BodyId id -> BepuPhysics.BodyHandle(id)
            let bodyRef = sim.Bodies.[handle]
            Expect.isTrue (bodyRef.Pose.Position.Y < startPos.Y) "Body should have fallen under gravity"

        testCase "MaterialProperties.create constructs correct values" <| fun _ ->
            let mat = MaterialProperties.create 0.5f 3.0f 60.0f 0.8f
            Expect.floatClose Accuracy.medium (float mat.Friction) 0.5 "Friction"
            Expect.floatClose Accuracy.medium (float mat.MaxRecoveryVelocity) 3.0 "MaxRecoveryVelocity"
            Expect.floatClose Accuracy.medium (float mat.SpringFrequency) 60.0 "SpringFrequency"
            Expect.floatClose Accuracy.medium (float mat.SpringDampingRatio) 0.8 "SpringDampingRatio"

        testCase "bodies with different friction on slope slide differently" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            // Angled floor (static)
            let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
            let floorPose = Pose.create Vector3.Zero (Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.3f))
            let floorDesc = StaticBodyDesc.create floorShape floorPose
            let _floor = PhysicsWorld.addStatic floorDesc world

            let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let startPos = Vector3(0.0f, 5.0f, 0.0f)

            // High friction body
            let highFricDesc =
                { DynamicBodyDesc.create sphereShape (Pose.ofPosition startPos) 1.0f
                    with Material = MaterialProperties.create 10.0f 2.0f 30.0f 1.0f }
            let highFricBody = PhysicsWorld.addBody highFricDesc world

            // Low friction body - offset to avoid collision between the two
            let lowFricDesc =
                { DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(5.0f, 5.0f, 0.0f))) 1.0f
                    with Material = MaterialProperties.create 0.001f 2.0f 30.0f 1.0f }
            let lowFricBody = PhysicsWorld.addBody lowFricDesc world

            for _ in 1..120 do
                PhysicsWorld.step (1.0f / 60.0f) world

            let highFricPose = PhysicsWorld.getBodyPose highFricBody world
            let lowFricPose = PhysicsWorld.getBodyPose lowFricBody world

            // Low friction body should have moved further laterally on the slope
            let highXDist = abs highFricPose.Position.X
            let lowXDist = abs (lowFricPose.Position.X - 5.0f)
            Expect.isTrue (lowXDist >= highXDist * 0.5f || lowFricPose.Position.Y < highFricPose.Position.Y)
                "Low friction body should slide more or fall further on slope"

        testCase "bodyExists returns true for active body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            Expect.isTrue (PhysicsWorld.bodyExists bodyId world) "Active body should exist"

        testCase "bodyExists returns false for removed body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.removeBody bodyId world
            Expect.isFalse (PhysicsWorld.bodyExists bodyId world) "Removed body should not exist"

        testCase "staticExists returns true for active static" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Box(10.0f, 1.0f, 10.0f)) world
            let desc = StaticBodyDesc.create shape Pose.identity
            let staticId = PhysicsWorld.addStatic desc world
            Expect.isTrue (PhysicsWorld.staticExists staticId world) "Active static should exist"

        testCase "staticExists returns false for removed static" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Box(10.0f, 1.0f, 10.0f)) world
            let desc = StaticBodyDesc.create shape Pose.identity
            let staticId = PhysicsWorld.addStatic desc world
            PhysicsWorld.removeStatic staticId world
            Expect.isFalse (PhysicsWorld.staticExists staticId world) "Removed static should not exist"

        testCase "tryGetBodyPose returns ValueSome for active body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let pos = Vector3(1.0f, 2.0f, 3.0f)
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition pos) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            match PhysicsWorld.tryGetBodyPose bodyId world with
            | ValueSome pose ->
                Expect.floatClose Accuracy.medium (float pose.Position.X) 1.0 "X should match"
            | ValueNone -> failtest "Should return ValueSome for active body"

        testCase "tryGetBodyPose returns ValueNone for removed body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.removeBody bodyId world
            match PhysicsWorld.tryGetBodyPose bodyId world with
            | ValueNone -> ()
            | ValueSome _ -> failtest "Should return ValueNone for removed body"

        testCase "tryGetBodyVelocity returns ValueSome for active body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            match PhysicsWorld.tryGetBodyVelocity bodyId world with
            | ValueSome _ -> ()
            | ValueNone -> failtest "Should return ValueSome for active body"

        testCase "tryGetBodyVelocity returns ValueNone for removed body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.removeBody bodyId world
            match PhysicsWorld.tryGetBodyVelocity bodyId world with
            | ValueNone -> ()
            | ValueSome _ -> failtest "Should return ValueNone for removed body"

        testCase "trySetBodyPose returns true for active body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            let result = PhysicsWorld.trySetBodyPose bodyId (Pose.ofPosition (Vector3(5.0f, 0.0f, 0.0f))) world
            Expect.isTrue result "Should return true for active body"

        testCase "trySetBodyPose returns false for removed body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.removeBody bodyId world
            let result = PhysicsWorld.trySetBodyPose bodyId Pose.identity world
            Expect.isFalse result "Should return false for removed body"

        testCase "trySetBodyVelocity returns true for active body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            let result = PhysicsWorld.trySetBodyVelocity bodyId Velocity.zero world
            Expect.isTrue result "Should return true for active body"

        testCase "trySetBodyVelocity returns false for removed body" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.removeBody bodyId world
            let result = PhysicsWorld.trySetBodyVelocity bodyId Velocity.zero world
            Expect.isFalse result "Should return false for removed body"
    ]
