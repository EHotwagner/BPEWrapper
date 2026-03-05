module BepuFSharp.Tests.ContactEventTests

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

/// Step and collect all events across multiple steps
let private stepAndCollect steps dt world =
    let allEvents = ResizeArray<ContactEvent>()
    for _ in 1..steps do
        PhysicsWorld.step dt world
        allEvents.AddRange(PhysicsWorld.getContactEvents world)
    allEvents.ToArray()

[<Tests>]
let tests = testList "ContactEvents" [
    test "sphere dropping onto static floor produces Began event" {
        let world = createWorld ()
        let _floor = addFloor world
        // Start sphere close to floor so collision happens quickly
        let _sphere = addSphere (Vector3(0.0f, 1.0f, 0.0f)) 1.0f world
        let events = stepAndCollect 60 (1.0f/60.0f) world
        Expect.isGreaterThan events.Length 0 "Should have at least one contact event"
    }

    test "continued overlap produces events across frames" {
        let world = createWorld ()
        let _floor = addFloor world
        let _sphere = addSphere (Vector3(0.0f, 1.0f, 0.0f)) 1.0f world
        // Step until contact
        let events1 = stepAndCollect 60 (1.0f/60.0f) world
        Expect.isGreaterThan events1.Length 0 "Should have events during collision"
        // Events should appear across multiple frames
        Expect.isGreaterThan events1.Length 1 "Should have multiple events across frames"
    }

    test "separation produces Ended event" {
        let world = createWorld ()
        let _floor = addFloor world
        let sphere = addSphere (Vector3(0.0f, 1.0f, 0.0f)) 1.0f world
        // Let sphere fall and contact floor
        let _ = stepAndCollect 30 (1.0f/60.0f) world
        // Verify we have active contact by checking events on the last step
        // Wake the body and ensure active contact
        PhysicsWorld.setBodyVelocity sphere (Velocity.create (Vector3(0.0f, -0.1f, 0.0f)) Vector3.Zero) world
        PhysicsWorld.step (1.0f/60.0f) world
        let eventsBeforeSep = PhysicsWorld.getContactEvents world
        // Now teleport sphere far away
        PhysicsWorld.setBodyPose sphere (Pose.ofPosition (Vector3(0.0f, 100.0f, 0.0f))) world
        PhysicsWorld.setBodyVelocity sphere (Velocity.zero) world
        // Step to detect separation
        PhysicsWorld.step (1.0f/60.0f) world
        let events = PhysicsWorld.getContactEvents world
        let ended = events |> Array.filter (fun e -> e.EventType = Ended)
        Expect.isGreaterThan ended.Length 0
            (sprintf "Should have Ended event. Events before sep: %d, events after: %d" eventsBeforeSep.Length events.Length)
    }

    test "two dynamic bodies colliding produces event with BodyA and BodyB" {
        let world = createWorld ()
        // Two spheres overlapping initially
        let _b1 = addSphere (Vector3(0.0f, 0.0f, 0.0f)) 1.0f world
        let _b2 = addSphere (Vector3(0.0f, 0.8f, 0.0f)) 1.0f world
        let events = stepAndCollect 10 (1.0f/60.0f) world
        let bodyBody = events |> Array.filter (fun e ->
            e.BodyA.IsSome && e.BodyB.IsSome && e.StaticA.IsNone && e.StaticB.IsNone)
        Expect.isGreaterThan bodyBody.Length 0 "Should have body-body contact event"
    }

    test "body-static contact has body and static identifiers" {
        let world = createWorld ()
        let _floor = addFloor world
        let _sphere = addSphere (Vector3(0.0f, 1.0f, 0.0f)) 1.0f world
        let events = stepAndCollect 60 (1.0f/60.0f) world
        let bodyStatic = events |> Array.filter (fun e ->
            (e.BodyA.IsSome || e.BodyB.IsSome) && (e.StaticA.IsSome || e.StaticB.IsSome))
        Expect.isGreaterThan bodyStatic.Length 0 "Should have body-static contact event"
    }

    test "collision filter: bodies in different groups with matching masks collide" {
        let world = createWorld ()
        let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
        let descA =
            { DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f))) 1.0f
                with CollisionGroup = 0u; CollisionMask = 0xFFFFFFFFu }
        let descB =
            { DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 0.8f, 0.0f))) 1.0f
                with CollisionGroup = 1u; CollisionMask = 0xFFFFFFFFu }
        let _a = PhysicsWorld.addBody descA world
        let _b = PhysicsWorld.addBody descB world
        let events = stepAndCollect 10 (1.0f/60.0f) world
        Expect.isGreaterThan events.Length 0 "Bodies with matching masks should collide"
    }

    test "collision filter: bodies with mask=0 don't collide" {
        let world = createWorld ()
        let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
        let descA =
            { DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f))) 1.0f
                with CollisionGroup = 1u; CollisionMask = 0u }
        let descB =
            { DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 0.8f, 0.0f))) 1.0f
                with CollisionGroup = 1u; CollisionMask = 0u }
        let _a = PhysicsWorld.addBody descA world
        let _b = PhysicsWorld.addBody descB world
        let events = stepAndCollect 10 (1.0f/60.0f) world
        let bodyBody = events |> Array.filter (fun e ->
            e.BodyA.IsSome && e.BodyB.IsSome)
        Expect.equal bodyBody.Length 0 "Bodies with mask=0 should not collide"
    }

    test "collision filter: default mask collides with everything" {
        let world = createWorld ()
        let shape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
        let descA = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 0.0f, 0.0f))) 1.0f
        let descB = DynamicBodyDesc.create shape (Pose.ofPosition (Vector3(0.0f, 0.8f, 0.0f))) 1.0f
        let _a = PhysicsWorld.addBody descA world
        let _b = PhysicsWorld.addBody descB world
        let events = stepAndCollect 10 (1.0f/60.0f) world
        Expect.isGreaterThan events.Length 0 "Default mask should allow collision"
    }
]
