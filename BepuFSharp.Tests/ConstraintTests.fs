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

        // --- Constraint Readback Tests ---

        testCase "getConstraintDescription reads back Hinge parameters" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(2.0f, 0.0f, 0.0f))) 1.0f) world
            let spring = SpringConfig.create 30.0f 1.0f
            let desc = ConstraintDesc.Hinge(Vector3.UnitY, Vector3.UnitY, Vector3.UnitX, -Vector3.UnitX, spring)
            let cid = PhysicsWorld.addConstraint bodyA bodyB desc world
            let readback = PhysicsWorld.getConstraintDescription cid world
            Expect.isSome readback "Should read back the constraint"
            match readback.Value with
            | ConstraintDesc.Hinge(axA, axB, offA, offB, sp) ->
                Expect.floatClose Accuracy.medium (float axA.Y) 1.0 "Hinge axis A Y"
                Expect.floatClose Accuracy.medium (float axB.Y) 1.0 "Hinge axis B Y"
                Expect.floatClose Accuracy.medium (float offA.X) 1.0 "Offset A X"
                Expect.floatClose Accuracy.medium (float offB.X) -1.0 "Offset B X"
                Expect.floatClose Accuracy.medium (float sp.Frequency) 30.0 "Spring frequency"
            | other -> failwithf "Expected Hinge, got %A" other

        testCase "constraintExists returns true for active, false after removal" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(2.0f, 0.0f, 0.0f))) 1.0f) world
            let cid = PhysicsWorld.addConstraint bodyA bodyB (ConstraintDesc.BallSocket(Vector3.Zero, Vector3.Zero, SpringConfig.create 30.0f 1.0f)) world
            Expect.isTrue (PhysicsWorld.constraintExists cid world) "Should exist after creation"
            PhysicsWorld.removeConstraint cid world
            Expect.isFalse (PhysicsWorld.constraintExists cid world) "Should not exist after removal"

        testCase "getConstraintBodies returns correct two BodyIds" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(2.0f, 0.0f, 0.0f))) 1.0f) world
            let cid = PhysicsWorld.addConstraint bodyA bodyB (ConstraintDesc.BallSocket(Vector3.Zero, Vector3.Zero, SpringConfig.create 30.0f 1.0f)) world
            let bodies = PhysicsWorld.getConstraintBodies cid world
            Expect.isSome bodies "Should return bodies"
            let (a, b) = bodies.Value
            Expect.equal a bodyA "First body should match"
            Expect.equal b bodyB "Second body should match"

        testCase "readback all 10 constraint types" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(2.0f, 0.0f, 0.0f))) 1.0f) world
            let spring = SpringConfig.create 30.0f 1.0f
            let motor = { MaxForce = 100.0f; Damping = 10.0f }
            let descs = [
                ConstraintDesc.BallSocket(Vector3.UnitX, -Vector3.UnitX, spring)
                ConstraintDesc.Hinge(Vector3.UnitY, Vector3.UnitY, Vector3.UnitX, -Vector3.UnitX, spring)
                ConstraintDesc.Weld(Vector3.UnitX, Quaternion.Identity, spring)
                ConstraintDesc.DistanceLimit(Vector3.UnitX, -Vector3.UnitX, 0.5f, 3.0f, spring)
                ConstraintDesc.DistanceSpring(Vector3.UnitX, -Vector3.UnitX, 2.0f, spring)
                ConstraintDesc.SwingLimit(Vector3.UnitY, Vector3.UnitY, 1.0f, spring)
                ConstraintDesc.TwistLimit(Vector3.UnitY, Vector3.UnitY, -0.5f, 0.5f, spring)
                ConstraintDesc.LinearAxisMotor(Vector3.UnitX, -Vector3.UnitX, Vector3.UnitX, 1.0f, motor)
                ConstraintDesc.AngularMotor(Vector3.UnitY, motor)
                ConstraintDesc.PointOnLine(Vector3.Zero, Vector3.UnitX, Vector3.Zero, spring)
            ]
            for desc in descs do
                // Remove previous constraint by removing and re-adding bodyB
                PhysicsWorld.removeBody bodyB world
                let bodyB2 = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(2.0f, 0.0f, 0.0f))) 1.0f) world
                let cid = PhysicsWorld.addConstraint bodyA bodyB2 desc world
                let readback = PhysicsWorld.getConstraintDescription cid world
                Expect.isSome readback (sprintf "Should read back %A" desc)

        testCase "readback after constraint removal returns None" <| fun _ ->
            use world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create shape Pose.identity 1.0f) world
            let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(2.0f, 0.0f, 0.0f))) 1.0f) world
            let cid = PhysicsWorld.addConstraint bodyA bodyB (ConstraintDesc.BallSocket(Vector3.Zero, Vector3.Zero, SpringConfig.create 30.0f 1.0f)) world
            PhysicsWorld.removeConstraint cid world
            let readback = PhysicsWorld.getConstraintDescription cid world
            Expect.isNone readback "Should return None after removal"
    ]
