# Data Model: BepuPhysics2 F# Wrapper

**Feature**: 001-bepu-fsharp-wrapper
**Date**: 2026-03-05

## Entity Diagram

```
PhysicsConfig ──creates──> PhysicsWorld
                              │
              ┌───────────────┼───────────────┐
              │               │               │
          has many         has many        has many
              │               │               │
          BodyId          StaticId       ConstraintId
              │               │               │
          references      references     connects two
              │               │           BodyIds
              ▼               ▼
           ShapeId ◄──────ShapeId
              │
          registered in
              │
              ▼
        PhysicsShape (DU)

    MaterialProperties ──assigned to──> BodyId / StaticId
    ContactEvent ──produced per step──> PhysicsWorld
```

## Core Value Types

### Pose `[<Struct>]`

| Field | Type | Description |
|-------|------|-------------|
| Position | Vector3 | World-space position (x, y, z) |
| Orientation | Quaternion | Rotation quaternion (x, y, z, w) |

**Conversions**: Bidirectional with BepuPhysics2 `RigidPose`.
**Invariants**: Quaternion should be normalized (enforced at boundary, not per-frame).

### Velocity `[<Struct>]`

| Field | Type | Description |
|-------|------|-------------|
| Linear | Vector3 | Linear velocity (units/second) |
| Angular | Vector3 | Angular velocity (radians/second) |

**Conversions**: Bidirectional with BepuPhysics2 `BodyVelocity`.

### SpringConfig `[<Struct>]`

| Field | Type | Description |
|-------|------|-------------|
| Frequency | float32 | Oscillation frequency in Hz |
| DampingRatio | float32 | Damping ratio (1.0 = critically damped) |

**Conversions**: Maps to BepuPhysics2 `SpringSettings`.

### MaterialProperties `[<Struct>]`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Friction | float32 | 1.0f | Coulomb friction coefficient |
| MaxRecoveryVelocity | float32 | 2.0f | Maximum bounce recovery speed |
| SpringFrequency | float32 | 30.0f | Contact spring frequency in Hz |
| SpringDampingRatio | float32 | 1.0f | Contact spring damping ratio |

**Validation**: All fields must be >= 0.

## Opaque Handle Types

### BodyId `[<Struct>]`

Single-case struct DU wrapping an `int32` (from BepuPhysics2 `BodyHandle`).
References one dynamic or kinematic body in the world.

### StaticId `[<Struct>]`

Single-case struct DU wrapping an `int32` (from BepuPhysics2 `StaticHandle`).
References one static body in the world.

### ShapeId `[<Struct>]`

| Field | Type | Description |
|-------|------|-------------|
| TypeId | int32 | Shape type discriminator |
| Index | int32 | Index within the type-specific shape batch |

Maps to BepuPhysics2 `TypedIndex`. Referenced by bodies and statics. Shared across multiple bodies.

### ConstraintId `[<Struct>]`

Single-case struct DU wrapping an `int32` (from BepuPhysics2 `ConstraintHandle`).
Connects two `BodyId` handles.

## Discriminated Union Types

### PhysicsShape

| Case | Fields | Description |
|------|--------|-------------|
| Sphere | radius: float32 | Sphere with given radius |
| Box | width: float32, height: float32, length: float32 | Axis-aligned box (wrapper accepts full dimensions; internally halved for BepuPhysics2 half-extents) |
| Capsule | radius: float32, length: float32 | Capsule (two hemispheres + cylinder) |
| Cylinder | radius: float32, length: float32 | Cylinder |
| Triangle | a: Vector3, b: Vector3, c: Vector3 | Single triangle |
| ConvexHull | points: Vector3[] | Convex hull from point cloud |
| Compound | children: CompoundChild[] | Compound of child shapes with offsets |
| Mesh | triangles: Triangle[] | Triangle mesh |

**Validation**:
- Sphere: radius > 0
- Box: all dimensions > 0
- Capsule: radius > 0, length > 0
- Cylinder: radius > 0, length > 0
- ConvexHull: points.Length >= 4
- Compound: children.Length >= 1
- Mesh: triangles.Length >= 1

**Note**: ConvexHull, Compound, and Mesh cases are heap-allocated (reference types in the DU). Primitive shape cases (Sphere, Box, Capsule, Cylinder, Triangle) could be struct DUs but F# does not support struct DU cases selectively.

**ComputeInertia limitation**: Only convex shapes (Sphere, Box, Capsule, Cylinder, Triangle, ConvexHull) support `ComputeInertia(mass)`. Mesh and Compound do not implement `IConvexShape`. For Compound bodies, inertia must be computed from constituent shapes. For Mesh bodies, explicit inertia must be provided or computed externally.

### CompoundChild `[<Struct>]`

| Field | Type | Description |
|-------|------|-------------|
| Shape | ShapeId | Registered shape for this child |
| LocalPose | Pose | Offset from compound origin |

### ConstraintDesc

