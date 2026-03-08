# Quickstart: API Safety & Missing Features

**Feature**: 004-api-safety-features | **Date**: 2026-03-08

## Implementation Order

### Step 1: Dependency version pinning (FR-011)
Change `BepuFSharp.fsproj` to pin exact versions:
```xml
<PackageReference Include="BepuPhysics" Version="[2.4.0]" />
<PackageReference Include="BepuUtilities" Version="[2.4.0]" />
```
Verify build still succeeds.

### Step 2: bodyExists / staticExists (FR-001)
Add to `PhysicsWorld.fsi` and `PhysicsWorld.fs`:
```fsharp
let bodyExists (bodyId: BodyId) (world: PhysicsWorld) : bool =
    world.ThrowIfDisposed()
    let handle = Interop.bodyIdToHandle bodyId
    world.Sim.Bodies.BodyExists(handle)

let staticExists (staticId: StaticId) (world: PhysicsWorld) : bool =
    world.ThrowIfDisposed()
    let handle = Interop.staticIdToHandle staticId
    world.Sim.Statics.StaticExists(handle)
```
Add tests in `BodyTests.fs`. Update `PhysicsWorld.baseline`.

### Step 3: tryGet* / trySet* (FR-002, FR-003)
Add safe variants that gate on `bodyExists`:
```fsharp
let tryGetBodyPose (bodyId: BodyId) (world: PhysicsWorld) : Pose voption =
    world.ThrowIfDisposed()
    let handle = Interop.bodyIdToHandle bodyId
    if world.Sim.Bodies.BodyExists(handle) then
        ValueSome (Interop.rigidToPose world.Sim.Bodies.[handle].Pose)
    else
        ValueNone
```
Same pattern for `tryGetBodyVelocity`, `trySetBodyPose`, `trySetBodyVelocity`.

### Step 4: Impulse functions (FR-005, FR-006)
Add impulse application guarded by `bodyExists`:
```fsharp
let applyImpulse (bodyId: BodyId) (impulse: Vector3) (offset: Vector3) (world: PhysicsWorld) : unit =
    world.ThrowIfDisposed()
    let handle = Interop.bodyIdToHandle bodyId
    if world.Sim.Bodies.BodyExists(handle) then
        world.Sim.Bodies.[handle].ApplyImpulse(impulse, offset)
```
Add `ImpulseTests.fs` to test project.

### Step 5: Enumeration (FR-007, FR-008)
Iterate body sets and collect handles:
```fsharp
let getAllBodyIds (world: PhysicsWorld) : BodyId[] =
    world.ThrowIfDisposed()
    // Iterate active + sleeping sets, collect IndexToHandle values
```

### Step 6: Gravity (FR-009)
Store `PoseIntegrator` reference in PhysicsWorld at construction. Add getter/setter.

### Step 7: Shape query (FR-010) and describe (FR-012)
Add `getBodyShape` to PhysicsWorld and `describe` to PhysicsShape module.

### Step 8: Update baselines, docs, and example script
- Update `PhysicsWorld.baseline` and `Shapes.baseline`
- Add XML doc comments to `.fsi` files
- Create `scripts/examples/09-impulse-and-gravity.fsx`
- Build docs to verify

## Verification

```bash
dotnet build
dotnet test
dotnet pack
dotnet fsi scripts/examples/09-impulse-and-gravity.fsx
```
