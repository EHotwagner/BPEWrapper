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
        let hit = PhysicsWorld.raycast (Vector3(0.0f, 10.0f, 0.0f)) (Vector3(0.0f, -1.0f, 0.0f)) 100.0f world
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
        let hit = PhysicsWorld.raycast (Vector3(0.0f, 10.0f, 0.0f)) (Vector3(0.0f, 1.0f, 0.0f)) 100.0f world
        Expect.isNone hit "Ray pointing up should miss floor"
    }

    test "raycastAll returns multiple hits sorted by distance" {
        let world = createWorld ()
        let _floor = addFloor world
        let _sphere = addSphere (Vector3(0.0f, 5.0f, 0.0f)) 1.0f world
        PhysicsWorld.step (1.0f/60.0f) world
        let hits = PhysicsWorld.raycastAll (Vector3(0.0f, 20.0f, 0.0f)) (Vector3(0.0f, -1.0f, 0.0f)) 100.0f world
        Expect.isGreaterThan hits.Length 1 "Should hit both sphere and floor"
        for i in 0 .. hits.Length - 2 do
            Expect.isLessThanOrEqual hits.[i].Distance hits.[i+1].Distance "Hits should be sorted by distance"
    }

    test "ray hitting dynamic body returns RayHit with BodyId" {
        let world = createWorld ()
        let _sphere = addSphere (Vector3(0.0f, 5.0f, 0.0f)) 1.0f world
        PhysicsWorld.step (1.0f/60.0f) world
        let hit = PhysicsWorld.raycast (Vector3(0.0f, 20.0f, 0.0f)) (Vector3(0.0f, -1.0f, 0.0f)) 100.0f world
        Expect.isSome hit "Ray should hit the sphere"
        let h = hit.Value
        Expect.isTrue h.Body.IsSome "Hit should be a body"
        Expect.isTrue h.Static.IsNone "Hit should not be a static"
    }
]
