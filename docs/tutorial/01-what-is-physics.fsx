(**
---
title: What Is a Physics Engine
category: Tutorial
categoryindex: 3
index: 1
---
*)

(**
# What Is a Physics Engine

**Prerequisites**: None — this is the starting point.

This chapter explains what a physics engine is, why games need one,
and introduces BepuFSharp with a minimal working example. It also
covers the F# constructs you will see throughout the tutorial.

## Why Physics Engines Exist

Imagine tossing a ball across a room. It arcs through the air, bounces off
the floor, and rolls to a stop. You intuitively understand the trajectory,
the bounce, the friction — but a computer does not. A **physics engine** is
the software that teaches the computer these rules.

In a game or simulation, every frame the engine answers one question:
*"Given where everything is right now, where will it be a fraction of a
second from now?"*

It does this by simulating forces (gravity, collisions, springs) and updating
positions and orientations for every object in the world, many times per
second.

## Where Physics Fits in a Game Loop

A typical game loop looks like this:

```text
  +------------------+
  |   Read Input     |
  +--------+---------+
           |
           v
  +--------+---------+
  |  Update Game     |
  |  Logic           |
  +--------+---------+
           |
           v
  +--------+---------+
  |  Step Physics    |  <--- The physics engine runs here
  +--------+---------+
           |
           v
  +--------+---------+
  |  Render Frame    |
  +------------------+
```

The physics step happens once per frame (or at a fixed interval). The engine
receives the current state of the world, advances time by a small increment
— the **timestep** — and produces new positions and velocities for every
object.
*)

#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp

(**
## A Quick F# Primer

Before we write code, here are the four F# constructs you will see
throughout this tutorial.

**`let` bindings** — give names to values (like `const` in other languages):
*)

let greeting = "Hello, physics!"

(**
**Records** — lightweight data types with named fields:
*)

type MyPoint = { X: float32; Y: float32 }
let origin = { X = 0.0f; Y = 0.0f }

(**
**Discriminated unions** — types that can be one of several named cases:
*)

type Shape =
    | Circle of radius: float32
    | Rectangle of width: float32 * height: float32

let myShape = Circle 2.5f

(**
**The pipe operator `|>`** — passes a value into the next function, enabling
left-to-right reading:
*)

let doubled = 5 |> fun x -> x * 2

(**
These four constructs are all you need to follow this tutorial. For more
on F#, see these resources:

- [F# for Fun and Profit](https://fsharpforfunandprofit.com/) — beginner-friendly guide
- [Microsoft F# Tour](https://learn.microsoft.com/dotnet/fsharp/tour) — official language tour
- [F# Cheat Sheet](https://dungpa.github.io/fsharp-cheatsheet/) — quick syntax reference

## Your First Physics World

BepuFSharp wraps BepuPhysics2 in an idiomatic F# API. The central object is
`PhysicsWorld` — it holds the simulation, memory pools, and threading.
Let's create one, step it, and tear it down:
*)

// Create a world with Earth-like gravity (0, -9.81, 0)
let world = PhysicsWorld.create PhysicsConfig.defaults

(**
`PhysicsConfig.defaults` gives us standard Earth gravity pointing downward
along the Y axis, 8 solver iterations, and automatic thread detection.

Now step the simulation forward by 1/60th of a second (a single frame at
60 FPS):
*)

PhysicsWorld.step (1.0f / 60.0f) world

(**
Nothing visible happened — we have no objects yet! But the engine ran a full
simulation step: checking for collisions, solving constraints, and
integrating velocities. We will add objects in the next chapters.

### Customizing Gravity

You can override any config field. Here is a world with double gravity:
*)

let heavyWorld =
    PhysicsWorld.create
        { PhysicsConfig.defaults with
            Gravity = Vector3(0.0f, -19.62f, 0.0f) }

PhysicsWorld.step (1.0f / 60.0f) heavyWorld

(**
### Cleaning Up

Always destroy worlds when done. This releases native memory and threads:
*)

PhysicsWorld.destroy heavyWorld
PhysicsWorld.destroy world

(**
## Experiment

Try these modifications:

1. Change gravity to `Vector3(0.0f, 0.0f, 0.0f)` — a zero-gravity world.
   What would happen to objects? (We will find out in Chapter 4.)
2. Change gravity to `Vector3(0.0f, 9.81f, 0.0f)` — gravity pointing *up*.
3. Set `SubstepCount = 4` in the config. This makes the simulation more
   accurate at the cost of more computation.

## Summary

A physics engine simulates forces and motion so your game does not have to.
BepuFSharp provides an idiomatic F# wrapper: `PhysicsWorld.create` makes a
world, `PhysicsWorld.step` advances time, and `PhysicsWorld.destroy` cleans
up.

**Next**: [Shapes](02-shapes.html)
*)
