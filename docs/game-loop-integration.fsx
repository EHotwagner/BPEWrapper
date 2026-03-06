(**
---
title: Game Loop Integration
category: Guides
categoryindex: 1
index: 2
---
*)

(**
# Game Loop Integration Patterns

BepuFSharp is designed for data-oriented game engines. The key integration point is bulk
read/write of poses and velocities using pre-allocated arrays — zero managed allocations
on the hot path. This works with any engine architecture, whether ECS-based or not.

## The Sync Loop

A typical game loop synchronizes physics state with engine transforms:

1. **Write** kinematic targets from the engine to physics (e.g., animated platforms)
2. **Step** the simulation
3. **Read** dynamic body poses from physics back to engine transforms
*)

#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp

let world = PhysicsWorld.create PhysicsConfig.defaults

(**
## Setting Up Bodies

Create an array of body handles — this is your physics component array:
*)

let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world

let bodyCount = 1000
let bodies =
    [| for i in 0 .. bodyCount - 1 ->
        let x = float32 (i % 32) * 2.0f
        let z = float32 (i / 32) * 2.0f
        PhysicsWorld.addBody
            (DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(x, 10.0f, z))) 1.0f)
            world |]

(**
## Pre-Allocate Buffers

Allocate pose and velocity arrays **once** and reuse them every frame.
This is the key to zero-allocation bulk operations:
*)

let poses = Array.zeroCreate<Pose> bodyCount
let velocities = Array.zeroCreate<Velocity> bodyCount

(**
## The Hot Loop

Each frame: step, then bulk-read into your pre-allocated arrays.
No per-body allocation occurs — just struct copies:
*)

// Simulate 60 frames
for _frame in 1 .. 60 do
    PhysicsWorld.step (1.0f / 60.0f) world
    PhysicsWorld.readPoses bodies poses world
    PhysicsWorld.readVelocities bodies velocities world
    // In a real engine: copy poses array into engine transform components

(**
After the loop, `poses` contains the current position/orientation for all 1000 bodies:
*)

let firstBodyY = poses.[0].Position.Y

(*** include-value: firstBodyY ***)

(**
## Bulk Write: Teleporting Bodies

Use `writePoses` to teleport multiple bodies at once (e.g., respawning):
*)

for i in 0 .. bodyCount - 1 do
    poses.[i] <- Pose.ofPosition (Vector3(poses.[i].Position.X, 20.0f, poses.[i].Position.Z))

PhysicsWorld.writePoses bodies poses world

(**
## Bulk Write: Setting Velocities

Use `writeVelocities` to apply impulses or set velocities for multiple bodies:
*)

for i in 0 .. bodyCount - 1 do
    velocities.[i] <- Velocity.create (Vector3(0.0f, 5.0f, 0.0f)) Vector3.Zero

PhysicsWorld.writeVelocities bodies velocities world

(**
## Performance Notes

- `readPoses` and `writePoses` iterate the handle array and perform direct struct copies
  via inline interop functions. No boxing, no allocation.
- The arrays can be pinned and passed to native rendering code if needed.
- For maximum throughput, keep body handles in a contiguous array that matches
  your engine's component storage order.
*)

PhysicsWorld.destroy world
