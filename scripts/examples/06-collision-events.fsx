#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

let world = PhysicsWorld.create PhysicsConfig.defaults

// Static floor
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
let _floor = PhysicsWorld.addStatic (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f)))) world

// Falling sphere
let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let _sphere = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 3.0f, 0.0f))) 1.0f) world

// Step and check for contact events
for i in 1..120 do
    PhysicsWorld.step (1.0f / 60.0f) world
    let events = PhysicsWorld.getContactEvents world
    if events.Length > 0 then
        for evt in events do
            let typeStr = match evt.EventType with Began -> "Began" | Persisted -> "Persisted" | Ended -> "Ended"
            printfn "Step %d: %s event" i typeStr

PhysicsWorld.destroy world
printfn "Done!"
