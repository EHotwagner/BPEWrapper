(**
---
title: Collision Filtering with Layers
category: Tutorial
categoryindex: 3
index: 8
---
*)

(**
# Collision Filtering with Layers

**Prerequisites**: [Chapter 1](01-what-is-physics.html),
[Chapter 3: Bodies](03-bodies.html),
[Chapter 5: Collisions](05-collisions.html)

This chapter explains how to control **which objects can collide with
each other** using collision groups and masks.

## Why Filter Collisions?

Not everything should collide with everything else:

- A player should not collide with their own projectiles.
- A trigger zone should detect overlap but not block movement.
- Teammates should pass through each other but collide with enemies.
- A ghost power-up should ignore walls temporarily.

Without filtering, the engine tests every possible pair. Filtering
lets you skip pairs that should never interact.

## Groups and Masks

BepuFSharp uses a **group/mask** system based on bitwise operations.
Each body has two values:

- **CollisionGroup**: A bit flag identifying which layer this body
  belongs to (e.g., Player = 1, Enemy = 2, Projectile = 4).
- **CollisionMask**: A bitmask saying which groups this body can
  collide *with*.

Two bodies A and B collide only if **both** agree:

```text
  Can Collide = (A.Group AND B.Mask) != 0
            AND (B.Group AND A.Mask) != 0
```

Here is a truth table for a simple three-layer setup:

```text
  Layers:  Player = 1   Enemy = 2   Projectile = 4

  +-------------+-------+-------+------------+
  |             | Player| Enemy | Projectile |
  +-------------+-------+-------+------------+
  | Player      |  NO   |  YES  |    NO      |
  | Enemy       |  YES  |  NO   |    YES     |
  | Projectile  |  NO   |  YES  |    NO      |
  +-------------+-------+-------+------------+

  Player:     Group=1, Mask=2          (collides with Enemy only)
  Enemy:      Group=2, Mask=1+4=5      (collides with Player and Projectile)
  Projectile: Group=4, Mask=2          (collides with Enemy only)
```

## Setting Up Collision Layers

Let's implement this three-layer scenario:
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

// Layer constants
let playerLayer = 1u
let enemyLayer = 2u
let projectileLayer = 4u

// Floor collides with everything
let _floor =
    PhysicsWorld.addStatic
        { StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f)))
          with
            CollisionGroup = playerLayer ||| enemyLayer ||| projectileLayer
            CollisionMask = 0xFFFFFFFFu }
        world

(**
### Creating Bodies with Layers

Each body descriptor has `CollisionGroup` and `CollisionMask` fields
that we set using record update syntax:
*)

// Player: group=1, collides with enemies only (mask=2)
let player =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(0.0f, 2.0f, 0.0f)))
            1.0f
          with
            CollisionGroup = playerLayer
            CollisionMask = enemyLayer ||| playerLayer ||| projectileLayer ||| 0xFFFFFFF8u }
        world

// Enemy: group=2, collides with player and projectiles (mask=5)
let enemy =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(1.5f, 2.0f, 0.0f)))
            1.0f
          with
            CollisionGroup = enemyLayer
            CollisionMask = playerLayer ||| projectileLayer ||| enemyLayer ||| 0xFFFFFFF8u }
        world

// Projectile: group=4, collides with enemies only (mask=2)
let projectile =
    PhysicsWorld.addBody
        { DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(0.0f, 2.0f, 2.0f)))
            0.1f
          with
            CollisionGroup = projectileLayer
            CollisionMask = enemyLayer ||| 0xFFFFFFF8u }
        world

(**
### Testing Collision Filtering

Let's push the projectile toward the enemy and check for contacts:
*)

// Fire the projectile at the enemy
PhysicsWorld.setBodyVelocity
    projectile
    (Velocity.create (Vector3(3.0f, 0.0f, -4.0f)) Vector3.Zero)
    world

// Also push the player toward the projectile path
PhysicsWorld.setBodyVelocity
    player
    (Velocity.create (Vector3(0.0f, 0.0f, 3.0f)) Vector3.Zero)
    world

// Step several frames
for _ in 1..60 do
    PhysicsWorld.step (1.0f / 60.0f) world

let events = PhysicsWorld.getContactEvents world

(*** include-value: events ***)

(**
You should see contact events between the projectile and enemy (they
can collide), but no contacts between the player and projectile (they
are filtered out).

### Verifying Filter Behavior

Let's check positions to confirm the projectile passed through
the player but hit the enemy:
*)

let playerPose = PhysicsWorld.getBodyPose player world
let enemyPose = PhysicsWorld.getBodyPose enemy world
let projectilePose = PhysicsWorld.getBodyPose projectile world

(*** include-value: playerPose ***)
(*** include-value: enemyPose ***)
(*** include-value: projectilePose ***)

(**
The enemy should have been knocked sideways by the projectile impact,
while the player moved independently, unaffected by the projectile.

## Default Collision Behavior

When you use `DynamicBodyDesc.create` without setting `CollisionGroup`
and `CollisionMask`, the defaults are:

- `CollisionGroup = 1u` (layer 1)
- `CollisionMask = 0xFFFFFFFFu` (collides with everything)

This means all bodies collide with all other bodies by default, which
is the expected behavior for most simple scenes.

## Designing Layer Schemes

A common approach uses powers of two for groups:

```text
  Layer 0:  Default     = 1   (0b00000001)
  Layer 1:  Player      = 2   (0b00000010)
  Layer 2:  Enemy       = 4   (0b00000100)
  Layer 3:  Projectile  = 8   (0b00001000)
  Layer 4:  Trigger     = 16  (0b00010000)
  Layer 5:  Environment = 32  (0b00100000)
```

You can combine groups: a body on layers 1 and 2 would have
`CollisionGroup = 2u ||| 4u` (= 6).
*)

PhysicsWorld.destroy world

(**
## Experiment

Try these modifications:

1. Change the projectile's mask to `0xFFFFFFFFu` so it collides with
   everything. Observe that it now hits the player too.
2. Create a "trigger" layer that detects contacts but uses very low
   `MaxRecoveryVelocity` material so objects pass through with minimal
   physical effect.
3. Make two enemies on the same layer with mask excluding their own
   layer — they should pass through each other.

## Summary

Collision filtering controls which body pairs generate contacts. Each
body has a `CollisionGroup` (what layer it is on) and `CollisionMask`
(which layers it interacts with). Two bodies collide only if both
masks agree. Use powers of two for layer values and bitwise OR to
combine them.

**Next**: [Raycasting and Spatial Queries](09-raycasting.html)
*)
