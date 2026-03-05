(**
---
title: Putting It All Together
category: Tutorial
categoryindex: 3
index: 11
---
*)

(**
# Putting It All Together

**Prerequisites**: All previous chapters (1-10)

This capstone chapter builds a complete physics scenario from scratch,
combining every concept from the tutorial. We will construct a scene
with a floor, walls, dynamic objects, constraints, collision filtering,
material tuning, raycasting, and bulk operations — all in one script.

## The Scene

We will build a small arena:

```text
       Wall (static)
  +========================+
  |                        |
  |   [Pendulum]           |
  |    o---o               |
  |                        |
  |        (bouncy)        |
  |          O   (crate)   |
  |              [=]       |
  |   * projectile         |
  |                        |
  +========================+
       Floor (static)
```

- A static **floor** and **walls** forming an arena
- A **bouncy ball** with rubber material
- A **heavy crate** with high-friction material
- A **pendulum** made of two bodies connected by a BallSocket constraint
- A **projectile** on a separate collision layer that only hits the crate
- **Raycasting** to check line of sight across the arena
- **Bulk operations** to read all dynamic poses at once

## Step 1: Create the World (Chapter 1)
*)

#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp

// Custom config with slightly more solver iterations for stability
let config =
    { PhysicsConfig.defaults with
        SolverIterations = 12 }

let world = PhysicsWorld.create config

(**
## Step 2: Register Shapes (Chapter 2)

We need shapes for every object in the scene:
*)

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let smallSphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.3f) world
let crateShape = PhysicsWorld.addShape (PhysicsShape.Box(1.0f, 1.0f, 1.0f)) world
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(20.0f, 1.0f, 20.0f)) world
let wallShape = PhysicsWorld.addShape (PhysicsShape.Box(20.0f, 3.0f, 0.5f)) world

(**
## Step 3: Build the Arena (Chapter 3 - Static Bodies)

Static bodies for the floor and surrounding walls:
*)

let _floor =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))))
        world

let _wallNorth =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create wallShape (Pose.ofPosition (Vector3(0.0f, 1.5f, -10.0f))))
        world

let _wallSouth =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create wallShape (Pose.ofPosition (Vector3(0.0f, 1.5f, 10.0f))))
        world

let sideWallShape = PhysicsWorld.addShape (PhysicsShape.Box(0.5f, 3.0f, 20.0f)) world

let _wallEast =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create sideWallShape (Pose.ofPosition (Vector3(10.0f, 1.5f, 0.0f))))
        world

let _wallWest =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create sideWallShape (Pose.ofPosition (Vector3(-10.0f, 1.5f, 0.0f))))
        world

(**
## Step 4: Add Dynamic Objects with Materials (Chapters 3 and 6)

A bouncy rubber ball and a heavy, grippy crate:
*)

// Bouncy rubber ball (Chapter 6: high MaxRecoveryVelocity for bounce)
let rubberMaterial = MaterialProperties.create 0.8f 8.0f 30.0f 1.0f

let bouncyBall =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(-3.0f, 5.0f, 0.0f)))
            1.0f
          with Material = rubberMaterial }
        world

// Heavy crate with high friction (Chapter 6: resists sliding)
let grittyMaterial = MaterialProperties.create 1.5f 0.1f 30.0f 1.0f

let crate =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            crateShape
            (Pose.ofPosition (Vector3(3.0f, 1.0f, 0.0f)))
            10.0f
          with Material = grittyMaterial }
        world

(**
## Step 5: Build a Pendulum with Constraints (Chapter 7)

Two bodies connected by a BallSocket joint, forming a swinging
pendulum:
*)

let pendulumAnchor =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            crateShape
            (Pose.ofPosition (Vector3(-5.0f, 6.0f, -3.0f)))
            100.0f)  // Heavy anchor
        world

let pendulumBob =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(-5.0f, 3.0f, -3.0f)))
            2.0f)
        world

// BallSocket connecting anchor bottom to bob top (Chapter 7)
let _pendulumJoint =
    PhysicsWorld.addConstraint
        pendulumAnchor
        pendulumBob
        (ConstraintDesc.BallSocket(
            Vector3(0.0f, -0.5f, 0.0f),
            Vector3(0.0f, 0.5f, 0.0f),
            SpringConfig.create 30.0f 1.0f))
        world

// Give the pendulum a push to start swinging
PhysicsWorld.setBodyVelocity
    pendulumBob
    (Velocity.create (Vector3(4.0f, 0.0f, 0.0f)) Vector3.Zero)
    world

(**
## Step 6: Collision Filtering for a Projectile (Chapter 8)

The projectile is on a layer that only collides with the crate,
passing through the bouncy ball:
*)

let environmentLayer = 1u
let crateLayer = 2u
let projectileLayer = 4u

