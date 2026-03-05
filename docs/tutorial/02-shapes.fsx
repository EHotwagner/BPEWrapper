(**
---
title: Shapes
category: Tutorial
categoryindex: 3
index: 2
---
*)

(**
# Shapes

**Prerequisites**: [Chapter 1: What Is a Physics Engine](01-what-is-physics.html)

This chapter explains how physics engines represent geometry using shapes,
and walks through all eight shape types available in BepuFSharp.

## What Is a Shape?

In real life, every object has a physical form — a ball is round, a table is
flat, a cup is hollow. A physics engine needs to know these forms to figure
out when objects collide. But it does not care about color, texture, or fine
visual detail. It only needs a simplified boundary for collision detection.

Think of shapes like **cookie cutters**: they define the outline the engine
uses to detect overlaps, but not the decoration on the cookie.

## Coordinate System

BepuPhysics2 uses a right-handed coordinate system:

```text
        Y (up)
        |
        |
        |_______ X (right)
       /
      /
     Z (toward you)
```

All dimensions are in world units (typically meters). A `Box(2, 1, 3)` is
2 units wide along X, 1 unit tall along Y, and 3 units deep along Z.

**Important**: Box dimensions are **full extents**, not half-extents. A
`Box(2, 1, 3)` stretches from -1 to +1 along X, -0.5 to +0.5 along Y,
and -1.5 to +1.5 along Z.

```text
       +------+
      /|     /|     width = 2 (X)
     / |    / |     height = 1 (Y)
    +------+  |     length = 3 (Z)
    |  +---|--+
    | /    | /
    +------+
```

## The Eight Shape Types

BepuFSharp represents shapes with the `PhysicsShape` discriminated union.
Let's register one of each type with a physics world:
*)

#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp

let world = PhysicsWorld.create PhysicsConfig.defaults

(**
### Sphere

The simplest shape — defined by a single radius. Great for balls, bullets,
and particles.
*)

let sphere = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world

(**
### Box

An axis-aligned box defined by width (X), height (Y), and length (Z).
Use for crates, floors, walls, and platforms.
*)

let box = PhysicsWorld.addShape (PhysicsShape.Box(2.0f, 1.0f, 3.0f)) world

(**
### Capsule

A cylinder with hemispherical caps — defined by radius and length. Commonly
used for character controllers because it slides smoothly over edges.
*)

let capsule = PhysicsWorld.addShape (PhysicsShape.Capsule(0.4f, 1.6f)) world

(**
### Cylinder

A flat-ended cylinder — radius and length. Good for wheels, barrels, and
columns.
*)

let cylinder = PhysicsWorld.addShape (PhysicsShape.Cylinder(0.5f, 2.0f)) world

(**
### Triangle

A single triangle defined by three vertices. Rarely used alone, but is the
building block for meshes.
*)

let triangle =
    PhysicsWorld.addShape
        (PhysicsShape.Triangle(
            Vector3(0.0f, 0.0f, 0.0f),
            Vector3(1.0f, 0.0f, 0.0f),
            Vector3(0.5f, 1.0f, 0.0f)))
        world

(**
### Convex Hull

A shape computed from a cloud of points — the engine finds the tightest
convex boundary that contains all of them. Needs at least 4 non-coplanar
points. Good for irregular objects like rocks.
*)

let hull =
    PhysicsWorld.addShape
        (PhysicsShape.ConvexHull [|
            Vector3(0.0f, 0.0f, 0.0f)
            Vector3(1.0f, 0.0f, 0.0f)
            Vector3(0.5f, 1.0f, 0.0f)
            Vector3(0.5f, 0.5f, 1.0f)
        |])
        world

(**
### Compound

Multiple shapes combined into one, each with a local offset. Useful for
complex objects like a table (a box for the top + four cylinders for legs).

Note: compound children must be shapes already registered with the world.
*)

let legShape = PhysicsWorld.addShape (PhysicsShape.Cylinder(0.05f, 0.8f)) world

let compound =
    PhysicsWorld.addShape
        (PhysicsShape.Compound [|
            { Shape = box; LocalPose = Pose.ofPosition (Vector3(0.0f, 0.5f, 0.0f)) }
            { Shape = legShape; LocalPose = Pose.ofPosition (Vector3(-0.8f, -0.4f, -1.2f)) }
            { Shape = legShape; LocalPose = Pose.ofPosition (Vector3(0.8f, -0.4f, -1.2f)) }
            { Shape = legShape; LocalPose = Pose.ofPosition (Vector3(-0.8f, -0.4f, 1.2f)) }
            { Shape = legShape; LocalPose = Pose.ofPosition (Vector3(0.8f, -0.4f, 1.2f)) }
        |])
        world

(**
### Mesh

A collection of triangles for static terrain or complex geometry. Each
element is a tuple of three vertices.
*)

let mesh =
    PhysicsWorld.addShape
        (PhysicsShape.Mesh [|
            (Vector3(0.0f, 0.0f, 0.0f), Vector3(10.0f, 0.0f, 0.0f), Vector3(5.0f, 0.0f, 10.0f))
            (Vector3(10.0f, 0.0f, 0.0f), Vector3(10.0f, 0.0f, 10.0f), Vector3(5.0f, 0.0f, 10.0f))
        |])
        world

(**
## Shape Lifecycle

Shapes are registered with `addShape` and removed with `removeShape`. A
shape can be shared by multiple bodies — removing a shape that is still in
use will raise an error.
*)

let tempShape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
PhysicsWorld.removeShape tempShape world

(**
## Choosing the Right Shape

| Shape | Best For | Cost |
|-------|----------|------|
| Sphere | Balls, particles, bullets | Cheapest |
| Box | Floors, walls, crates | Cheap |
| Capsule | Characters, limbs | Cheap |
| Cylinder | Wheels, barrels | Moderate |
| Triangle | Building block for meshes | Cheap |
| ConvexHull | Rocks, irregular objects | Moderate |
| Compound | Multi-part objects | Moderate-High |
| Mesh | Static terrain, level geometry | High |

Simpler shapes are faster to test for collisions. Use the simplest shape
that reasonably approximates your object.
*)

PhysicsWorld.destroy world

(**
## Experiment

Try these modifications:

1. Create a `Box(100.0f, 0.1f, 100.0f)` — a thin, wide floor. This is a
   common pattern for ground planes.
2. Create a `ConvexHull` with 8 points forming a rough cube shape. Compare
   it mentally to a `Box` — when would you choose one over the other?
3. Build a compound shape that resembles a dumbbell: two spheres connected
   by a cylinder.

## Summary

Shapes define the collision boundaries of objects. BepuFSharp provides eight
variants through the `PhysicsShape` discriminated union, from simple spheres
to complex meshes. Register shapes with `addShape` and remove them with
`removeShape`.

**Next**: [Bodies](03-bodies.html)
*)
