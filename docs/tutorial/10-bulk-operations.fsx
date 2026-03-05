(**
---
title: Bulk Operations and ECS Integration
category: Tutorial
categoryindex: 3
index: 10
---
*)

(**
# Bulk Operations and ECS Integration

**Prerequisites**: [Chapter 1](01-what-is-physics.html),
[Chapter 3: Bodies](03-bodies.html),
[Chapter 4: Simulation Loop](04-simulation-loop.html)

This chapter explains how to efficiently synchronize physics state with
a game engine using **bulk operations**, and introduces the
Entity-Component-System (ECS) architecture pattern.

## The Problem: Per-Body Reads Are Slow

In previous chapters we used `getBodyPose` to read one body at a time.
This works for a few objects, but games often have hundreds or thousands
of bodies. Calling `getBodyPose` in a loop means many individual
function calls with overhead each time.

```text
  Slow approach (per-body):
  +-------+  getBodyPose  +----------+
  | Body 1| ------------> | Pose 1   |   N separate calls
  +-------+               +----------+   for N bodies
  +-------+  getBodyPose  +----------+
  | Body 2| ------------> | Pose 2   |
  +-------+               +----------+
     ...                     ...

  Fast approach (bulk):
  +-------+               +----------+
  | Body 1|               | Pose 1   |
  +-------+  readPoses    +----------+   1 call for
  | Body 2| ------------> | Pose 2   |   all N bodies
  +-------+  (all at once)+----------+
  | Body 3|               | Pose 3   |
  +-------+               +----------+
```

## What Is ECS?

Many modern game engines use an **Entity-Component-System** architecture:

- **Entity**: Just an ID number (like a row in a spreadsheet).
- **Component**: A data struct attached to an entity (Position, Health,
  Sprite, PhysicsBody). Components are stored in flat arrays.
- **System**: A function that processes all entities with certain
  components (PhysicsSystem, RenderSystem, AISystem).

The physics sync loop in an ECS game looks like this:

```text
  Game Frame:
  +------------------+
  |  Game Logic      |   AI moves entities, player input
  +--------+---------+
           |
           v
  +--------+---------+
  |  writePoses      |   Push ECS positions into physics
  +--------+---------+
           |
           v
  +--------+---------+
  |  PhysicsWorld    |   Simulate one step
  |  .step()         |
  +--------+---------+
           |
           v
  +--------+---------+
  |  readPoses       |   Pull physics results back to ECS
  +--------+---------+
           |
           v
  +--------+---------+
  |  Render          |   Draw everything at new positions
  +------------------+
```

## Setting Up a Bulk Scene

Let's create a scene with many bodies and use bulk operations to
read and write their state efficiently:
*)

#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp

let world = PhysicsWorld.create PhysicsConfig.defaults

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.3f) world
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(50.0f, 1.0f, 50.0f)) world

let _floor =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))))
        world

// Create 100 bodies in a 10x10 grid, each at different heights
let bodyCount = 100

let bodies =
    [| for i in 0..bodyCount - 1 do
        let x = float32 (i % 10) * 2.0f - 9.0f
        let z = float32 (i / 10) * 2.0f - 9.0f
        let y = 5.0f + float32 i * 0.1f  // staggered heights
        PhysicsWorld.addBody
            (DynamicBodyDesc.create
                sphereShape
                (Pose.ofPosition (Vector3(x, y, z)))
                1.0f)
            world |]

(**
We now have 100 spheres hovering above a floor. Let's step the
simulation and then read all their poses at once.

## Bulk Read: readPoses

`PhysicsWorld.readPoses` takes an array of body IDs and fills a
pre-allocated pose array:
*)

// Step the simulation
for _ in 1..60 do
    PhysicsWorld.step (1.0f / 60.0f) world

// Allocate the output array once (reuse each frame in a real game)
let poses = Array.zeroCreate<Pose> bodyCount

// Read all 100 poses in one call
PhysicsWorld.readPoses bodies poses world

// Check a few results
let firstPose = poses.[0]
let lastPose = poses.[bodyCount - 1]

(*** include-value: firstPose ***)
(*** include-value: lastPose ***)

(**
All 100 poses are now in the `poses` array, ready to be copied back
into your game's ECS component storage.

## Bulk Read: readVelocities

Similarly, you can read all velocities at once:
*)

let velocities = Array.zeroCreate<Velocity> bodyCount
PhysicsWorld.readVelocities bodies velocities world

let firstVelocity = velocities.[0]

(*** include-value: firstVelocity ***)

(**
## Bulk Write: writePoses

`writePoses` pushes positions from your game back into physics. This is
useful when game logic teleports or repositions entities:
*)

// Teleport all bodies back up to their starting heights
let resetPoses =
    [| for i in 0..bodyCount - 1 do
        let x = float32 (i % 10) * 2.0f - 9.0f
        let z = float32 (i / 10) * 2.0f - 9.0f
        let y = 10.0f + float32 i * 0.1f
        Pose.ofPosition (Vector3(x, y, z)) |]

PhysicsWorld.writePoses bodies resetPoses world

// Verify a body was teleported
let teleportedPose = PhysicsWorld.getBodyPose bodies.[0] world

(*** include-value: teleportedPose ***)

(**
The bodies are now back at their reset positions.

## Bulk Write: writeVelocities

You can also set velocities in bulk. Let's give every body a random
horizontal velocity:
*)

let pushVelocities =
    [| for i in 0..bodyCount - 1 do
        let angle = float32 i * 0.1f
        let vx = cos angle * 3.0f
        let vz = sin angle * 3.0f
        Velocity.create (Vector3(vx, 0.0f, vz)) Vector3.Zero |]

PhysicsWorld.writeVelocities bodies pushVelocities world

// Step and see them scatter
for _ in 1..60 do
    PhysicsWorld.step (1.0f / 60.0f) world

PhysicsWorld.readPoses bodies poses world
let scatteredPose = poses.[0]

(*** include-value: scatteredPose ***)

(**
## The Full ECS Sync Pattern

In a real game, the sync loop each frame looks like this:

```fsharp
// Each frame:
// 1. Game logic updates entityPositions (e.g., AI, input)
// 2. Push changes into physics
PhysicsWorld.writePoses bodyIds entityPoses world

// 3. Step physics
PhysicsWorld.step dt world

// 4. Pull results back
PhysicsWorld.readPoses bodyIds entityPoses world
PhysicsWorld.readVelocities bodyIds entityVelocities world

// 5. Render using entityPoses
```

The key insight is that you allocate the pose and velocity arrays
**once** and reuse them every frame. No allocations in the hot loop.
*)

PhysicsWorld.destroy world

(**
## Experiment

Try these modifications:

1. Increase the body count to 500. Bulk operations scale well because
   they minimize per-call overhead.
2. After `writePoses`, immediately call `readPoses` without stepping.
   Verify you get back exactly what you wrote.
3. Use `writeVelocities` to give all bodies the same upward velocity
   `Vector3(0, 10, 0)` and watch them jump in unison.

## Summary

Bulk operations let you read and write physics state for many bodies
in a single call. `readPoses` and `readVelocities` pull data from
physics into pre-allocated arrays. `writePoses` and `writeVelocities`
push data from your game into physics. This pattern is essential for
ECS integration where thousands of entities need synchronized each
frame.

**Next**: [Putting It All Together](11-capstone.html)
*)
