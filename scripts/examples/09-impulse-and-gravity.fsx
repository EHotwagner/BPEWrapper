#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

let world = PhysicsWorld.create PhysicsConfig.defaults

// Show default gravity
let g = PhysicsWorld.getGravity world
printfn "Default gravity: (%g, %g, %g)" g.X g.Y g.Z

// Create a sphere
let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let bodyId = PhysicsWorld.addBody (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f))) 1.0f) world

// Describe the shape
printfn "Shape: %s" (PhysicsShape.describe (PhysicsShape.Sphere 0.5f))

// Check body exists
printfn "Body exists: %b" (PhysicsWorld.bodyExists bodyId world)

// Apply impulse with offset (produces both linear and angular velocity)
PhysicsWorld.applyImpulse bodyId (Vector3(10.0f, 0.0f, 0.0f)) (Vector3(0.0f, 0.5f, 0.0f)) world
let vel = PhysicsWorld.getBodyVelocity bodyId world
printfn "After impulse: linear=(%g, %g, %g) angular=(%g, %g, %g)"
    vel.Linear.X vel.Linear.Y vel.Linear.Z vel.Angular.X vel.Angular.Y vel.Angular.Z

// Safe accessor: tryGetBodyPose
match PhysicsWorld.tryGetBodyPose bodyId world with
| ValueSome pose -> printfn "Body pose Y: %g" pose.Position.Y
| ValueNone -> printfn "Body not found"

// Set zero gravity and simulate
PhysicsWorld.setGravity Vector3.Zero world
printfn "Gravity set to zero"

for _ in 1..60 do
    PhysicsWorld.step (1.0f / 60.0f) world

let pose = PhysicsWorld.getBodyPose bodyId world
printfn "After 1s zero-gravity: Y=%g (should stay near 10)" pose.Position.Y

// Enumerate all bodies
let allBodies = PhysicsWorld.getAllBodyIds world
printfn "Active bodies: %d" allBodies.Length

// Query shape from body
match PhysicsWorld.getBodyShape bodyId world with
| Some shape -> printfn "Body shape: %s" (PhysicsShape.describe shape)
| None -> printfn "Shape not found"

// Safe accessor on removed body
PhysicsWorld.removeBody bodyId world
printfn "Body exists after remove: %b" (PhysicsWorld.bodyExists bodyId world)
match PhysicsWorld.tryGetBodyPose bodyId world with
| ValueSome _ -> printfn "Unexpected"
| ValueNone -> printfn "tryGetBodyPose correctly returns ValueNone for removed body"

PhysicsWorld.destroy world
printfn "Done!"
