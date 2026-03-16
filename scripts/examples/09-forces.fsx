#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

// Zero-gravity world so we can see pure force/impulse effects
let world = PhysicsWorld.create { PhysicsConfig.defaults with Gravity = Vector3.Zero }

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world

// --- 1. Linear impulse: launch a ball ---
let ball = PhysicsWorld.addBody
            (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 2.0f) world

PhysicsWorld.applyLinearImpulse ball (Vector3(20.0f, 0.0f, 0.0f)) world
let v1 = PhysicsWorld.getBodyVelocity ball world
printfn "After impulse(20,0,0) on mass=2: velocity = (%.1f, %.1f, %.1f)" v1.Linear.X v1.Linear.Y v1.Linear.Z
// Expected: (10, 0, 0) because velocity = impulse / mass

// --- 2. Continuous force: apply wind ---
let windTarget = PhysicsWorld.addBody
                    (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f))) 1.0f) world

let dt = 1.0f / 60.0f
PhysicsWorld.applyForce windTarget (Vector3(0.0f, 0.0f, -5.0f)) dt world
let v2 = PhysicsWorld.getBodyVelocity windTarget world
printfn "After force(0,0,-5) for dt=%.4f on mass=1: velocity = (%.4f, %.4f, %.4f)" dt v2.Linear.X v2.Linear.Y v2.Linear.Z

// --- 3. Angular impulse: spin a body ---
let spinner = PhysicsWorld.addBody
                (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 15.0f, 0.0f))) 1.0f) world

PhysicsWorld.applyAngularImpulse spinner (Vector3(0.0f, 5.0f, 0.0f)) world
let v3 = PhysicsWorld.getBodyVelocity spinner world
printfn "After angular impulse(0,5,0): angular velocity = (%.2f, %.2f, %.2f)" v3.Angular.X v3.Angular.Y v3.Angular.Z

// --- 4. Impulse at point: off-center hit ---
let target = PhysicsWorld.addBody
                (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 20.0f, 0.0f))) 1.0f) world

// Hit the "top" of the sphere — impulse in X, offset in Y
PhysicsWorld.applyImpulseAtPoint target (Vector3(10.0f, 0.0f, 0.0f)) (Vector3(0.0f, 1.0f, 0.0f)) world
let v4 = PhysicsWorld.getBodyVelocity target world
printfn "After impulse(10,0,0) at offset(0,1,0):"
printfn "  linear  = (%.2f, %.2f, %.2f)" v4.Linear.X v4.Linear.Y v4.Linear.Z
printfn "  angular = (%.2f, %.2f, %.2f)" v4.Angular.X v4.Angular.Y v4.Angular.Z

// --- 5. Bulk impulse: apply to multiple bodies at once ---
let bodies = [|
    PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 25.0f, 0.0f))) 1.0f) world
    PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(3.0f, 25.0f, 0.0f))) 2.0f) world
    PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(6.0f, 25.0f, 0.0f))) 4.0f) world
|]
let impulses = [|
    Vector3(10.0f, 0.0f, 0.0f)
    Vector3(10.0f, 0.0f, 0.0f)
    Vector3(10.0f, 0.0f, 0.0f)
|]
PhysicsWorld.applyLinearImpulses bodies impulses world
for i in 0..2 do
    let v = PhysicsWorld.getBodyVelocity bodies.[i] world
    printfn "Bulk body %d: velocity.X = %.2f" i v.Linear.X

PhysicsWorld.destroy world
printfn "Done."
