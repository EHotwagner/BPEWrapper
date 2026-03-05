# BepuFSharp

An idiomatic F# wrapper for [BepuPhysics2](https://github.com/bepu/bepuphysics2) v2, designed for data-oriented game engines.

**[Documentation](https://ehotwagner.github.io/BPEWrapper/)** · **[API Reference](https://ehotwagner.github.io/BPEWrapper/reference/index.html)**

## Quick Start

```fsharp
open System.Numerics
open BepuFSharp

let world = PhysicsWorld.create PhysicsConfig.defaults

// Add a floor and a falling sphere
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
let _floor = PhysicsWorld.addStatic (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f)))) world

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let ball = PhysicsWorld.addBody (DynamicBodyDesc.create sphereShape (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f))) 1.0f) world

// Step and read
PhysicsWorld.step (1.0f / 60.0f) world
let pose = PhysicsWorld.getBodyPose ball world
printfn "Ball Y: %f" pose.Position.Y

PhysicsWorld.destroy world
```

## Why Not Use BepuPhysics2 Directly from F#?

BepuPhysics2 is an excellent C# library, but using it directly from F# involves constant friction. BepuFSharp removes that friction while preserving full performance.

### Handle confusion → Compile-time safety

BepuPhysics2 uses `BodyHandle`, `StaticHandle`, and `ConstraintHandle` — all thin `int` wrappers that are trivially interchangeable. Passing a `BodyHandle` where a `StaticHandle` is expected compiles fine in C# and crashes at runtime.

```fsharp
// BepuFSharp: the compiler rejects this
let body: BodyId = ...
PhysicsWorld.removeStatic body world  // FS0001: expected StaticId, got BodyId
```

### 18+ concrete struct types → 2 discriminated unions

In vanilla BepuPhysics2, creating shapes means constructing `Sphere`, `Box`, `Capsule`, `Cylinder`, `ConvexHull`, `Mesh`, `Compound`, `Triangle` — each a separate struct with different generic method paths. Constraints are worse: `BallSocket`, `Hinge`, `Weld`, `DistanceLimit`, `DistanceServo`, `SwingLimit`, `TwistLimit`, `LinearAxisMotor`, `AngularServo`, `PointOnLineServo`, and more.

```fsharp
// BepuFSharp: one type, exhaustive matching
let shape = PhysicsShape.Capsule(0.3f, 1.2f)

match constraint with
| BallSocket(offsetA, offsetB, spring) -> ...
| Hinge(axisA, axisB, oA, oB, spring) -> ...
| Weld _ | DistanceLimit _ | ... -> ...
// compiler warns if you miss a case
```

### Mutable C# patterns → Pipeline-friendly API

BepuPhysics2's API is method-heavy: `simulation.Bodies.Add(bodyDescription)`, `simulation.Solver.Add(handleA, handleB, constraint)`, `simulation.Bodies[handle].Pose`. This doesn't compose in F#.

```fsharp
// BepuFSharp: everything pipelines
world
|> PhysicsWorld.addBody desc
|> ignore

let pose = world |> PhysicsWorld.getBodyPose bodyId
```

### Manual callback structs → Built-in defaults

BepuPhysics2 requires implementing `INarrowPhaseCallbacks` and `IPoseIntegratorCallbacks` as generic struct types before you can create a simulation. This is ~100 lines of boilerplate in C#, and significantly more painful in F# due to struct interface constraints and `byref` parameters.

```fsharp
// BepuFSharp: one line, batteries included
let world = PhysicsWorld.create PhysicsConfig.defaults
// Contact events, collision filtering, per-body materials — all wired up
```

### No event system → Frame-diffed contact events

BepuPhysics2 fires callbacks during multithreaded narrow phase execution. Consuming these safely requires thread-safe buffers and careful lifecycle management. BepuFSharp handles all of this internally with a double-buffered event system that detects began/persisted/ended transitions automatically.

```fsharp
PhysicsWorld.step (1.0f / 60.0f) world
let events = PhysicsWorld.getContactEvents world
for evt in events do
    match evt.EventType with
    | Began -> printfn "Contact started"
    | Ended -> printfn "Contact ended"
    | Persisted -> ()
```

### Per-body allocation on readback → Zero-allocation bulk ops

Synchronizing physics with an ECS requires reading thousands of poses per frame. BepuFSharp provides bulk operations that write directly into pre-allocated arrays — zero managed heap allocation on the hot path.

```fsharp
let poses = Array.zeroCreate<Pose> bodyIds.Length
PhysicsWorld.readPoses bodyIds poses world  // no allocation
```

### Summary

| Concern | Vanilla BepuPhysics2 from F# | BepuFSharp |
|---------|------------------------------|------------|
| Handle safety | `int` wrappers, easy to mix up | `BodyId`, `StaticId`, `ShapeId`, `ConstraintId` — compiler-enforced |
| Shape creation | 8 separate struct types + generic methods | `PhysicsShape` DU with 8 cases |
| Constraint creation | 10+ struct types + generic solver methods | `ConstraintDesc` DU with 10 cases |
| API style | `simulation.Bodies.Add(desc)` | `world \|> PhysicsWorld.addBody desc` |
| Callbacks | ~100 lines of struct boilerplate | Zero config — built-in defaults |
| Contact events | Manual thread-safe buffering | `getContactEvents` returns flat struct array |
| Collision filtering | Implement in callback struct | `CollisionFilter` with `group`/`mask` per body |
| Materials | Implement in callback struct | `MaterialProperties` per body with auto-blending |
| Bulk ECS sync | Manual loop + struct access | `readPoses`/`writePoses` — zero allocation |
| Raycasting | Implement `IRayHitHandler` struct | `raycast`/`raycastAll` returning `RayHit option` / `RayHit[]` |
| FSI scripting | Complex `#r` setup + callback boilerplate | `#load "prelude.fsx"` and go |
| Escape hatch | N/A | `PhysicsWorld.simulation` for raw access |

## Features

- 8 shape types (sphere, box, capsule, cylinder, triangle, convex hull, compound, mesh)
- 10 constraint types (ball socket, hinge, weld, distance limit/spring, swing/twist limits, motors, point-on-line)
- Single-hit and multi-hit raycasting
- 32-layer collision filtering via bitmask
- Per-body material properties with friction/spring blending
- Double-buffered contact events (began/persisted/ended)
- Bulk pose and velocity read/write for ECS integration
- `.fsi` signature files for every public module
- Surface-area baseline tests preventing accidental API drift
- 60 tests (Expecto + FsCheck property tests)
- FSI prelude script + 8 example scripts
- FSharp.Formatting documentation site with literate tutorials and ADRs

## Requirements

- .NET 10.0 SDK
- BepuPhysics 2.4.0 / BepuUtilities 2.4.0 (pulled automatically via NuGet)

## Build

```bash
dotnet build
dotnet test
dotnet pack    # outputs to ~/.local/share/nuget-local/
```

## FSI Scripting

```bash
dotnet fsi scripts/examples/01-hello-physics.fsx
```

Or load the prelude in an interactive session:

```fsharp
#load "scripts/prelude.fsx"
open BepuFSharp
```

## Documentation

The full documentation is hosted at **[ehotwagner.github.io/BPEWrapper](https://ehotwagner.github.io/BPEWrapper/)**.

To build locally:

```bash
dotnet tool restore
dotnet fsdocs build        # output in output/
dotnet fsdocs watch        # live-reload at localhost:8901
```

## License

See [LICENSE](LICENSE) for details.
