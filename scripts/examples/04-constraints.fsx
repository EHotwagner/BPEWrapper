#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

let world = PhysicsWorld.create PhysicsConfig.defaults
let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world

// Create two bodies connected by a ball socket joint
let bodyA = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f) world
let bodyB = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(2.0f, 5.0f, 0.0f))) 1.0f) world

let spring = SpringConfig.create 30.0f 1.0f
let _ballSocket = PhysicsWorld.addConstraint bodyA bodyB
    (ConstraintDesc.BallSocket(Vector3(1.0f, 0.0f, 0.0f), Vector3(-1.0f, 0.0f, 0.0f), spring)) world
printfn "Ball socket constraint created"

// Create a welded pair
let bodyC = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(5.0f, 5.0f, 0.0f))) 1.0f) world
let bodyD = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(5.0f, 6.0f, 0.0f))) 1.0f) world
let _weld = PhysicsWorld.addConstraint bodyC bodyD
    (ConstraintDesc.Weld(Vector3(0.0f, 0.5f, 0.0f), Quaternion.Identity, spring)) world
printfn "Weld constraint created"

// Step and observe
for i in 1..120 do
    PhysicsWorld.step (1.0f / 60.0f) world
    if i % 30 = 0 then
        let pA = PhysicsWorld.getBodyPose bodyA world
        let pB = PhysicsWorld.getBodyPose bodyB world
        let dist = Vector3.Distance(pA.Position, pB.Position)
        printfn "Step %d: A-B distance = %.3f" i dist

PhysicsWorld.destroy world
printfn "Done!"
