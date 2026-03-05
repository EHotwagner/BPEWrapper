#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

let world = PhysicsWorld.create PhysicsConfig.defaults

// Add shapes
let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
let boxShape = PhysicsWorld.addShape (PhysicsShape.Box(2.0f, 2.0f, 2.0f)) world

// Dynamic body
let dynamic = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f))) 1.0f) world
printfn "Dynamic body: %A" dynamic

// Kinematic body (zero mass via addBody, or explicit)
let kinematic = PhysicsWorld.addKinematicBody (KinematicBodyDesc.create boxShape (Pose.ofPosition (Vector3(5.0f, 0.0f, 0.0f)))) world
printfn "Kinematic body: %A" kinematic

// Static body
let floor = PhysicsWorld.addStatic (StaticBodyDesc.create boxShape (Pose.ofPosition (Vector3(0.0f, -1.0f, 0.0f)))) world
printfn "Static body: %A" floor

// Step and observe
for _ in 1..60 do
    PhysicsWorld.step (1.0f / 60.0f) world

let dynPose = PhysicsWorld.getBodyPose dynamic world
let kinPose = PhysicsWorld.getBodyPose kinematic world
printfn "Dynamic fell to Y=%.2f, Kinematic stayed at Y=%.2f" dynPose.Position.Y kinPose.Position.Y

// Remove bodies
PhysicsWorld.removeBody dynamic world
PhysicsWorld.removeBody kinematic world
PhysicsWorld.removeStatic floor world
printfn "All bodies removed."

PhysicsWorld.destroy world
