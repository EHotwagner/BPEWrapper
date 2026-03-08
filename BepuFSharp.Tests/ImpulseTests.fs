module BepuFSharp.Tests.ImpulseTests

open System.Numerics
open Expecto
open BepuFSharp

[<Tests>]
let tests =
    testList "Impulse" [
        testCase "applyImpulse with offset produces linear and angular velocity" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.applyImpulse bodyId (Vector3(10.0f, 0.0f, 0.0f)) (Vector3(0.0f, 0.5f, 0.0f)) world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.isTrue (vel.Linear.X > 0.0f) "Should have positive X linear velocity"
            Expect.isTrue (vel.Angular.LengthSquared() > 0.0f) "Should have angular velocity from offset impulse"

        testCase "applyLinearImpulse produces linear velocity only" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.applyLinearImpulse bodyId (Vector3(5.0f, 0.0f, 0.0f)) world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.isTrue (vel.Linear.X > 0.0f) "Should have positive X linear velocity"
            Expect.floatClose Accuracy.medium (float (vel.Angular.LengthSquared())) 0.0 "Should have no angular velocity"

        testCase "applyAngularImpulse produces angular velocity only" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.applyAngularImpulse bodyId (Vector3(0.0f, 5.0f, 0.0f)) world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.floatClose Accuracy.medium (float vel.Linear.X) 0.0 "Should have no X linear velocity"
            Expect.isTrue (vel.Angular.Y > 0.0f) "Should have positive Y angular velocity"

        testCase "impulse on removed body does not crash" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
            let desc = DynamicBodyDesc.create shape Pose.identity 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.removeBody bodyId world
            PhysicsWorld.applyImpulse bodyId (Vector3(1.0f, 0.0f, 0.0f)) Vector3.Zero world
            PhysicsWorld.applyLinearImpulse bodyId (Vector3(1.0f, 0.0f, 0.0f)) world
            PhysicsWorld.applyAngularImpulse bodyId (Vector3(1.0f, 0.0f, 0.0f)) world
    ]
