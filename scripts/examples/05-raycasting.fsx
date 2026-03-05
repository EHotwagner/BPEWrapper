#load "../prelude.fsx"
open BepuFSharp
open System.Numerics

let world = PhysicsWorld.create PhysicsConfig.defaults

// Add a floor and some bodies
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
let _floor = PhysicsWorld.addStatic (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f)))) world

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
let _sphere1 = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f) world
let _sphere2 = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f))) 1.0f) world

PhysicsWorld.step (1.0f / 60.0f) world

// Single raycast - closest hit
let hit = PhysicsWorld.raycast (Vector3(0.0f, 20.0f, 0.0f)) (Vector3(0.0f, -1.0f, 0.0f)) 100.0f world
match hit with
| Some h ->
    printfn "Closest hit at distance %.2f, normal (%.2f, %.2f, %.2f)" h.Distance h.Normal.X h.Normal.Y h.Normal.Z
    match h.Body, h.Static with
    | ValueSome (BodyId id), _ -> printfn "  Hit body %d" id
    | _, ValueSome (StaticId id) -> printfn "  Hit static %d" id
    | _ -> ()
| None -> printfn "No hit"

// Raycast all - all hits
let hits = PhysicsWorld.raycastAll (Vector3(0.0f, 20.0f, 0.0f)) (Vector3(0.0f, -1.0f, 0.0f)) 100.0f world
printfn "\nAll hits (%d):" hits.Length
for h in hits do
    printfn "  Distance: %.2f" h.Distance

// Miss test
let miss = PhysicsWorld.raycast (Vector3(0.0f, 20.0f, 0.0f)) (Vector3(0.0f, 1.0f, 0.0f)) 100.0f world
printfn "\nRay pointing up: %s" (if miss.IsNone then "miss (correct)" else "hit (unexpected)")

PhysicsWorld.destroy world
