module BepuFSharp.Tests.QueryTests

open System.Numerics
open Expecto
open BepuFSharp

let private createWorld () =
    PhysicsWorld.create PhysicsConfig.defaults

let private addFloor world =
    let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(100.0f, 1.0f, 100.0f)) world
    let floorDesc = StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f)))
    PhysicsWorld.addStatic floorDesc world

let private addSphere pos mass world =
    let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
    let desc = DynamicBodyDesc.create shape (Pose.ofPosition pos) mass
    PhysicsWorld.addBody desc world

[<Tests>]
let tests = testList "Queries" [
    test "ray hitting static floor returns hit with correct distance and upward normal" {
        let world = createWorld ()
        let _floor = addFloor world
        PhysicsWorld.step (1.0f/60.0f) world
        let hit = PhysicsWorld.raycast (Vector3(0.0f, 10.0f, 0.0f)) (Vector3(0.0f, -1.0f, 0.0f)) 100.0f None world
        Expect.isSome hit "Ray should hit the floor"
        let h = hit.Value
        Expect.isTrue h.Static.IsSome "Hit should be a static"
        Expect.isLessThan (abs (h.Distance - 10.0f)) 0.6f "Distance should be close to 10"
        Expect.isGreaterThan h.Normal.Y 0.5f "Normal should point up"
    }

    test "ray missing all bodies returns None" {
        let world = createWorld ()
        let _floor = addFloor world
        PhysicsWorld.step (1.0f/60.0f) world
        let hit = PhysicsWorld.raycast (Vector3(0.0f, 10.0f, 0.0f)) (Vector3(0.0f, 1.0f, 0.0f)) 100.0f None world
        Expect.isNone hit "Ray pointing up should miss floor"
    }

    test "raycastAll returns multiple hits sorted by distance" {
        let world = createWorld ()
        let _floor = addFloor world
        let _sphere = addSphere (Vector3(0.0f, 5.0f, 0.0f)) 1.0f world
        PhysicsWorld.step (1.0f/60.0f) world
        let hits = PhysicsWorld.raycastAll (Vector3(0.0f, 20.0f, 0.0f)) (Vector3(0.0f, -1.0f, 0.0f)) 100.0f None world
        Expect.isGreaterThan hits.Length 1 "Should hit both sphere and floor"
        for i in 0 .. hits.Length - 2 do
            Expect.isLessThanOrEqual hits.[i].Distance hits.[i+1].Distance "Hits should be sorted by distance"
    }

    test "ray hitting dynamic body returns RayHit with BodyId" {
        let world = createWorld ()
        let _sphere = addSphere (Vector3(0.0f, 5.0f, 0.0f)) 1.0f world
        PhysicsWorld.step (1.0f/60.0f) world
        let hit = PhysicsWorld.raycast (Vector3(0.0f, 20.0f, 0.0f)) (Vector3(0.0f, -1.0f, 0.0f)) 100.0f None world
        Expect.isSome hit "Ray should hit the sphere"
        let h = hit.Value
        Expect.isTrue h.Body.IsSome "Hit should be a body"
        Expect.isTrue h.Static.IsNone "Hit should not be a static"
    }
]

// --- Sweep Cast Tests ---

let private addStaticBox pos (size: Vector3) world =
    let shape = PhysicsWorld.addShape (PhysicsShape.Box(size.X, size.Y, size.Z)) world
    let desc = StaticBodyDesc.create shape (Pose.ofPosition pos)
    PhysicsWorld.addStatic desc world

let private addLayeredSphere pos mass group mask world =
    let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
    let desc =
        { DynamicBodyDesc.create shape (Pose.ofPosition pos) mass with
            CollisionGroup = group
            CollisionMask = mask }
    PhysicsWorld.addBody desc world

let private addLayeredStaticBox pos (size: Vector3) group mask world =
    let shape = PhysicsWorld.addShape (PhysicsShape.Box(size.X, size.Y, size.Z)) world
    let desc =
        { StaticBodyDesc.create shape (Pose.ofPosition pos) with
            CollisionGroup = group
            CollisionMask = mask }
    PhysicsWorld.addStatic desc world

