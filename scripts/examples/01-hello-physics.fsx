#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

// Create world with default gravity
let world = PhysicsWorld.create PhysicsConfig.defaults

// Add a static floor and a falling sphere
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
let _floor = PhysicsWorld.addStatic (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f)))) world

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let sphere = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f) world

// Step the simulation and print the sphere's position
for i in 1..120 do
    PhysicsWorld.step (1.0f / 60.0f) world
    if i % 20 = 0 then
        let pose = PhysicsWorld.getBodyPose sphere world
        printfn "Step %d: Y = %.3f" i pose.Position.Y

PhysicsWorld.destroy world
printfn "Done!"
