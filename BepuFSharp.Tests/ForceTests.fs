module BepuFSharp.Tests.ForceTests

open System.Numerics
open Expecto
open BepuFSharp

/// Zero-gravity config to isolate force/impulse effects
let private noGravityConfig =
    { PhysicsConfig.defaults with Gravity = Vector3.Zero }

[<Tests>]
let tests =
    testList "Forces" [
        // --- US1: Linear Impulse ---

        testCase "applyLinearImpulse changes velocity by impulse/mass" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 2.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.applyLinearImpulse bodyId (Vector3(10.0f, 0.0f, 0.0f)) world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.floatClose Accuracy.medium (float vel.Linear.X) 5.0 "Linear X should be impulse/mass = 10/2 = 5"
            Expect.floatClose Accuracy.medium (float vel.Linear.Y) 0.0 "Linear Y should be 0"
            Expect.floatClose Accuracy.medium (float vel.Linear.Z) 0.0 "Linear Z should be 0"

        testCase "applyLinearImpulse on kinematic body has no effect" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = KinematicBodyDesc.create shape Pose.identity
            let bodyId = PhysicsWorld.addKinematicBody desc world
            PhysicsWorld.applyLinearImpulse bodyId (Vector3(10.0f, 0.0f, 0.0f)) world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.floatClose Accuracy.medium (float vel.Linear.X) 0.0 "Kinematic body should not be affected"

        testCase "applyLinearImpulses bulk matches per-body results" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let ids = [|
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f) world
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(3.0f, 5.0f, 0.0f))) 2.0f) world
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(6.0f, 5.0f, 0.0f))) 4.0f) world
            |]
            let impulses = [|
                Vector3(10.0f, 0.0f, 0.0f)
                Vector3(10.0f, 0.0f, 0.0f)
                Vector3(10.0f, 0.0f, 0.0f)
            |]
            PhysicsWorld.applyLinearImpulses ids impulses world
            let v0 = PhysicsWorld.getBodyVelocity ids.[0] world
            let v1 = PhysicsWorld.getBodyVelocity ids.[1] world
            let v2 = PhysicsWorld.getBodyVelocity ids.[2] world
            Expect.floatClose Accuracy.medium (float v0.Linear.X) 10.0 "Body 0: impulse/mass = 10/1"
            Expect.floatClose Accuracy.medium (float v1.Linear.X) 5.0 "Body 1: impulse/mass = 10/2"
            Expect.floatClose Accuracy.medium (float v2.Linear.X) 2.5 "Body 2: impulse/mass = 10/4"

        // --- US2: Continuous Force ---

        testCase "applyForce changes velocity by force*dt/mass" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 2.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.applyForce bodyId (Vector3(10.0f, 0.0f, 0.0f)) 0.5f world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.floatClose Accuracy.medium (float vel.Linear.X) 2.5 "velocity = force*dt/mass = 10*0.5/2 = 2.5"

        testCase "applyForce does not persist across steps" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.applyForce bodyId (Vector3(10.0f, 0.0f, 0.0f)) (1.0f / 60.0f) world
            let velAfterApply = PhysicsWorld.getBodyVelocity bodyId world
            let vx1 = velAfterApply.Linear.X
            // Step twice without reapplying force
            PhysicsWorld.step (1.0f / 60.0f) world
            PhysicsWorld.step (1.0f / 60.0f) world
            let velAfterSteps = PhysicsWorld.getBodyVelocity bodyId world
            // Velocity should not have increased beyond the initial impulse (no gravity)
            Expect.floatClose Accuracy.medium (float velAfterSteps.Linear.X) (float vx1) "Force should not persist"

        testCase "applyForces bulk matches per-body results" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let ids = [|
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f) world
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(3.0f, 5.0f, 0.0f))) 2.0f) world
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(6.0f, 5.0f, 0.0f))) 4.0f) world
            |]
            let forces = [|
                Vector3(10.0f, 0.0f, 0.0f)
                Vector3(10.0f, 0.0f, 0.0f)
                Vector3(10.0f, 0.0f, 0.0f)
            |]
            let dt = 0.5f
            PhysicsWorld.applyForces ids forces dt world
            let v0 = PhysicsWorld.getBodyVelocity ids.[0] world
            let v1 = PhysicsWorld.getBodyVelocity ids.[1] world
            let v2 = PhysicsWorld.getBodyVelocity ids.[2] world
            Expect.floatClose Accuracy.medium (float v0.Linear.X) 5.0 "Body 0: force*dt/mass = 10*0.5/1"
            Expect.floatClose Accuracy.medium (float v1.Linear.X) 2.5 "Body 1: force*dt/mass = 10*0.5/2"
            Expect.floatClose Accuracy.medium (float v2.Linear.X) 1.25 "Body 2: force*dt/mass = 10*0.5/4"

        // --- US3: Angular Impulse and Torque ---

        testCase "applyAngularImpulse changes angular velocity" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.applyAngularImpulse bodyId (Vector3(0.0f, 1.0f, 0.0f)) world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.isTrue (vel.Angular.Y > 0.0f) "Angular velocity Y should be positive after Y-axis angular impulse"

        testCase "applyTorque changes angular velocity by torque*dt scaled by inertia" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            // Apply torque for dt=1.0 — should give same result as angular impulse of same magnitude
            PhysicsWorld.applyTorque bodyId (Vector3(0.0f, 1.0f, 0.0f)) 1.0f world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.isTrue (vel.Angular.Y > 0.0f) "Angular velocity Y should be positive after Y-axis torque"

        testCase "applyAngularImpulses bulk matches per-body results" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let ids = [|
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f) world
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(3.0f, 5.0f, 0.0f))) 1.0f) world
                PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(6.0f, 5.0f, 0.0f))) 1.0f) world
            |]
            let angularImpulses = [|
                Vector3(1.0f, 0.0f, 0.0f)
                Vector3(0.0f, 1.0f, 0.0f)
                Vector3(0.0f, 0.0f, 1.0f)
            |]
            PhysicsWorld.applyAngularImpulses ids angularImpulses world
            let v0 = PhysicsWorld.getBodyVelocity ids.[0] world
            let v1 = PhysicsWorld.getBodyVelocity ids.[1] world
            let v2 = PhysicsWorld.getBodyVelocity ids.[2] world
            Expect.isTrue (v0.Angular.X > 0.0f) "Body 0 should spin around X"
            Expect.isTrue (v1.Angular.Y > 0.0f) "Body 1 should spin around Y"
            Expect.isTrue (v2.Angular.Z > 0.0f) "Body 2 should spin around Z"

        // --- US4: Impulse at Point ---

        testCase "applyImpulseAtPoint at center produces only linear velocity" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            PhysicsWorld.applyImpulseAtPoint bodyId (Vector3(10.0f, 0.0f, 0.0f)) Vector3.Zero world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.floatClose Accuracy.medium (float vel.Linear.X) 10.0 "Linear X should equal impulse/mass"
            Expect.floatClose Accuracy.medium (float vel.Angular.X) 0.0 "Angular X should be 0 for center impulse"
            Expect.floatClose Accuracy.medium (float vel.Angular.Y) 0.0 "Angular Y should be 0 for center impulse"
            Expect.floatClose Accuracy.medium (float vel.Angular.Z) 0.0 "Angular Z should be 0 for center impulse"

        testCase "applyImpulseAtPoint at offset produces linear and angular velocity" <| fun _ ->
            use world = PhysicsWorld.create noGravityConfig
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f
            let bodyId = PhysicsWorld.addBody desc world
            // Apply impulse in X direction at offset (0, 1, 0) from center
            PhysicsWorld.applyImpulseAtPoint bodyId (Vector3(10.0f, 0.0f, 0.0f)) (Vector3(0.0f, 1.0f, 0.0f)) world
            let vel = PhysicsWorld.getBodyVelocity bodyId world
            Expect.floatClose Accuracy.medium (float vel.Linear.X) 10.0 "Linear X should equal impulse/mass"
            let angMag = vel.Angular.Length()
            Expect.isTrue (angMag > 0.0f) "Angular velocity should be non-zero for offset impulse"
    ]