| Case | Key Fields | Description |
|------|-----------|-------------|
| BallSocket | LocalOffsetA: Vector3, LocalOffsetB: Vector3, SpringSettings: SpringConfig | Ball-and-socket joint |
| Hinge | LocalHingeAxisA: Vector3, LocalHingeAxisB: Vector3, LocalOffsetA: Vector3, LocalOffsetB: Vector3, SpringSettings: SpringConfig | Hinge joint with axis |
| Weld | LocalOffset: Vector3, LocalOrientation: Quaternion, SpringSettings: SpringConfig | Rigid attachment |
| DistanceLimit | LocalOffsetA: Vector3, LocalOffsetB: Vector3, MinDistance: float32, MaxDistance: float32, SpringSettings: SpringConfig | Distance range constraint |
| DistanceSpring | LocalOffsetA: Vector3, LocalOffsetB: Vector3, TargetDistance: float32, SpringSettings: SpringConfig | Spring-like distance constraint |
| SwingLimit | AxisLocalA: Vector3, AxisLocalB: Vector3, MaxSwingAngle: float32, SpringSettings: SpringConfig | Cone-shaped angular limit |
| TwistLimit | LocalAxisA: Vector3, LocalAxisB: Vector3, MinAngle: float32, MaxAngle: float32, SpringSettings: SpringConfig | Twist range limit |
| LinearAxisMotor | LocalOffsetA: Vector3, LocalOffsetB: Vector3, LocalAxis: Vector3, TargetVelocity: float32, Settings: MotorSettings | Linear motor along axis |
| AngularMotor | TargetVelocity: Vector3, Settings: MotorSettings | Angular velocity motor |
| PointOnLine | LocalOrigin: Vector3, LocalDirection: Vector3, LocalOffset: Vector3, SpringSettings: SpringConfig | Point-on-line servo |

### ContactEventType

| Case | Description |
|------|-------------|
| Began | First frame of contact |
| Persisted | Ongoing contact from previous frame |
| Ended | Contact ceased this frame |

## Composite Types

### PhysicsConfig

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Gravity | Vector3 | (0, -9.81, 0) | World gravity vector |
| SolverIterations | int32 | 8 | Velocity solver iteration count |
| SubstepCount | int32 | 1 | Simulation substeps per step call |
| ThreadCount | int32 | 0 (auto) | Worker threads (0 = auto-detect CPU count) |
| Deterministic | bool | false | Enable deterministic simulation |

### DynamicBodyDesc `[<Struct>]`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Shape | ShapeId | required | Registered shape |
| Pose | Pose | required | Initial position and orientation |
| Velocity | Velocity | zero | Initial velocity |
| Mass | float32 | required | Mass in kg (0 = kinematic, negative = error) |
| Material | MaterialProperties | default | Surface material |
| CollisionGroup | uint32 | 0u | Collision layer (0-31) |
| CollisionMask | uint32 | 0xFFFFFFFFu | Collision layer mask (all layers) |
| SleepThreshold | float32 | 0.01f | Activity threshold for sleep |
| ContinuousDetection | bool | false | Enable CCD |

### KinematicBodyDesc `[<Struct>]`

Same fields as `DynamicBodyDesc` minus `Mass` (implicit infinite mass / zero inverse mass).

### StaticBodyDesc `[<Struct>]`

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| Shape | ShapeId | required | Registered shape |
| Pose | Pose | required | Position and orientation |
| Material | MaterialProperties | default | Surface material |
| CollisionGroup | uint32 | 0u | Collision layer |
| CollisionMask | uint32 | 0xFFFFFFFFu | Collision layer mask |

### ContactEvent `[<Struct>]`

| Field | Type | Description |
|-------|------|-------------|
| BodyA | BodyId voption | Body identifier for side A (ValueNone if static-only) |
| StaticA | StaticId voption | Static identifier for side A (ValueNone if body-only) |
| BodyB | BodyId voption | Body identifier for side B (ValueNone if static-only) |
| StaticB | StaticId voption | Static identifier for side B (ValueNone if body-only) |
| Normal | Vector3 | Contact normal (from A to B) |
| Depth | float32 | Penetration depth |
| EventType | ContactEventType | Began, Persisted, or Ended |

**Invariant**: For each side, exactly one of Body/Static is ValueSome.

### RayHit `[<Struct>]`

| Field | Type | Description |
|-------|------|-------------|
| Body | BodyId voption | Body hit (ValueNone if static) |
| Static | StaticId voption | Static hit (ValueNone if body) |
| Position | Vector3 | World-space hit point |
| Normal | Vector3 | Surface normal at hit point |
| Distance | float32 | Ray parameter at hit |

**Invariant**: Exactly one of Body/Static is ValueSome.

### PhysicsWorld

Mutable container type. Not a value type.

| Internal State | Type | Description |
|----------------|------|-------------|
| Simulation | BepuPhysics.Simulation | The physics simulation |
| BufferPool | BepuUtilities.Memory.BufferPool | Memory allocator |
| ThreadDispatcher | ThreadDispatcher | Thread pool for parallel stepping |
| MaterialTable | Dictionary<int, MaterialProperties> | Per-body material lookup |
| CollisionFilterTable | Dictionary<int, CollisionFilter> | Per-body collision group/mask |
| ContactEventBuffer | ContactEventBuffer | Double-buffered event collector |
| Config | PhysicsConfig | Creation configuration (immutable) |

**Lifecycle**: Created via `PhysicsWorld.create` or `PhysicsWorld.createCustom`. Destroyed via `PhysicsWorld.destroy` or `Dispose()`. Implements `IDisposable`.

## State Transitions

### Body Lifecycle

```
[not exists] ──addBody/addKinematicBody──> [active]
[active] ──step (low activity)──> [sleeping]
[sleeping] ──external force/collision──> [active]
[active/sleeping] ──removeBody──> [not exists]
    (auto-removes dependent constraints)
```

### Contact Event Lifecycle

```
[no contact] ──overlap detected──> Began
Began ──still overlapping next step──> Persisted
Persisted ──still overlapping──> Persisted
Persisted/Began ──separated──> Ended
Ended ──next step──> [no contact]
```

### PhysicsWorld Lifecycle

```
[not exists] ──create/createCustom──> [active]
[active] ──step──> [active] (produces ContactEvents)
[active] ──destroy/Dispose──> [disposed]
[disposed] ──any operation──> error
```