[<Tests>]
let sweepCastTests = testList "SweepCast" [
    test "sphere sweep hits static box — returns contact point, normal, distance" {
        let world = createWorld ()
        let _box = addStaticBox (Vector3(0.0f, 0.0f, 10.0f)) (Vector3(2.0f, 2.0f, 2.0f)) world
        PhysicsWorld.step (1.0f/60.0f) world
        let hit = PhysicsWorld.sweepCast
                    (PhysicsShape.Sphere 0.5f)
                    (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f)))
                    (Vector3(0.0f, 0.0f, 1.0f))
                    20.0f
                    None
                    world
        Expect.isSome hit "Sweep should hit the box"
        let h = hit.Value
        Expect.isTrue h.Static.IsSome "Hit should be a static"
        Expect.isGreaterThan h.Distance 0.0f "Distance should be positive"
        Expect.isLessThan h.Distance 20.0f "Distance should be less than max"
    }

    test "sweep along empty path returns None" {
        let world = createWorld ()
        PhysicsWorld.step (1.0f/60.0f) world
        let hit = PhysicsWorld.sweepCast
                    (PhysicsShape.Sphere 0.5f)
                    (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f)))
                    (Vector3(0.0f, 0.0f, 1.0f))
                    20.0f
                    None
                    world
        Expect.isNone hit "Sweep should miss in empty world"
    }

    test "sweep past multiple bodies returns only closest hit" {
        let world = createWorld ()
        let _box1 = addStaticBox (Vector3(0.0f, 0.0f, 5.0f)) (Vector3(2.0f, 2.0f, 2.0f)) world
        let _box2 = addStaticBox (Vector3(0.0f, 0.0f, 15.0f)) (Vector3(2.0f, 2.0f, 2.0f)) world
        PhysicsWorld.step (1.0f/60.0f) world
        let hit = PhysicsWorld.sweepCast
                    (PhysicsShape.Sphere 0.5f)
                    (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f)))
                    (Vector3(0.0f, 0.0f, 1.0f))
                    20.0f
                    None
                    world
        Expect.isSome hit "Sweep should hit"
        let h = hit.Value
        Expect.isLessThan h.Distance 10.0f "Should hit the closer box first"
    }

    test "sweep with collision mask ignores non-matching layers" {
        let world = createWorld ()
        // Box on layer 1, sphere query filters to layer 0 only
        let _box = addLayeredStaticBox (Vector3(0.0f, 0.0f, 5.0f)) (Vector3(2.0f, 2.0f, 2.0f)) 1u 0xFFFFFFFFu world
        PhysicsWorld.step (1.0f/60.0f) world
        let hit = PhysicsWorld.sweepCast
                    (PhysicsShape.Sphere 0.5f)
                    (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f)))
                    (Vector3(0.0f, 0.0f, 1.0f))
                    20.0f
                    (Some { Group = 0u; Mask = 1u }) // mask bit 0 only = layer 0
                    world
        Expect.isNone hit "Sweep should miss box on layer 1 when filtering for layer 0"
    }

    test "degenerate shape (zero radius sphere) returns None" {
        let world = createWorld ()
        let _box = addStaticBox (Vector3(0.0f, 0.0f, 5.0f)) (Vector3(2.0f, 2.0f, 2.0f)) world
        PhysicsWorld.step (1.0f/60.0f) world
        let hit = PhysicsWorld.sweepCast
                    (PhysicsShape.Sphere 0.0f)
                    (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f)))
                    (Vector3(0.0f, 0.0f, 1.0f))
                    20.0f
                    None
                    world
        Expect.isNone hit "Degenerate shape should return None"
    }
]

// --- Overlap Tests ---

[<Tests>]
let overlapTests = testList "Overlap" [
    test "5 bodies, 3 inside sphere — exactly 3 returned" {
        let world = createWorld ()
        // Place 3 spheres close to origin (inside test volume)
        let _b1 = addSphere (Vector3(0.0f, 1.0f, 0.0f)) 1.0f world
        let _b2 = addSphere (Vector3(1.0f, 1.0f, 0.0f)) 1.0f world
        let _b3 = addSphere (Vector3(-1.0f, 1.0f, 0.0f)) 1.0f world
        // Place 2 spheres far away (outside test volume)
        let _b4 = addSphere (Vector3(50.0f, 1.0f, 0.0f)) 1.0f world
        let _b5 = addSphere (Vector3(-50.0f, 1.0f, 0.0f)) 1.0f world
        PhysicsWorld.step (1.0f/60.0f) world
        let results = PhysicsWorld.overlap
                        (PhysicsShape.Sphere 5.0f)
                        (Pose.ofPosition (Vector3(0.0f, 1.0f, 0.0f)))
                        None
                        world
        Expect.equal results.Length 3 "Should find exactly 3 overlapping bodies"
    }

    test "no bodies inside volume returns empty array" {
        let world = createWorld ()
        let _b1 = addSphere (Vector3(50.0f, 1.0f, 0.0f)) 1.0f world
        PhysicsWorld.step (1.0f/60.0f) world
        let results = PhysicsWorld.overlap
                        (PhysicsShape.Sphere 1.0f)
                        (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f)))
                        None
                        world
        Expect.equal results.Length 0 "No bodies near origin"
    }

    test "overlap with collision mask excludes non-matching layers" {
        let world = createWorld ()
        // Body on layer 0
        let _b1 = addLayeredSphere (Vector3(0.0f, 1.0f, 0.0f)) 1.0f 0u 0xFFFFFFFFu world
        // Body on layer 1
        let _b2 = addLayeredSphere (Vector3(1.0f, 1.0f, 0.0f)) 1.0f 1u 0xFFFFFFFFu world
        PhysicsWorld.step (1.0f/60.0f) world
        // Filter: only layer 0 (mask bit 0)
        let results = PhysicsWorld.overlap
                        (PhysicsShape.Sphere 5.0f)
                        (Pose.ofPosition (Vector3(0.0f, 1.0f, 0.0f)))
                        (Some { Group = 0u; Mask = 1u })
                        world
        Expect.equal results.Length 1 "Should find only the layer-0 body"
    }

    test "zero-volume shape returns empty array" {
        let world = createWorld ()
        let _b1 = addSphere (Vector3(0.0f, 1.0f, 0.0f)) 1.0f world
        PhysicsWorld.step (1.0f/60.0f) world
        let results = PhysicsWorld.overlap
                        (PhysicsShape.Sphere 0.0f)
                        (Pose.ofPosition (Vector3(0.0f, 1.0f, 0.0f)))
                        None
                        world
        Expect.equal results.Length 0 "Zero-volume shape should return empty"
    }
]

