(**
---
title: Bodies
category: Tutorial
categoryindex: 3
index: 3
---
*)

(**
# Bodies

**Prerequisites**: [Chapter 1: What Is a Physics Engine](01-what-is-physics.html),
[Chapter 2: Shapes](02-shapes.html)

This chapter explains the three kinds of bodies in a physics engine and how
to create them using BepuFSharp.

## What Is a Body?

A shape defines *what something looks like* to the collision system. A body
defines *how it behaves*. It combines a shape with a position, orientation,
mass, and velocity. Think of the shape as the mold and the body as the
actual object placed in the world.

## Three Kinds of Bodies

Physics engines categorize bodies by how they respond to forces:

```text
  +--------------------+-------------------+-------------------+
  |     Dynamic        |    Kinematic      |     Static        |
  +--------------------+-------------------+-------------------+
  | Affected by gravity| NOT affected by   | Does not move     |
  | and collisions     | gravity           | at all            |
  |                    |                   |                   |
  | Has finite mass    | Has infinite mass | Has infinite mass |
  |                    |                   |                   |
  | Example:           | Example:          | Example:          |
  | thrown ball,        | moving platform,  | floor, wall,     |
  | ragdoll, debris    | elevator, door    | terrain           |
  +--------------------+-------------------+-------------------+
```

- **Dynamic**: A thrown ball — gravity pulls it, collisions deflect it.
  You give it a mass and the engine does the rest.
- **Kinematic**: A moving elevator — you control its velocity directly.
  It pushes dynamic objects out of the way but is not pushed back.
- **Static**: The floor — it never moves. Other objects collide with it
  but it stays put. Cheapest to simulate.
*)

#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp

let world = PhysicsWorld.create PhysicsConfig.defaults

// Register shapes we will use for bodies
let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let boxShape = PhysicsWorld.addShape (PhysicsShape.Box(10.0f, 0.5f, 10.0f)) world

(**
## Creating a Dynamic Body

A dynamic body needs a shape, a starting pose (position + orientation), and
a mass. `DynamicBodyDesc.create` fills in sensible defaults for everything
else:
*)

let ball =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f)))
            1.0f)
        world

(**
We placed a 1 kg sphere at Y=5. When we step the simulation later, gravity
will pull it downward.

## Creating a Static Body

A static body has no mass — it is immovable. Perfect for floors and walls:
*)

let floor =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create
            boxShape
            (Pose.ofPosition (Vector3(0.0f, -0.25f, 0.0f))))
        world

(**
The floor sits at Y=-0.25 (centered on its 0.5-unit height), forming a
surface at Y=0.

## Creating a Kinematic Body

A kinematic body is moved by setting its velocity directly. The engine
will not apply gravity to it, but it will push dynamic objects:
*)

let capsuleShape = PhysicsWorld.addShape (PhysicsShape.Capsule(0.4f, 1.0f)) world

let platform =
    PhysicsWorld.addKinematicBody
        (KinematicBodyDesc.create
            capsuleShape
            (Pose.ofPosition (Vector3(3.0f, 2.0f, 0.0f))))
        world

(**
## Removing Bodies

When a body is no longer needed, remove it to free resources. Dynamic and
kinematic bodies use `removeBody`; static bodies use `removeStatic`:
*)

let tempShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.3f) world
let tempBody =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            tempShape
            (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f)))
            0.5f)
        world

PhysicsWorld.removeBody tempBody world

(**
Removing a body also automatically removes any constraints attached to it.

## Putting It Together

Let's verify our scene by stepping it and checking the ball's position.
After 60 steps at 1/60s (one second of simulation), the ball should have
fallen under gravity:
*)

for _ in 1..60 do
    PhysicsWorld.step (1.0f / 60.0f) world

let ballPose = PhysicsWorld.getBodyPose ball world

(*** include-value: ballPose ***)

(**
The ball started at Y=5 and fell for one second under gravity. It should
have landed on the floor (near Y=0.5, since the sphere has radius 0.5)
or be resting on it.
*)

PhysicsWorld.destroy world

(**
## Experiment

Try these modifications:

1. Change the ball's mass from `1.0f` to `100.0f`. Does it fall faster?
   (Hint: in a vacuum, all objects fall at the same rate — but the mass
   affects how collisions and constraints respond.)
2. Create two dynamic spheres at different heights and watch them both
   fall onto the floor.
3. Set the kinematic platform's velocity using `PhysicsWorld.setBodyVelocity`
   to make it move sideways. Place a dynamic body on top and see if it
   rides along.

## Summary

Bodies give shapes a physical presence: position, mass, and velocity.
Dynamic bodies respond to forces, kinematic bodies are script-controlled
movers, and static bodies are immovable fixtures. Use `addBody`,
`addKinematicBody`, and `addStatic` to place them in the world.

**Next**: [The Simulation Loop](04-simulation-loop.html)
*)
