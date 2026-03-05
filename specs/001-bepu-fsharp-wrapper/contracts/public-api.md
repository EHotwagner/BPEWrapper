# Public API Contract: BepuFSharp

**Feature**: 001-bepu-fsharp-wrapper
**Date**: 2026-03-05

This document defines the public API surface of the BepuFSharp wrapper library. Each section corresponds to a `.fsi` signature file that serves as the compiler-enforced contract (Constitution II).

## Module: BepuFSharp.Types

**File**: `Types.fsi`

### Types

- `BodyId` -- opaque struct wrapper for body handles
- `StaticId` -- opaque struct wrapper for static handles
- `ShapeId` -- opaque struct wrapper for shape typed indices (TypeId + Index)
- `ConstraintId` -- opaque struct wrapper for constraint handles
- `Pose` -- struct record: Position (Vector3), Orientation (Quaternion)
- `Velocity` -- struct record: Linear (Vector3), Angular (Vector3)
- `SpringConfig` -- struct record: Frequency (float32), DampingRatio (float32)
- `MaterialProperties` -- struct record: Friction, MaxRecoveryVelocity, SpringFrequency, SpringDampingRatio (all float32)
- `PhysicsConfig` -- record: Gravity (Vector3), SolverIterations (int32), SubstepCount (int32), ThreadCount (int32), Deterministic (bool)
- `ContactEventType` -- DU: Began | Persisted | Ended
- `ContactEvent` -- struct: BodyA (BodyId voption), StaticA (StaticId voption), BodyB (BodyId voption), StaticB (StaticId voption), Normal (Vector3), Depth (float32), EventType (ContactEventType)
- `RayHit` -- struct: Body (BodyId voption), Static (StaticId voption), Position (Vector3), Normal (Vector3), Distance (float32)
- `MotorSettings` -- struct: MaxForce (float32), Damping (float32)
- `CollisionFilter` -- struct: Group (uint32), Mask (uint32)

### Functions

- `Pose.create: Vector3 -> Quaternion -> Pose`
- `Pose.ofPosition: Vector3 -> Pose`
- `Pose.identity: Pose`
- `Velocity.zero: Velocity`
- `Velocity.create: Vector3 -> Vector3 -> Velocity`
- `SpringConfig.create: float32 -> float32 -> SpringConfig`
- `MaterialProperties.defaults: MaterialProperties`
- `PhysicsConfig.defaults: PhysicsConfig`

## Module: BepuFSharp.Shapes

**File**: `Shapes.fsi`

### Types

- `PhysicsShape` -- DU: Sphere | Box | Capsule | Cylinder | Triangle | ConvexHull | Compound | Mesh
- `CompoundChild` -- struct: Shape (ShapeId), LocalPose (Pose)

## Module: BepuFSharp.Bodies

**File**: `Bodies.fsi`

### Types

- `DynamicBodyDesc` -- struct record with shape, pose, velocity, mass, material, collision group/mask, sleep threshold, CCD flag
- `KinematicBodyDesc` -- struct record (same minus mass)
- `StaticBodyDesc` -- struct record with shape, pose, material, collision group/mask

### Functions

- `DynamicBodyDesc.create: ShapeId -> Pose -> float32 -> DynamicBodyDesc`
- `KinematicBodyDesc.create: ShapeId -> Pose -> KinematicBodyDesc`
- `StaticBodyDesc.create: ShapeId -> Pose -> StaticBodyDesc`

## Module: BepuFSharp.Constraints

**File**: `Constraints.fsi`

### Types

- `ConstraintDesc` -- DU: BallSocket | Hinge | Weld | DistanceLimit | DistanceSpring | SwingLimit | TwistLimit | LinearAxisMotor | AngularMotor | PointOnLine

## Module: BepuFSharp.Materials

**File**: `Materials.fsi`

### Functions

- `MaterialProperties.create: friction:float32 -> maxRecoveryVelocity:float32 -> springFrequency:float32 -> springDampingRatio:float32 -> MaterialProperties`

## Internal Modules (no .fsi, no public surface)

- **BepuFSharp.Queries** (`Queries.fs`): Internal IRayHitHandler struct implementations for single-hit and multi-hit raycasting. Public raycast functions are in PhysicsWorld module.
- **BepuFSharp.ContactEvents** (`ContactEvents.fs`): Internal double-buffered ContactEventBuffer. Public getContactEvents function is in PhysicsWorld module.
- **BepuFSharp.Callbacks** (`Callbacks.fs`): Internal [<Struct>] callback implementations for INarrowPhaseCallbacks and IPoseIntegratorCallbacks.
- **BepuFSharp.Interop** (`Interop.fs`): Internal inline conversion helpers between wrapper and BepuPhysics2 types.

