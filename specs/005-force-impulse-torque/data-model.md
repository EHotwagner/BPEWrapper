# Data Model: Force, Impulse, and Torque Application

**Branch**: `005-force-impulse-torque` | **Date**: 2026-03-16

## Entities

This feature introduces no new persistent types or data structures. All force/impulse/torque operations use existing types (`BodyId`, `Vector3`, `float32`) and are stateless — they modify body velocity immediately and do not store any accumulated state.

### Existing Types Used

| Type | Role in this feature |
|------|---------------------|
| `BodyId` | Identifies the target body for force/impulse application |
| `Vector3` (System.Numerics) | Represents force, impulse, torque, and angular impulse vectors; also used for world-space point offsets |
| `float32` | Timestep duration (`dt`) for force-to-impulse conversion |
| `PhysicsWorld` | The simulation context; all functions take it as last parameter (pipeline style) |

### BepuPhysics2 Internal Types Accessed

| Type | Access pattern |
|------|---------------|
| `BodyReference` | Obtained via `sim.Bodies.[handle]`; provides `ApplyLinearImpulse`, `ApplyAngularImpulse`, `ApplyImpulse`, `Kinematic` |
| `IslandAwakener` | Obtained via `sim.Awakener`; provides `AwakenBody(handle)` |

## State Transitions

Force/impulse application does not introduce new states. The only relevant state transition is:

```
Sleeping body --[any force/impulse/torque applied]--> Awake body
```

This is handled by calling `sim.Awakener.AwakenBody(handle)` before applying the impulse.

## Validation Rules

- `dt` must be positive for force/torque functions (force * 0 or negative dt is meaningless)
- Kinematic bodies (`bodyRef.Kinematic = true`) are silently skipped
- World must not be disposed (`ThrowIfDisposed()` guard, consistent with all existing functions)
- Bulk arrays must have matching lengths (ids and vectors arrays)
