# Quickstart: BepuFSharp

**Feature**: 001-bepu-fsharp-wrapper
**Date**: 2026-03-05

## Prerequisites

- .NET 8.0 SDK
- F# 8.0 (included with .NET 8.0 SDK)

## Build

```bash
dotnet build BepuFSharp/BepuFSharp.fsproj
dotnet pack BepuFSharp/BepuFSharp.fsproj
```

The `.nupkg` is output to `~/.local/share/nuget-local/`.

## Run Tests

```bash
dotnet test BepuFSharp.Tests/BepuFSharp.Tests.fsproj
```

## Hello Physics (10 lines)

```fsharp
open BepuFSharp

// Create a world with default settings (Earth gravity, 8 solver iterations)
let world = PhysicsWorld.create PhysicsConfig.defaults

// Add a floor
let floorShape = world |> PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f))
let _floor = world |> PhysicsWorld.addStatic (StaticBodyDesc.create floorShape (Pose.ofPosition(System.Numerics.Vector3(0.0f, -0.5f, 0.0f))))

// Add a falling sphere
let sphereShape = world |> PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f)
let ball = world |> PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition(System.Numerics.Vector3(0.0f, 10.0f, 0.0f))) 1.0f)

// Step the simulation
world |> PhysicsWorld.step (1.0f / 60.0f)

// Read the ball's position
let pose = world |> PhysicsWorld.getBodyPose ball
printfn "Ball position: %A" pose.Position

// Cleanup
world |> PhysicsWorld.destroy
```

## FSI Usage

```fsharp
#load "scripts/prelude.fsx"
open BepuFSharp

let world = PhysicsWorld.create PhysicsConfig.defaults
// ... prototype interactively
world |> PhysicsWorld.destroy
```

## Key Patterns

### Adding bodies (pipeline style)

```fsharp
let shapeId = world |> PhysicsWorld.addShape (PhysicsShape.Capsule(0.5f, 1.8f))
let bodyId = world |> PhysicsWorld.addBody (DynamicBodyDesc.create shapeId pose 75.0f)
```

### Bulk ECS sync

```fsharp
let bodyIds = [| body1; body2; body3 |]
let poses = Array.zeroCreate<Pose> bodyIds.Length
world |> PhysicsWorld.readPoses bodyIds poses
// poses array is now populated -- copy to ECS transform components
```

### Constraints

```fsharp
let joint = ConstraintDesc.BallSocket {
    LocalOffsetA = Vector3(0.0f, 1.0f, 0.0f)
    LocalOffsetB = Vector3(0.0f, -1.0f, 0.0f)
    SpringSettings = SpringConfig.create 30.0f 1.0f
}
let constraintId = world |> PhysicsWorld.addConstraint bodyA bodyB joint
```

### Raycasting

```fsharp
match world |> PhysicsWorld.raycast origin direction 100.0f with
| Some hit -> printfn "Hit at %A, distance %f" hit.Position hit.Distance
| None -> printfn "Miss"
```

### Collision events

```fsharp
world |> PhysicsWorld.step dt
let events = world |> PhysicsWorld.getContactEvents
for e in events do
    match e.EventType with
    | ContactEventType.Began -> printfn "Contact began: %A" e.Normal
    | _ -> ()
```

## Build Documentation

```bash
dotnet tool restore
dotnet fsdocs build
```
