# Quickstart: Stride BepuPhysics Integration & Query Extensions

**Feature**: 006-stride-bepu-integration

## Prerequisites

- .NET 10.0 SDK
- BepuFSharp project builds successfully (`dotnet build`)
- Stride.BepuPhysics NuGet package available (v4.3.0.2507)

## Build & Test

```bash
# Build the library
dotnet build BepuFSharp/BepuFSharp.fsproj

# Run all tests
dotnet test BepuFSharp.Tests/BepuFSharp.Tests.fsproj

# Pack to local NuGet store
dotnet pack BepuFSharp/BepuFSharp.fsproj
```

## Implementation Order

Work through the stories in this order to minimize rework:

1. **Story 3 — Filtered Raycasting** (P2, smallest scope)
   - Modify `SingleHitHandler` and `MultiHitHandler` in Queries.fs
   - Add optional `?filter` parameter to `raycast`/`raycastAll` in PhysicsWorld
   - Verify backward compatibility: existing tests must pass unchanged

2. **Story 1 — Sweep Cast** (P1)
   - Add `SweepHit` type to Types.fs/fsi
   - Create `SweepHitHandler` struct in Queries.fs (implements `ISweepHitHandler`)
   - Add `sweepCast` to PhysicsWorld with shape-type dispatch
   - Reuse filter pattern from Story 3

3. **Story 2 — Overlap** (P1)
   - Add `OverlapResult` type to Types.fs/fsi
   - Create `OverlapEnumerator` struct in Queries.fs (implements `IBreakableForEach`)
   - Add `overlap` to PhysicsWorld with broad-phase + narrow confirm
   - Reuse filter pattern from Story 3

4. **Story 4 — Constraint Readback** (P2)
   - Add constraint type dictionary to PhysicsWorld internal state
   - Populate dictionary in `addConstraint`, clean up in `removeConstraint`/`removeBody`
   - Add `getConstraintDescription`, `constraintExists`, `getConstraintBodies`

5. **Story 5 — Runtime Modification** (P3)
   - Add mutation methods to `DefaultNarrowPhaseCallbacks` filter/material tables
   - Add `setCollisionFilter`, `setStaticCollisionFilter`, `setMaterial`, `setStaticMaterial`

6. **Story 6 — Stride Interop** (P3)
   - Add Stride.BepuPhysics package reference to .fsproj
   - Create StrideInterop.fsi and StrideInterop.fs
   - Implement shape, filter, material, and constraint conversions
   - Verify build still succeeds for non-Stride consumers

## Key Files to Modify

| File | Changes |
|------|---------|
| `BepuFSharp/Types.fsi` + `.fs` | Add `SweepHit`, `OverlapResult` |
| `BepuFSharp/Queries.fs` | Add `SweepHitHandler`, `OverlapEnumerator`, filter support to ray handlers |
| `BepuFSharp/PhysicsWorld.fsi` + `.fs` | Add ~10 new public functions |
| `BepuFSharp/Interop.fs` | Add motor settings conversion helpers |
| `BepuFSharp/Callbacks.fs` | Expose filter/material table mutation |
| `BepuFSharp/StrideInterop.fsi` + `.fs` | New module (type conversions) |
| `BepuFSharp/BepuFSharp.fsproj` | Add Stride.BepuPhysics dependency, new files |

## Verification Checklist

- [ ] All existing tests pass (backward compatibility)
- [ ] Sweep cast returns correct hits for all 8 shape types
- [ ] Overlap returns correct body sets
- [ ] Filtered raycast excludes masked bodies; unfiltered raycast unchanged
- [ ] Constraint readback matches creation parameters for all 10 constraint types
- [ ] Runtime filter/material changes take effect on next step
- [ ] Stride interop round-trips all types without data loss
- [ ] Surface-area baselines updated
- [ ] .fsi signature files updated for all changed public modules
- [ ] Example scripts runnable
- [ ] Package version bumped to 0.2.0
