(**
---
title: Materials - Friction and Restitution
category: Tutorial
categoryindex: 3
index: 6
---
*)

(**
# Materials: Friction and Restitution

**Prerequisites**: [Chapter 1](01-what-is-physics.html),
[Chapter 2: Shapes](02-shapes.html),
[Chapter 3: Bodies](03-bodies.html),
[Chapter 5: Collisions](05-collisions.html)

This chapter explains how **materials** control what happens when two
objects collide. Do they stick together, slide, or bounce?

## What Are Material Properties?

When a ball hits the floor, the outcome depends on what the ball and
floor are "made of":

- A **rubber ball** bounces high.
- A **bowling ball** barely bounces.
- A **hockey puck on ice** slides with almost no resistance.
- A **shoe on sandpaper** grips and stops quickly.

In a physics engine, we model this with two key properties:

```text
  Friction (how much objects resist sliding)
  =========================================
  Low (ice)           Medium (wood)          High (sandpaper)
  |---------------------|---------------------|
  0.0                  0.5                   1.0+

  Restitution / Bounciness (how much energy is returned)
  =====================================================
  Low (clay)           Medium (basketball)    High (rubber ball)
  |---------------------|---------------------|
  slow recovery        moderate recovery      fast recovery
```

## How BepuFSharp Models Materials

BepuFSharp uses `MaterialProperties` with four fields:

| Field | What It Controls | Typical Range |
|-------|-----------------|---------------|
| `Friction` | Sliding resistance | 0.0 (ice) to 1.0+ (sandpaper) |
| `MaxRecoveryVelocity` | Bounciness cap | 0.0 (no bounce) to 10.0+ (super bouncy) |
| `SpringFrequency` | Contact stiffness | 15-60 Hz (higher = harder surface) |
| `SpringDampingRatio` | Energy absorption | 0.5-1.0 (1.0 = no oscillation) |

The spring model is how BepuPhysics2 resolves contacts internally: when
two objects overlap, a virtual spring pushes them apart. The frequency
controls how stiff that spring is, and the damping ratio controls how
quickly it settles.

## Default Materials

Let's start with the defaults and then customize:
*)

#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp

let world = PhysicsWorld.create PhysicsConfig.defaults

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(20.0f, 1.0f, 20.0f)) world

// Floor with default material
let _floor =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))))
        world

(**
`MaterialProperties.defaults` gives us:

- Friction = 1.0
- MaxRecoveryVelocity = 2.0
- SpringFrequency = 30 Hz
- SpringDampingRatio = 1.0

This is a reasonable "average" material.

## Bouncy Ball vs Heavy Crate

Let's create two objects with different materials and compare how they
behave when dropped:
*)

// A bouncy rubber ball — high recovery velocity, moderate spring
let bouncyMaterial =
    MaterialProperties.create 0.8f 8.0f 30.0f 1.0f

let bouncyBall =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(-2.0f, 5.0f, 0.0f)))
            1.0f
          with Material = bouncyMaterial }
        world

// A heavy crate — high friction, almost no bounce
let crateMaterial =
    MaterialProperties.create 1.2f 0.1f 30.0f 1.0f

let boxShape = PhysicsWorld.addShape (PhysicsShape.Box(1.0f, 1.0f, 1.0f)) world

let crate =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            boxShape
            (Pose.ofPosition (Vector3(2.0f, 5.0f, 0.0f)))
            5.0f
          with Material = crateMaterial }
        world

(**
### Drop and Compare

After dropping both from the same height, the bouncy ball should
bounce back up while the crate thuds to a stop:
*)

// Step 2 seconds
for _ in 1..120 do
    PhysicsWorld.step (1.0f / 60.0f) world

let bouncyPose = PhysicsWorld.getBodyPose bouncyBall world
let cratePose = PhysicsWorld.getBodyPose crate world

(*** include-value: bouncyPose ***)
(*** include-value: cratePose ***)

(**
The bouncy ball's Y position may still be above the resting height
if it is mid-bounce. The crate should be resting near Y=0.5 (half
its height above the floor surface).

## Friction Comparison: Ice vs Sandpaper

Let's create a sloped floor and slide two objects down it with
different friction values:
*)

PhysicsWorld.destroy world

let world2 = PhysicsWorld.create PhysicsConfig.defaults

let sShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world2
let rampShape = PhysicsWorld.addShape (PhysicsShape.Box(10.0f, 0.5f, 4.0f)) world2

// Tilted ramp (rotated slightly around Z axis)
let rampOrientation =
    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.3f) // ~17 degrees

let _ramp =
    PhysicsWorld.addStatic
        { StaticBodyDesc.create rampShape (Pose.create (Vector3(0.0f, 2.0f, 0.0f)) rampOrientation)
          with Material = MaterialProperties.create 0.5f 0.0f 30.0f 1.0f }
        world2

// Icy ball — almost no friction
let icyBall =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            sShape
            (Pose.ofPosition (Vector3(-3.0f, 4.0f, 0.0f)))
            1.0f
          with Material = MaterialProperties.create 0.02f 0.0f 30.0f 1.0f }
        world2

// Grippy ball — high friction
let gripBall =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            sShape
            (Pose.ofPosition (Vector3(-3.0f, 4.0f, 2.0f)))
            1.0f
          with Material = MaterialProperties.create 2.0f 0.0f 30.0f 1.0f }
        world2

// Step 3 seconds
for _ in 1..180 do
    PhysicsWorld.step (1.0f / 60.0f) world2

let icyPos = PhysicsWorld.getBodyPose icyBall world2
let gripPos = PhysicsWorld.getBodyPose gripBall world2

(*** include-value: icyPos ***)
(*** include-value: gripPos ***)

(**
The icy ball should have slid much farther down the ramp than the
grippy ball, which may have barely moved due to high friction.

## Creating Materials with `MaterialProperties.create`

The function signature is:

```fsharp
MaterialProperties.create:
    friction: float32 ->
    maxRecoveryVelocity: float32 ->
    springFrequency: float32 ->
    springDampingRatio: float32 ->
    MaterialProperties
```

Common material recipes:

| Material | Friction | MaxRecovery | SpringFreq | Damping |
|----------|----------|-------------|------------|---------|
| Ice | 0.02 | 0.0 | 30 | 1.0 |
| Rubber | 0.8 | 8.0 | 30 | 1.0 |
| Wood | 0.5 | 1.0 | 30 | 1.0 |
| Metal | 0.3 | 3.0 | 60 | 1.0 |
| Sandpaper | 1.5 | 0.0 | 30 | 1.0 |
*)

PhysicsWorld.destroy world2

(**
## Experiment

Try these modifications:

1. Set friction to `0.0f` on both the ball and the floor. Drop the ball
   with a horizontal velocity — it should slide forever.
2. Set `MaxRecoveryVelocity` to `20.0f` and drop a ball from height 10.
   How high does it bounce?
3. Lower `SpringFrequency` to `5.0f` — objects will feel "mushy" and
   sink into each other slightly before bouncing back.

## Summary

Materials control how contacts resolve: friction determines sliding
resistance, and `MaxRecoveryVelocity` controls bounciness. The contact
spring model (frequency + damping) determines surface stiffness. Use
`MaterialProperties.create` to define custom materials and assign them
via the `Material` field on body descriptors.

**Next**: [Constraints and Joints](07-constraints.html)
*)
