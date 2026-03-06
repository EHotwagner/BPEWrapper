(**
---
title: BepuFSharp
category: Overview
categoryindex: 0
index: 0
---
*)

(**
# BepuFSharp

An idiomatic F# wrapper for [BepuPhysics2](https://github.com/bepu/bepuphysics2) v2,
designed for data-oriented game engines.

## Features

- **Functional API** — Pipeline-friendly module functions (`world |> PhysicsWorld.addBody desc`)
- **Type-safe handles** — Opaque `BodyId`, `StaticId`, `ShapeId`, `ConstraintId` prevent handle misuse
- **Discriminated unions** — `PhysicsShape` (8 variants) and `ConstraintDesc` (10 variants) for exhaustive matching
- **Zero-allocation bulk ops** — `readPoses`/`writePoses` populate pre-allocated arrays for game loop sync
- **Contact events** — Double-buffered began/persisted/ended events as flat struct arrays
- **Collision filtering** — 32-layer bitmask system via group/mask fields
- **Raycasting** — Single-hit and multi-hit queries returning typed results
- **Per-body materials** — Friction and spring properties with automatic blending

## Quick Start
*)

#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp

// Create a world with default gravity
let world = PhysicsWorld.create PhysicsConfig.defaults

// Add a floor and a falling sphere
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
let _floor = PhysicsWorld.addStatic (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f)))) world

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let sphere = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f) world

// Step and read pose
PhysicsWorld.step (1.0f / 60.0f) world
let pose = PhysicsWorld.getBodyPose sphere world

(*** include-value: pose ***)

(**
## Documentation

- [Tutorial](tutorial/01-what-is-physics.html) — Step-by-step tutorial covering 3D physics from scratch
- [Getting Started](getting-started.html) — Full walkthrough from zero to simulation
- [Game Loop Integration](game-loop-integration.html) — Bulk operations for data-oriented engines
- [Architecture Decisions](adr/001-mutable-world.html) — Why the world is mutable

## API Reference

See the [API Reference](reference/index.html) generated from `.fsi` signature files.

## Building the Documentation

The documentation site is generated from source using [FSharp.Formatting](https://fsprojects.github.io/FSharp.Formatting/).
It is not committed to the repository — build it locally:

```bash
dotnet tool restore
dotnet fsdocs build
```

Then open `output/index.html` in a browser. For live-reload during editing:

```bash
dotnet fsdocs watch
```
*)
