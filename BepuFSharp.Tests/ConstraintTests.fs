module BepuFSharp.Tests.ConstraintTests

open System.Numerics
open Expecto
open BepuFSharp

[<Tests>]
let tests =
    testList "Constraints" [
        testCase "BallSocket keeps bodies connected" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(-1.0f, 0.0f, 0.0f))) 1.0f) world
            let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(1.0f, 0.0f, 0.0f))) 1.0f) world
            let spring = SpringConfig.create 30.0f 1.0f
            let (ConstraintId cid) = PhysicsWorld.addConstraint bodyA bodyB (ConstraintDesc.BallSocket(Vector3.UnitX, -Vector3.UnitX, spring)) world
            Expect.isTrue (cid >= 0) "Constraint handle should be valid"
            for _ in 1..60 do
                PhysicsWorld.step (1.0f / 60.0f) world
            let poseA = PhysicsWorld.getBodyPose bodyA world
            let poseB = PhysicsWorld.getBodyPose bodyB world
            let dist = (poseA.Position - poseB.Position).Length()
            Expect.isLessThan dist 5.0f "Bodies should stay reasonably close"

        testCase "removeConstraint removes it" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(2.0f, 0.0f, 0.0f))) 1.0f) world
            let cid = PhysicsWorld.addConstraint bodyA bodyB (ConstraintDesc.BallSocket(Vector3.Zero, Vector3.Zero, SpringConfig.create 30.0f 1.0f)) world
            PhysicsWorld.removeConstraint cid world

        testCase "Weld rigidly attaches bodies" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(1.0f, 0.0f, 0.0f))) 1.0f) world
            let spring = SpringConfig.create 120.0f 1.0f
            let _cid = PhysicsWorld.addConstraint bodyA bodyB (ConstraintDesc.Weld(Vector3.UnitX, Quaternion.Identity, spring)) world
            PhysicsWorld.setBodyVelocity bodyA (Velocity.create (Vector3(5.0f, 0.0f, 0.0f)) Vector3.Zero) world
            for _ in 1..60 do
                PhysicsWorld.step (1.0f / 60.0f) world
            let poseA = PhysicsWorld.getBodyPose bodyA world
            let poseB = PhysicsWorld.getBodyPose bodyB world
            let dist = (poseA.Position - poseB.Position).Length()
            Expect.isLessThan dist 2.0f "Welded bodies should stay close"

        testCase "removeBody auto-removes constraints" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(2.0f, 0.0f, 0.0f))) 1.0f) world
            let _cid = PhysicsWorld.addConstraint bodyA bodyB (ConstraintDesc.BallSocket(Vector3.Zero, Vector3.Zero, SpringConfig.create 30.0f 1.0f)) world
            PhysicsWorld.removeBody bodyA world
    ]
