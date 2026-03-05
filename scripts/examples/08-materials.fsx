#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

let world = PhysicsWorld.create PhysicsConfig.defaults

// Angled floor
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
let floorPose = Pose.create Vector3.Zero (Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.3f))
let _floor = PhysicsWorld.addStatic (StaticBodyDesc.create floorShape floorPose) world

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world

// High friction sphere (sticky)
let highFric = PhysicsWorld.addBody
                { DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f
                    with Material = MaterialProperties.create 5.0f 2.0f 30.0f 1.0f } world

// Low friction sphere (slippery)
let lowFric = PhysicsWorld.addBody
                { DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(5.0f, 5.0f, 0.0f))) 1.0f
                    with Material = MaterialProperties.create 0.01f 2.0f 30.0f 1.0f } world

for i in 1..120 do
    PhysicsWorld.step (1.0f / 60.0f) world
    if i % 30 = 0 then
        let hPos = (PhysicsWorld.getBodyPose highFric world).Position
        let lPos = (PhysicsWorld.getBodyPose lowFric world).Position
        printfn "Step %d: High-fric X=%.2f Y=%.2f | Low-fric X=%.2f Y=%.2f" i hPos.X hPos.Y lPos.X lPos.Y

PhysicsWorld.destroy world
printfn "Done!"
