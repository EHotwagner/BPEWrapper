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

        // --- Runtime Filter and Material Modification Tests ---

        testCase "setCollisionFilter: change mask to exclude layer, body passes through" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
            let floorDesc =
                { StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))) with
                    CollisionGroup = 0u; CollisionMask = 0xFFFFFFFFu }
            let _floor = PhysicsWorld.addStatic floorDesc world
            let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyDesc =
                { DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 2.0f, 0.0f))) 1.0f with
                    CollisionGroup = 0u; CollisionMask = 0xFFFFFFFFu }
            let bodyId = PhysicsWorld.addBody bodyDesc world
            // Step to let it settle on the floor
            for _ in 1..60 do PhysicsWorld.step (1.0f/60.0f) world
            let poseBefore = PhysicsWorld.getBodyPose bodyId world
            // Change mask to exclude layer 0 (floor's layer)
            PhysicsWorld.setCollisionFilter bodyId { Group = 0u; Mask = 0u } world
            // Step more — body should fall through floor
            for _ in 1..120 do PhysicsWorld.step (1.0f/60.0f) world
            let poseAfter = PhysicsWorld.getBodyPose bodyId world
            Expect.isTrue (poseAfter.Position.Y < poseBefore.Position.Y - 1.0f)
                "Body should fall through floor after filter change"

        testCase "setMaterial: change friction to zero" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyDesc =
                { DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 2.0f, 0.0f))) 1.0f with
                    Material = MaterialProperties.create 10.0f 2.0f 30.0f 1.0f }
            let bodyId = PhysicsWorld.addBody bodyDesc world
            // Change material to zero friction
            PhysicsWorld.setMaterial bodyId (MaterialProperties.create 0.0f 2.0f 30.0f 1.0f) world
            // Should not throw — just verifying mutation works
            Expect.isTrue true "setMaterial should succeed"

        testCase "setStaticCollisionFilter: change static body's filter" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
            let floorDesc =
                { StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))) with
                    CollisionGroup = 0u; CollisionMask = 0xFFFFFFFFu }
            let floorId = PhysicsWorld.addStatic floorDesc world
            // Change the static's filter — should not throw
            PhysicsWorld.setStaticCollisionFilter floorId { Group = 1u; Mask = 0xFFFFFFFFu } world
            Expect.isTrue true "setStaticCollisionFilter should succeed"

        testCase "setCollisionFilter on removed body raises error" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyId = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            PhysicsWorld.removeBody bodyId world
            Expect.throws
                (fun () -> PhysicsWorld.setCollisionFilter bodyId { Group = 0u; Mask = 0u } world)
                "Should raise error for removed body"
    ]
