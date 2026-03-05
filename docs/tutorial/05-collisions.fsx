(**
---
title: Collisions and Contact Events
category: Tutorial
categoryindex: 3
index: 5
---
*)

(**
# Collisions and Contact Events

**Prerequisites**: [Chapter 1](01-what-is-physics.html),
[Chapter 2: Shapes](02-shapes.html),
[Chapter 3: Bodies](03-bodies.html),
[Chapter 4: Simulation Loop](04-simulation-loop.html)

This chapter explains how the physics engine detects collisions and how
you can observe them through **contact events**.

## How Collision Detection Works

When two objects overlap, the engine needs to figure out *where* they
touch and *how deeply* they penetrate. This happens in two stages:

```text
  +--------------------------+
  |   Broadphase             |   Fast bounding-box test
  |   "Are they even close?" |   Eliminates most pairs
  +------------+-------------+
               |
               v
  +------------+-------------+
  |   Narrowphase            |   Exact geometry test
  |   "Do they actually      |   Produces contact points
  |    touch?"               |   with normals and depths
  +--------------------------+
```

The **broadphase** wraps every shape in an axis-aligned bounding box and
checks for overlaps. This is cheap and eliminates most pairs. The
**narrowphase** then runs precise geometry tests on the remaining
candidates, producing **contact points**.

A contact point has two important pieces of information:

```text
       Body A           Body B
      +------+        +------+
      |      |<--+--->|      |
      |      | normal |      |
      +------+  |     +------+
                 |
           depth = overlap distance
```

- **Normal**: The direction to push the objects apart.
- **Depth**: How far they overlap.

## Contact Event Lifecycle

BepuFSharp tracks contacts across frames and classifies them:

| Event Type | Meaning | When It Fires |
|------------|---------|---------------|
| `Began` | First frame of contact | Objects just touched |
| `Persisted` | Ongoing contact | Objects still touching |
| `Ended` | Contact ceased | Objects separated |

This lifecycle lets you trigger game logic: play a sound on `Began`,
apply damage while `Persisted`, stop effects on `Ended`.

## Observing Collisions

Let's drop a sphere onto a floor and watch the contact events.
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

let _floor =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))))
        world

// Drop a sphere from height 3
let ball =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(0.0f, 3.0f, 0.0f)))
            1.0f)
        world

(**
### Before Contact

Let's step a few frames and check for events. The ball is still falling,
so there should be no contacts yet:
*)

for _ in 1..20 do
    PhysicsWorld.step (1.0f / 60.0f) world

let earlyEvents = PhysicsWorld.getContactEvents world

(*** include-value: earlyEvents ***)

(**
An empty array — the ball has not reached the floor yet.

### Moment of Impact

Now step until the ball hits the floor. After about 0.5 seconds of free
fall from height 3, it should make contact:
*)

for _ in 1..40 do
    PhysicsWorld.step (1.0f / 60.0f) world

let impactEvents = PhysicsWorld.getContactEvents world

(*** include-value: impactEvents ***)

(**
### Reading Contact Event Fields

Each `ContactEvent` contains:

- `BodyA` / `BodyB`: The body identifiers involved (as `ValueOption`)
- `StaticA` / `StaticB`: Static identifiers (if a static body is involved)
- `Normal`: Contact normal direction (from A toward B)
- `Depth`: Penetration depth
- `EventType`: `Began`, `Persisted`, or `Ended`

Since one side is a static floor, `StaticA` or `StaticB` will have a
value while the corresponding body field will be `ValueNone`.

### Watching the Lifecycle

Let's step a few more frames to see the contact persist, then remove
the ball to see it end:
*)

// Step a few more frames — contacts should persist
PhysicsWorld.step (1.0f / 60.0f) world
let persistEvents = PhysicsWorld.getContactEvents world

(*** include-value: persistEvents ***)

(**
After the ball settles on the floor, contacts persist each frame.

### Multiple Colliding Bodies

Let's add a second sphere that will collide with the first:
*)

let ball2 =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f)))
            1.0f)
        world

// Step until the second ball lands
for _ in 1..90 do
    PhysicsWorld.step (1.0f / 60.0f) world

let multiEvents = PhysicsWorld.getContactEvents world

(*** include-value: multiEvents ***)

(**
You should see contact events for multiple pairs: ball-floor,
ball2-floor, and potentially ball-ball2.
*)

PhysicsWorld.destroy world

(**
## Experiment

Try these modifications:

1. Create three spheres at different heights and observe which contact
   pairs appear as they land and stack up.
2. Check the `Normal` field of a contact event — it should point upward
   (positive Y) for objects resting on the floor.
3. Give a ball a horizontal velocity so it slides across the floor.
   Do contact events fire every frame while it slides?

## Summary

The physics engine detects collisions in two stages: broadphase finds
nearby pairs, narrowphase produces exact contact points. BepuFSharp
exposes these as `ContactEvent` values with a lifecycle of `Began`,
`Persisted`, and `Ended`. Use `PhysicsWorld.getContactEvents` after
each step to inspect them.

**Next**: [Materials: Friction and Restitution](06-materials.html)
*)
