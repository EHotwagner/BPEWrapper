(**
---
title: Raycasting and Spatial Queries
category: Tutorial
categoryindex: 3
index: 9
---
*)

(**
# Raycasting and Spatial Queries

**Prerequisites**: [Chapter 1](01-what-is-physics.html),
[Chapter 2: Shapes](02-shapes.html),
[Chapter 3: Bodies](03-bodies.html)

This chapter explains how to fire invisible rays through the physics
world to find out what they hit. This is called **raycasting**.

## What Is a Raycast?

Imagine pointing a laser pointer across a room. The beam travels in a
straight line until it hits something. A raycast does exactly this in
the physics world:

```text
  Origin          Direction
    *-----------+----------->
    |           |
    |     +-----+-----+
    |     |  Hit!      |     The ray starts at Origin,
    |     |  Body A    |     travels along Direction,
    |     +------------+     and reports what it hits.
    |
    +--- max distance ---+
```

You provide:

1. **Origin**: Where the ray starts (a point in world space).
2. **Direction**: Which way it goes (a unit vector).
3. **Max Distance**: How far to check (avoids infinite searches).

The engine returns a **RayHit** with:

- Which body or static was hit
- The world-space hit position
- The surface normal at the hit point
- The distance along the ray

## Common Use Cases

| Use Case | How It Works |
|----------|-------------|
| Line of sight | Cast from character to target; if no hit, target is visible |
| Bullet trace | Cast along firing direction; apply damage to first hit |
| Ground detection | Cast downward from character; distance = height above ground |
| Mouse picking | Cast from camera through mouse position; find clicked object |

## Single Raycast

Let's set up a scene with several objects and cast a ray through it:
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
let boxShape = PhysicsWorld.addShape (PhysicsShape.Box(2.0f, 2.0f, 2.0f)) world
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(20.0f, 1.0f, 20.0f)) world

let _floor =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))))
        world

// Place some targets
let _boxTarget =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            boxShape
            (Pose.ofPosition (Vector3(5.0f, 1.0f, 0.0f)))
            10.0f)
        world

let _sphereTarget =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(10.0f, 1.0f, 0.0f)))
            1.0f)
        world

// Step once to settle the scene
PhysicsWorld.step (1.0f / 60.0f) world

(**
### Casting a Ray

`PhysicsWorld.raycast` returns `RayHit option` — `Some hit` if the ray
hit something, `None` if it missed:
*)

// Cast from the origin, aiming right (+X), max distance 20
let hit =
    PhysicsWorld.raycast
        (Vector3(0.0f, 1.0f, 0.0f))     // origin
        (Vector3(1.0f, 0.0f, 0.0f))     // direction (+X)
        20.0f                             // max distance
        world

(*** include-value: hit ***)

(**
The ray should hit the box target first (at around X=4, since the box
is centered at X=5 and has a half-width of 1).

### Inspecting the Hit

A `RayHit` contains these fields:

- `Body`: `ValueSome bodyId` if a dynamic/kinematic body was hit
- `Static`: `ValueSome staticId` if a static body was hit
- `Position`: World-space point where the ray hit the surface
- `Normal`: Surface normal at the hit point (perpendicular to surface)
- `Distance`: How far along the ray the hit occurred

Let's examine the hit in more detail:
*)

let hitInfo =
    match hit with
    | Some h ->
        $"Distance: {h.Distance:F2}, Position: {h.Position}, Normal: {h.Normal}"
    | None ->
        "Ray missed everything"

(*** include-value: hitInfo ***)

(**
### Handling Misses

If we cast upward where nothing exists, we get `None`:
*)

let missedHit =
    PhysicsWorld.raycast
        (Vector3(0.0f, 1.0f, 0.0f))
        (Vector3(0.0f, 1.0f, 0.0f))     // straight up
        100.0f
        world

(*** include-value: missedHit ***)

(**
## Multi-Hit Raycast

Sometimes you want to find *all* objects along a ray, not just the
closest one. `PhysicsWorld.raycastAll` returns an array of all hits
sorted by distance:
*)

// Cast through the entire scene from left to right
let allHits =
    PhysicsWorld.raycastAll
        (Vector3(-5.0f, 1.0f, 0.0f))    // start behind the targets
        (Vector3(1.0f, 0.0f, 0.0f))     // direction (+X)
        30.0f                             // max distance
        world

(*** include-value: allHits ***)

(**
The array should contain hits for the box and the sphere, sorted by
distance from the ray origin. This is useful for "penetrating" rays
like X-ray vision or finding all objects in a line.

## Ground Detection Example

A common pattern is casting a ray downward from a character to detect
the ground distance:
*)

// Simulate a character at height Y=5
let characterPos = Vector3(0.0f, 5.0f, 0.0f)

let groundHit =
    PhysicsWorld.raycast
        characterPos
        (Vector3(0.0f, -1.0f, 0.0f))    // cast downward
        100.0f
        world

let groundDistance =
    match groundHit with
    | Some h -> $"Ground is {h.Distance:F2} units below"
    | None -> "No ground detected"

(*** include-value: groundDistance ***)

(**
This tells you exactly how far below the ground is, which is essential
for character controllers, camera systems, and ground-snapping logic.

## Hitting Static Bodies

Rays can hit static bodies too. Let's cast at the floor:
*)

let floorHit =
    PhysicsWorld.raycast
        (Vector3(15.0f, 5.0f, 0.0f))    // above the floor, away from other objects
        (Vector3(0.0f, -1.0f, 0.0f))    // downward
        10.0f
        world

let floorHitInfo =
    match floorHit with
    | Some h ->
        let isStatic = h.Static <> ValueNone
        $"Hit static: {isStatic}, Distance: {h.Distance:F2}"
    | None -> "Missed"

(*** include-value: floorHitInfo ***)

(**
When a static body is hit, `RayHit.Static` will be `ValueSome staticId`
and `RayHit.Body` will be `ValueNone`.
*)

PhysicsWorld.destroy world

(**
## Experiment

Try these modifications:

1. Cast a ray at an angle (e.g., direction `Vector3(1, -0.5, 0)`) and
   observe how the hit position and normal change.
2. Add 10 spheres in a line and use `raycastAll` to find all of them
   with a single ray.
3. Cast a ray straight down from a falling object each frame to predict
   when it will hit the ground.

## Summary

Raycasting fires an invisible ray through the physics world.
`PhysicsWorld.raycast` returns the closest hit as `RayHit option`.
`PhysicsWorld.raycastAll` returns all hits sorted by distance. Each
`RayHit` tells you what was hit, where, at what angle, and how far.
Use raycasts for line of sight, bullet traces, ground detection, and
mouse picking.

**Next**: [Bulk Operations and Game Loop Integration](10-bulk-operations.html)
*)
