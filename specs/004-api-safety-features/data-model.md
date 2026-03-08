# Data Model: API Safety & Missing Features

**Feature**: 004-api-safety-features | **Date**: 2026-03-08

## Entities

No new entities are introduced. This feature extends existing entities with new operations.

### Modified Entities

#### PhysicsWorld (extended)

Gains internal state to support gravity access:

- **PoseIntegrator reference**: Captured at construction time to enable `getGravity`/`setGravity` without requiring the generic simulation type at call sites.

New operations:
- `bodyExists : BodyId -> PhysicsWorld -> bool`
- `staticExists : StaticId -> PhysicsWorld -> bool`
- `tryGetBodyPose : BodyId -> PhysicsWorld -> Pose voption`
- `tryGetBodyVelocity : BodyId -> PhysicsWorld -> Velocity voption`
- `trySetBodyPose : BodyId -> Pose -> PhysicsWorld -> bool`
- `trySetBodyVelocity : BodyId -> Velocity -> PhysicsWorld -> bool`
- `applyImpulse : BodyId -> impulse:Vector3 -> offset:Vector3 -> PhysicsWorld -> unit`
- `applyLinearImpulse : BodyId -> impulse:Vector3 -> PhysicsWorld -> unit`
- `applyAngularImpulse : BodyId -> impulse:Vector3 -> PhysicsWorld -> unit`
- `getAllBodyIds : PhysicsWorld -> BodyId[]`
- `getAllStaticIds : PhysicsWorld -> StaticId[]`
- `setGravity : Vector3 -> PhysicsWorld -> unit`
- `getGravity : PhysicsWorld -> Vector3`
- `getBodyShape : BodyId -> PhysicsWorld -> PhysicsShape option`

#### PhysicsShape (extended)

New companion module operation:
- `PhysicsShape.describe : PhysicsShape -> string`

### Unchanged Entities

- **BodyId**, **StaticId**, **ShapeId**, **ConstraintId**: No structural changes.
- **Pose**, **Velocity**, **SpringConfig**, **MaterialProperties**, **CollisionFilter**: No changes.
- **ContactEvent**, **RayHit**, **PhysicsConfig**: No changes.
- **DynamicBodyDesc**, **KinematicBodyDesc**, **StaticBodyDesc**: No changes.
- **ConstraintDesc**: No changes.
- **PhysicsError**, **PhysicsDiagnosticEvent**: No changes.

## State Transitions

### Body Lifecycle (clarified)

```
Created (addBody/addKinematicBody) → Active
Active → Removed (removeBody)

bodyExists: Active → true, Removed → false
tryGet*/trySet*: Active → ValueSome/true, Removed → ValueNone/false
applyImpulse: Active → velocity modified, Removed → no-op (silent)
getBodyShape: Active → Some shape, Removed → None
```

### Gravity Lifecycle

```
World Created → gravity = config.Gravity
setGravity called → gravity = new value (immediate, affects next step)
getGravity called → returns current gravity value
```

## Validation Rules

- `applyImpulse`/`applyLinearImpulse`/`applyAngularImpulse`: Guard with `bodyExists` before accessing body reference. Silent no-op if body doesn't exist.
- `setGravity`: No validation needed — any `Vector3` is valid gravity (including zero, negative, NaN). NaN gravity is a caller error but not something the physics library should prevent.
- `getBodyShape`: For complex shapes (ConvexHull, Compound, Mesh), return a simplified reconstruction. Document that point/triangle data may not round-trip perfectly for these types.
