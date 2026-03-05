module BepuFSharp.Tests.PropertyTests

open System.Numerics
open Expecto
open FsCheck
open BepuFSharp

let private createWorld () =
    PhysicsWorld.create PhysicsConfig.defaults

let private config = { FsCheckConfig.defaultConfig with maxTest = 50 }

[<Tests>]
let tests = testList "Properties" [
    testPropertyWithConfig config "pose round-trip preserves values" <| fun (x: float32) (y: float32) (z: float32) ->
        if System.Single.IsFinite x && System.Single.IsFinite y && System.Single.IsFinite z then
            let world = createWorld ()
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let pos = Vector3(x, y, z)
            let pose = Pose.ofPosition pos
            let desc = KinematicBodyDesc.create shape pose
            let bodyId = PhysicsWorld.addKinematicBody desc world
            let readPose = PhysicsWorld.getBodyPose bodyId world
            let dx = abs (readPose.Position.X - x)
            let dy = abs (readPose.Position.Y - y)
            let dz = abs (readPose.Position.Z - z)
            PhysicsWorld.destroy world
            dx < 0.001f && dy < 0.001f && dz < 0.001f
        else true

    testPropertyWithConfig config "velocity round-trip preserves values" <| fun (lx: float32) (ly: float32) (lz: float32) ->
        if System.Single.IsFinite lx && System.Single.IsFinite ly && System.Single.IsFinite lz then
            let world = createWorld ()
            let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) world
            let desc = KinematicBodyDesc.create shape Pose.identity
            let bodyId = PhysicsWorld.addKinematicBody desc world
            let vel = Velocity.create (Vector3(lx, ly, lz)) Vector3.Zero
            PhysicsWorld.setBodyVelocity bodyId vel world
            let readVel = PhysicsWorld.getBodyVelocity bodyId world
            let dx = abs (readVel.Linear.X - lx)
            let dy = abs (readVel.Linear.Y - ly)
            let dz = abs (readVel.Linear.Z - lz)
            PhysicsWorld.destroy world
            dx < 0.001f && dy < 0.001f && dz < 0.001f
        else true

    testPropertyWithConfig config "bulk readPoses/writePoses is idempotent" <| fun (n: int) ->
        let count = (abs n % 20) + 1
        let world = createWorld ()
        let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
        let bodies =
            [| for i in 0..count-1 ->
                PhysicsWorld.addKinematicBody (KinematicBodyDesc.create shape (Pose.ofPosition (Vector3(float32 i, 0.0f, 0.0f)))) world |]
        let poses1 = Array.zeroCreate<Pose> count
        let poses2 = Array.zeroCreate<Pose> count
        PhysicsWorld.readPoses bodies poses1 world
        PhysicsWorld.writePoses bodies poses1 world
        PhysicsWorld.readPoses bodies poses2 world
        PhysicsWorld.destroy world
        Array.forall2 (fun (a: Pose) (b: Pose) ->
            Vector3.Distance(a.Position, b.Position) < 0.001f) poses1 poses2

    testPropertyWithConfig config "SpringConfig round-trip through create" <| fun (freq: float32) (damp: float32) ->
        if System.Single.IsFinite freq && System.Single.IsFinite damp && freq > 0.0f && damp > 0.0f then
            let sc = SpringConfig.create freq damp
            abs (sc.Frequency - freq) < 0.001f && abs (sc.DampingRatio - damp) < 0.001f
        else true
]