## Module: BepuFSharp.Diagnostics

**File**: `Diagnostics.fsi`

### Types

- `PhysicsError` -- DU: InvalidBodyHandle | InvalidStaticHandle | InvalidShapeHandle | InvalidConstraintHandle | NegativeMass | DegenerateShape | ShapeInUse | WorldDisposed | BufferPoolExhausted
- `PhysicsDiagnosticEvent` -- DU: WorldCreated | WorldDisposed | ConstraintAutoRemoved | ShapeRemovalBlocked | BufferPoolWarning

### Functions

- `PhysicsError.describe: PhysicsError -> string`

## Module: BepuFSharp.PhysicsWorld

**File**: `PhysicsWorld.fsi`

### Types

- `PhysicsWorld` -- class implementing IDisposable

### Functions -- Lifecycle

- `PhysicsWorld.create: PhysicsConfig -> PhysicsWorld`
- `PhysicsWorld.createCustom: ...` (generic, accepts user-supplied callback structs)
- `PhysicsWorld.destroy: PhysicsWorld -> unit`

### Functions -- Shapes

- `PhysicsWorld.addShape: PhysicsShape -> PhysicsWorld -> ShapeId`
- `PhysicsWorld.removeShape: ShapeId -> PhysicsWorld -> unit`

### Functions -- Bodies

- `PhysicsWorld.addBody: DynamicBodyDesc -> PhysicsWorld -> BodyId`
- `PhysicsWorld.addKinematicBody: KinematicBodyDesc -> PhysicsWorld -> BodyId`
- `PhysicsWorld.addStatic: StaticBodyDesc -> PhysicsWorld -> StaticId`
- `PhysicsWorld.removeBody: BodyId -> PhysicsWorld -> unit`
- `PhysicsWorld.removeStatic: StaticId -> PhysicsWorld -> unit`

### Functions -- Single-Entity State

- `PhysicsWorld.getBodyPose: BodyId -> PhysicsWorld -> Pose`
- `PhysicsWorld.getBodyVelocity: BodyId -> PhysicsWorld -> Velocity`
- `PhysicsWorld.setBodyPose: BodyId -> Pose -> PhysicsWorld -> unit`
- `PhysicsWorld.setBodyVelocity: BodyId -> Velocity -> PhysicsWorld -> unit`

### Functions -- Bulk State (ECS Integration)

- `PhysicsWorld.readPoses: BodyId[] -> Pose[] -> PhysicsWorld -> unit`
- `PhysicsWorld.readVelocities: BodyId[] -> Velocity[] -> PhysicsWorld -> unit`
- `PhysicsWorld.writePoses: BodyId[] -> Pose[] -> PhysicsWorld -> unit`
- `PhysicsWorld.writeVelocities: BodyId[] -> Velocity[] -> PhysicsWorld -> unit`

### Functions -- Constraints

- `PhysicsWorld.addConstraint: BodyId -> BodyId -> ConstraintDesc -> PhysicsWorld -> ConstraintId`
- `PhysicsWorld.removeConstraint: ConstraintId -> PhysicsWorld -> unit`

### Functions -- Queries

- `PhysicsWorld.raycast: origin:Vector3 -> direction:Vector3 -> maxDistance:float32 -> PhysicsWorld -> RayHit option`
- `PhysicsWorld.raycastAll: origin:Vector3 -> direction:Vector3 -> maxDistance:float32 -> PhysicsWorld -> RayHit[]`

### Functions -- Events

- `PhysicsWorld.getContactEvents: PhysicsWorld -> ContactEvent[]`

### Functions -- Simulation

- `PhysicsWorld.step: float32 -> PhysicsWorld -> unit`

### Functions -- Escape Hatch

- `PhysicsWorld.simulation: PhysicsWorld -> Simulation`
- `PhysicsWorld.bufferPool: PhysicsWorld -> BufferPool`

## Pipeline Convention

All world-mutating functions take `PhysicsWorld` as the last parameter to support F# pipeline syntax:

```fsharp
world
|> PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f)
|> fun shapeId -> ...
```

For functions returning values (like `addBody`), the pattern is:

```fsharp
let bodyId = world |> PhysicsWorld.addBody bodyDesc
```

## Versioning

Public API changes require:
1. `.fsi` file update
2. Surface-area baseline update
3. PR review of baseline diff
4. Migration guidance if breaking