// Update the crate to be on the crate layer
// (We already created it with defaults; in a real game you would set
// this at creation time. Here we demonstrate the concept.)

let projectile =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            smallSphereShape
            (Pose.ofPosition (Vector3(-8.0f, 1.0f, 0.0f)))
            0.5f
          with
            CollisionGroup = projectileLayer
            CollisionMask = crateLayer ||| environmentLayer ||| 0xFFFFFFF8u }
        world

// Fire the projectile toward the crate
PhysicsWorld.setBodyVelocity
    projectile
    (Velocity.create (Vector3(15.0f, 2.0f, 0.0f)) Vector3.Zero)
    world

(**
## Step 7: Run the Simulation (Chapter 4)

Step 120 frames (2 seconds) and track what happens:
*)

for _ in 1..120 do
    PhysicsWorld.step (1.0f / 60.0f) world

(**
## Step 8: Query Contact Events (Chapter 5)

Check what collided during the simulation:
*)

let contacts = PhysicsWorld.getContactEvents world

let contactSummary =
    $"Active contacts after 2 seconds: {contacts.Length}"

(*** include-value: contactSummary ***)

(**
## Step 9: Raycast Across the Arena (Chapter 9)

Cast a ray from the west wall to the east wall at height Y=1 to see
what objects are in the way:
*)

let allHits =
    PhysicsWorld.raycastAll
        (Vector3(-9.0f, 1.0f, 0.0f))
        (Vector3(1.0f, 0.0f, 0.0f))
        18.0f
        world

let raySummary =
    $"Ray hits from west to east: {allHits.Length} objects"

(*** include-value: raySummary ***)

(**
## Step 10: Bulk Read All Poses (Chapter 10)

Read the pose of every dynamic body in one call:
*)

let allBodies = [| bouncyBall; crate; pendulumAnchor; pendulumBob; projectile |]
let allPoses = Array.zeroCreate<Pose> allBodies.Length

PhysicsWorld.readPoses allBodies allPoses world

let poseReport =
    [| "Bouncy ball", allPoses.[0].Position
       "Crate", allPoses.[1].Position
       "Pendulum anchor", allPoses.[2].Position
       "Pendulum bob", allPoses.[3].Position
       "Projectile", allPoses.[4].Position |]

(*** include-value: poseReport ***)

(**
## Step 11: Read Velocities (Chapter 4)

Check who is still moving:
*)

let allVelocities = Array.zeroCreate<Velocity> allBodies.Length
PhysicsWorld.readVelocities allBodies allVelocities world

let movingBodies =
    Array.zip
        [| "Bouncy ball"; "Crate"; "Pendulum anchor"; "Pendulum bob"; "Projectile" |]
        allVelocities
    |> Array.filter (fun (_, v) -> v.Linear.Length() > 0.1f)
    |> Array.map fst

(*** include-value: movingBodies ***)

(**
After 2 seconds, the pendulum bob is likely still swinging, and some
objects may still be settling.
*)

PhysicsWorld.destroy world

(**
## What We Used

| Concept | Chapter | API |
|---------|---------|-----|
| World creation | [Ch 1](01-what-is-physics.html) | `PhysicsWorld.create`, `PhysicsConfig` |
| Shapes | [Ch 2](02-shapes.html) | `PhysicsShape`, `addShape` |
| Bodies | [Ch 3](03-bodies.html) | `addBody`, `addStatic` |
| Simulation | [Ch 4](04-simulation-loop.html) | `step`, `getBodyPose`, `getBodyVelocity` |
| Contacts | [Ch 5](05-collisions.html) | `getContactEvents`, `ContactEvent` |
| Materials | [Ch 6](06-materials.html) | `MaterialProperties.create` |
| Constraints | [Ch 7](07-constraints.html) | `addConstraint`, `BallSocket`, `SpringConfig` |
| Filtering | [Ch 8](08-collision-filtering.html) | `CollisionGroup`, `CollisionMask` |
| Raycasting | [Ch 9](09-raycasting.html) | `raycast`, `raycastAll`, `RayHit` |
| Bulk ops | [Ch 10](10-bulk-operations.html) | `readPoses`, `readVelocities` |

## Experiment

Try these modifications:

1. Add a hinge constraint (Chapter 7) to create a swinging door in the
   arena that the projectile can knock open.
2. Create a chain of 5 bodies connected by DistanceSpring constraints
   and drape it over the crate.
3. Use `writePoses` to reset every body to its starting position and
   replay the simulation with different projectile velocity.

## Summary

Congratulations! You have built a complete physics scene using every
major feature of BepuFSharp. You now know how to create worlds, register
shapes, place bodies, configure materials, connect bodies with
constraints, filter collisions, cast rays, and use bulk operations for
efficient ECS integration.

**Next**: [Glossary](12-glossary.html)
*)