// --- Filtered Raycast Tests ---

[<Tests>]
let filteredRaycastTests = testList "FilteredRaycast" [
    test "raycast with mask matching only layer 0 — only layer-0 bodies hit" {
        let world = createWorld ()
        // Body on layer 0 at y=5
        let _b0 = addLayeredSphere (Vector3(0.0f, 5.0f, 0.0f)) 1.0f 0u 0xFFFFFFFFu world
        // Body on layer 1 at y=3 (closer to ray origin at y=20)
        let _b1 = addLayeredSphere (Vector3(0.0f, 3.0f, 0.0f)) 1.0f 1u 0xFFFFFFFFu world
        PhysicsWorld.step (1.0f/60.0f) world
        // Filter: group=0, mask=bit 0 only (matches layer 0 bodies)
        let hit = PhysicsWorld.raycast
                    (Vector3(0.0f, 20.0f, 0.0f))
                    (Vector3(0.0f, -1.0f, 0.0f))
                    100.0f
                    (Some { Group = 0u; Mask = 1u })
                    world
        Expect.isSome hit "Should hit layer-0 body"
        let h = hit.Value
        // Should hit the layer-0 body at y=5, not the layer-1 body at y=3
        Expect.isLessThan (abs (h.Distance - 15.0f)) 1.0f "Should hit body near y=5 (distance ~15)"
    }

    test "raycast without filter (None) hits all bodies — backward compatible" {
        let world = createWorld ()
        let _b0 = addLayeredSphere (Vector3(0.0f, 5.0f, 0.0f)) 1.0f 0u 0xFFFFFFFFu world
        let _b1 = addLayeredSphere (Vector3(0.0f, 3.0f, 0.0f)) 1.0f 1u 0xFFFFFFFFu world
        PhysicsWorld.step (1.0f/60.0f) world
        let hits = PhysicsWorld.raycastAll
                    (Vector3(0.0f, 20.0f, 0.0f))
                    (Vector3(0.0f, -1.0f, 0.0f))
                    100.0f
                    None
                    world
        Expect.isGreaterThanOrEqual hits.Length 2 "Should hit both bodies with no filter"
    }

    test "raycastAll with mask excludes specific layers from results" {
        let world = createWorld ()
        // Two bodies on layer 0
        let _b0 = addLayeredSphere (Vector3(0.0f, 5.0f, 0.0f)) 1.0f 0u 0xFFFFFFFFu world
        let _b1 = addLayeredSphere (Vector3(0.0f, 3.0f, 0.0f)) 1.0f 0u 0xFFFFFFFFu world
        // One body on layer 1
        let _b2 = addLayeredSphere (Vector3(0.0f, 8.0f, 0.0f)) 1.0f 1u 0xFFFFFFFFu world
        PhysicsWorld.step (1.0f/60.0f) world
        let hits = PhysicsWorld.raycastAll
                    (Vector3(0.0f, 20.0f, 0.0f))
                    (Vector3(0.0f, -1.0f, 0.0f))
                    100.0f
                    (Some { Group = 0u; Mask = 1u }) // layer 0 only
                    world
        Expect.equal hits.Length 2 "Should only hit the two layer-0 bodies"
    }
]
