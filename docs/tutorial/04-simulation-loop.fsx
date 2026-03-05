(**
---
title: The Simulation Loop
category: Tutorial
categoryindex: 3
index: 4
---
*)

(**
# The Simulation Loop

**Prerequisites**: [Chapter 1](01-what-is-physics.html),
[Chapter 2: Shapes](02-shapes.html),
[Chapter 3: Bodies](03-bodies.html)

This chapter explains what happens during a physics step, how timesteps
work, and how to read back positions and velocities after stepping.

## What Happens in a Single Step?

When you call `PhysicsWorld.step`, the engine runs through several stages:

```text
  +-----------------+
  |   Broadphase    |   Find pairs of objects that MIGHT collide
  +--------+--------+   (fast bounding-box overlap test)
           |
           v
  +--------+--------+
  |   Narrowphase   |   Check if those pairs ACTUALLY collide
  +--------+--------+   (exact geometry intersection)
           |
           v
  +--------+--------+
  |   Solver        |   Resolve collisions and constraints
  +--------+--------+   (calculate forces to push objects apart)
           |
           v
  +--------+--------+
  |   Integration   |   Apply velocities to update positions
  +--------+--------+   (move everything forward in time)
```

1. **Broadphase**: Uses bounding boxes to quickly find nearby pairs.
   Most pairs will not collide — this step eliminates the obvious non-collisions.
2. **Narrowphase**: For the remaining pairs, runs exact collision tests
   using the actual shapes. Produces contact points with normals and depths.
3. **Solver**: Takes contact constraints (and any joints) and computes
   forces to keep objects from overlapping and joints from breaking.
4. **Integration**: Applies gravity, adds computed forces to velocities,
   and updates positions from velocities.

## Timesteps: Fixed vs Variable

The `dt` parameter to `step` is the time delta in seconds. Common choices:

- `1.0f / 60.0f` — 60 Hz, the standard for most games
- `1.0f / 120.0f` — 120 Hz, smoother but more expensive
- `1.0f / 30.0f` — 30 Hz, cheaper but less accurate

**Fixed timesteps** (always the same dt) give deterministic, stable results.
**Variable timesteps** (dt matches real frame time) can cause instability
when frames spike. Most games use a fixed physics timestep even if the
rendering frame rate varies.

## Watching a Ball Fall

Let's create a classic scene: a sphere falling onto a floor, and track its
position over time.
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

// Floor centered at Y=-0.5 so its top surface is at Y=0
let _floor =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))))
        world

// Sphere at Y=10
let ball =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(0.0f, 10.0f, 0.0f)))
            1.0f)
        world

(**
### Reading Pose During Simulation

We can check the ball's position at any time with `getBodyPose`:
*)

// Step 30 frames (0.5 seconds)
for _ in 1..30 do
    PhysicsWorld.step (1.0f / 60.0f) world

let poseHalfSec = PhysicsWorld.getBodyPose ball world

(*** include-value: poseHalfSec ***)

(**
After 0.5 seconds of free fall under gravity (9.81 m/s^2), the ball should
have fallen about 1.2 meters from its starting height of 10.

### Continue to Landing
*)

// Step another 90 frames (1.5 more seconds, 2.0 seconds total)
for _ in 1..90 do
    PhysicsWorld.step (1.0f / 60.0f) world

let poseAfterLanding = PhysicsWorld.getBodyPose ball world

(*** include-value: poseAfterLanding ***)

(**
The ball should now be resting on the floor, at approximately Y=0.5
(sphere radius above the floor surface).

### Reading Velocity

Velocity has two components: linear (straight-line motion) and angular
(rotation):
*)

let velocity = PhysicsWorld.getBodyVelocity ball world

(*** include-value: velocity ***)

(**
After landing and settling, both linear and angular velocities should be
near zero.

## Setting Pose and Velocity

You can teleport a body or give it an impulse by setting its pose or
velocity directly:
*)

// Teleport the ball back up
PhysicsWorld.setBodyPose ball (Pose.ofPosition (Vector3(0.0f, 8.0f, 0.0f))) world

// Give it a horizontal velocity
PhysicsWorld.setBodyVelocity
    ball
    (Velocity.create (Vector3(5.0f, 0.0f, 0.0f)) Vector3.Zero)
    world

// Step a bit to see the trajectory
for _ in 1..30 do
    PhysicsWorld.step (1.0f / 60.0f) world

let trajectoryPose = PhysicsWorld.getBodyPose ball world

(*** include-value: trajectoryPose ***)

(**
The ball should have moved to the right (positive X) while also falling
down — a parabolic trajectory, just like throwing a ball sideways.

## Substeps

The `SubstepCount` in `PhysicsConfig` controls how many mini-steps the
solver runs per call to `step`. More substeps = better accuracy for fast
objects and stiff constraints, at the cost of more computation. The default
is 1, which is fine for most scenarios.
*)

PhysicsWorld.destroy world

(**
## Experiment

Try these modifications:

1. Step with `dt = 1.0f / 30.0f` instead of `1.0f / 60.0f`. Does the ball
   land in the same place after the same simulated time?
2. Create two balls at the same height but different masses (1 kg and 100 kg).
   Do they hit the ground at the same time?
3. Give the ball a large upward velocity with `setBodyVelocity` and watch
   it arc up and then fall back down.

## Summary

The simulation loop runs four stages per step: broadphase, narrowphase,
solver, and integration. Use `getBodyPose` and `getBodyVelocity` to read
state, and `setBodyPose` and `setBodyVelocity` to override it. Fixed
timesteps give the most stable results.

**Next**: [Collisions and Contact Events](05-collisions.html)
*)
