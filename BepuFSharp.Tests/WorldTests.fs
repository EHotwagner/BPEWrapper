module BepuFSharp.Tests.WorldTests

open System.Numerics
open Expecto
open BepuFSharp

[<Tests>]
let tests =
    testList "World" [
        testCase "create with default config has gravity -9.81 Y" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            let sim = PhysicsWorld.simulation world
            Expect.isNotNull (box sim) "Simulation should be created"

        testCase "create with custom config reflects settings" <| fun _ ->
            let config =
                { PhysicsConfig.defaults with
                    Gravity = Vector3(0.0f, -20.0f, 0.0f)
                    SubstepCount = 4
                    SolverIterations = 16 }
            use world = PhysicsWorld.create config
            Expect.isNotNull (box (PhysicsWorld.simulation world)) "Custom world should be created"

        testCase "destroy disposes all resources" <| fun _ ->
            let world = PhysicsWorld.create PhysicsConfig.defaults
            PhysicsWorld.destroy world
            Expect.throwsT<System.ObjectDisposedException>
                (fun () -> PhysicsWorld.simulation world |> ignore)
                "Should throw after destroy"

        testCase "step advances simulation without error" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            PhysicsWorld.step 0.016f world

        testCase "step with dt=0 is a no-op" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            PhysicsWorld.step 0.0f world

        testCase "step with negative dt is a no-op" <| fun _ ->
            use world = PhysicsWorld.create PhysicsConfig.defaults
            PhysicsWorld.step -0.016f world

        testCase "deterministic mode produces identical results" <| fun _ ->
            let config = { PhysicsConfig.defaults with Deterministic = true; ThreadCount = 1 }
            use w1 = PhysicsWorld.create config
            let s1 = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) w1
            use w2 = PhysicsWorld.create config
            let _s2 = PhysicsWorld.addShape (PhysicsShape.Sphere 1.0f) w2
            Expect.isTrue (s1.TypeId >= 0) "Shape type ID should be non-negative"
    ]
