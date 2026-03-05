#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

let world = PhysicsWorld.create PhysicsConfig.defaults
let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world

// Two spheres that CAN collide (default masks)
let a1 = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f))) 1.0f) world
let a2 = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 0.8f, 0.0f))) 1.0f) world

// Two spheres that CANNOT collide (mask=0 rejects all)
let b1 = PhysicsWorld.addBody
            { DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(5.0f, 0.0f, 0.0f))) 1.0f
                with CollisionGroup = 1u; CollisionMask = 0u } world
let b2 = PhysicsWorld.addBody
            { DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(5.0f, 0.8f, 0.0f))) 1.0f
                with CollisionGroup = 1u; CollisionMask = 0u } world

for i in 1..30 do
    PhysicsWorld.step (1.0f / 60.0f) world
    let events = PhysicsWorld.getContactEvents world
    if events.Length > 0 then
        printfn "Step %d: %d contact events" i events.Length

// The colliding pair should push apart, the filtered pair should pass through
let pA1 = PhysicsWorld.getBodyPose a1 world
let pA2 = PhysicsWorld.getBodyPose a2 world
let pB1 = PhysicsWorld.getBodyPose b1 world
let pB2 = PhysicsWorld.getBodyPose b2 world
printfn "Colliding pair distance: %.3f" (Vector3.Distance(pA1.Position, pA2.Position))
printfn "Filtered pair distance: %.3f" (Vector3.Distance(pB1.Position, pB2.Position))

PhysicsWorld.destroy world
