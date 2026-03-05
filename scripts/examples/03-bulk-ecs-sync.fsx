#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

let world = PhysicsWorld.create PhysicsConfig.defaults

// Create 100 bodies
let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let bodies =
    [| for i in 0..99 ->
        let x = float32 (i % 10) * 2.0f
        let z = float32 (i / 10) * 2.0f
        PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(x, 10.0f, z))) 1.0f) world |]

// Pre-allocate arrays for bulk operations (zero-allocation hot path)
let poses = Array.zeroCreate<Pose> bodies.Length
let velocities = Array.zeroCreate<Velocity> bodies.Length

// Step and bulk read
for frame in 1..60 do
    PhysicsWorld.step (1.0f / 60.0f) world
    PhysicsWorld.readPoses bodies poses world
    PhysicsWorld.readVelocities bodies velocities world
    if frame % 20 = 0 then
        printfn "Frame %d: Body[0] Y=%.3f, vel Y=%.3f" frame poses.[0].Position.Y velocities.[0].Linear.Y

// Bulk write - teleport all bodies back to starting height
for i in 0..bodies.Length-1 do
    poses.[i] <- Pose.ofPosition (Vector3(poses.[i].Position.X, 20.0f, poses.[i].Position.Z))
PhysicsWorld.writePoses bodies poses world

let afterWrite = PhysicsWorld.getBodyPose bodies.[0] world
printfn "After bulk write, Body[0] Y=%.2f" afterWrite.Position.Y

PhysicsWorld.destroy world
