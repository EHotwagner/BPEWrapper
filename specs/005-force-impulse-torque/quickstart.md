# Quickstart: Force, Impulse, and Torque Application

**Branch**: `005-force-impulse-torque` | **Date**: 2026-03-16

## Overview

This feature adds force, impulse, and torque application functions to the `PhysicsWorld` module. All new functions follow the existing pipeline style (`world` as last parameter) and integrate with BepuPhysics2's impulse API under the hood.

## Key Files to Modify

| File | Change |
|------|--------|
| `BepuFSharp/PhysicsWorld.fs` | Add implementation of 8 new functions |
| `BepuFSharp/PhysicsWorld.fsi` | Add public API signatures for 8 new functions |
| `BepuFSharp.Tests/baselines/PhysicsWorld.baseline` | Update surface area baseline |
| `BepuFSharp.Tests/ForceTests.fs` | New test module (add to .fsproj) |

## Implementation Pattern

All functions follow the same internal pattern:

```
1. ThrowIfDisposed()
2. Convert BodyId to BodyHandle via Interop
3. Get BodyReference from sim.Bodies.[handle]
4. Check bodyRef.Kinematic — if true, return (no-op)
5. Wake the body via sim.Awakener.AwakenBody(handle)
6. Call appropriate BodyReference method (ApplyLinearImpulse, etc.)
```

For force functions, step 6 becomes: `bodyRef.ApplyLinearImpulse(force * dt)`

For bulk functions, wrap steps 2-6 in a loop over the parallel arrays.

## Dependencies

- No new NuGet packages required
- No new F# source files beyond the test file
- Uses only existing BepuPhysics2 API (`BodyReference` methods + `Awakener`)

## Testing Strategy

- **Unit tests**: Apply known impulse/force to body with known mass, verify velocity change
- **Angular tests**: Apply angular impulse to symmetric body (sphere), verify angular velocity
- **Point impulse tests**: Apply impulse at offset, verify both linear + angular velocity change
- **Kinematic no-op tests**: Verify kinematic bodies are unaffected
- **Wake tests**: Verify sleeping bodies wake on force/impulse application
- **Bulk tests**: Verify bulk operations produce same results as per-body operations
- **Surface area test**: Update PhysicsWorld.baseline to include new signatures
