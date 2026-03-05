(**
---
title: Getting Started
category: Guides
categoryindex: 1
index: 1
---
*)

(**
# Getting Started with BepuFSharp

This guide walks through the core concepts: creating a world, adding shapes and bodies,
stepping the simulation, and reading results.

## Creating a Physics World

A `PhysicsWorld` is the central container holding the simulation, memory pools, and threading.
Create one with `PhysicsConfig.defaults` for standard Earth gravity:
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
Custom configuration is straightforward — override any field:
*)

let customWorld =
    PhysicsWorld.create
        { PhysicsConfig.defaults with
            Gravity = Vector3(0.0f, -20.0f, 0.0f)
            SubstepCount = 4 }

(**
## Registering Shapes

Before creating bodies, register shapes with the world. Shapes are defined via the
`PhysicsShape` discriminated union, which covers 8 variants:
*)

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
let boxShape = PhysicsWorld.addShape (PhysicsShape.Box(2.0f, 1.0f, 3.0f)) world
let capsuleShape = PhysicsWorld.addShape (PhysicsShape.Capsule(0.5f, 2.0f)) world

(**
## Adding Bodies

Bodies come in three flavors, each with a descriptor type:

- **Dynamic** — Has mass, affected by forces and gravity
- **Kinematic** — Infinite mass, moved via pose/velocity (platforms, elevators)
- **Static** — Immovable (floors, walls)
*)

// Dynamic sphere at height 10
let sphere =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f))) 1.0f)
        world

// Static floor
let floor =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create boxShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))))
        world

// Kinematic platform
let platform =
    PhysicsWorld.addKinematicBody
        (KinematicBodyDesc.create boxShape (Pose.ofPosition (Vector3(5.0f, 3.0f, 0.0f))))
        world

(**
## Stepping the Simulation

Advance the simulation with `PhysicsWorld.step`. Pass a time delta in seconds:
*)

for _ in 1..60 do
    PhysicsWorld.step (1.0f / 60.0f) world

(**
## Reading Body State

After stepping, read back poses and velocities:
*)

let spherePose = PhysicsWorld.getBodyPose sphere world
let sphereVel = PhysicsWorld.getBodyVelocity sphere world

(*** include-value: spherePose ***)

(**
## Constraints

Connect two bodies with a constraint. Use the `ConstraintDesc` discriminated union:
*)

let bodyA =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 15.0f, 0.0f))) 1.0f)
        world

let bodyB =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(2.0f, 15.0f, 0.0f))) 1.0f)
        world

let spring = SpringConfig.create 30.0f 1.0f
let _joint =
    PhysicsWorld.addConstraint bodyA bodyB
        (ConstraintDesc.BallSocket(Vector3(1.0f, 0.0f, 0.0f), Vector3(-1.0f, 0.0f, 0.0f), spring))
        world

(**
## Cleanup

Dispose the world when done to release all native resources:
*)

PhysicsWorld.destroy world
PhysicsWorld.destroy customWorld
