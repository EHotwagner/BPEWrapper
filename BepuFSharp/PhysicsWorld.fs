namespace BepuFSharp

open System
open System.Numerics
open BepuPhysics
open BepuPhysics.Collidables
open BepuUtilities
open BepuUtilities.Memory

[<Sealed>]
type PhysicsWorld internal (sim: Simulation, pool: BufferPool, threadDispatcher: ThreadDispatcher option, config: PhysicsConfig, eventBuffer: ContactEvents.ContactEventBuffer, materialTable: Collections.Generic.Dictionary<int, MaterialProperties>, filterTable: Collections.Generic.Dictionary<int, CollisionFilter>, getGravityFn: unit -> Vector3, setGravityFn: Vector3 -> unit) =
    let mutable disposed = false

    member internal _.Sim = sim
    member internal _.Pool = pool
    member internal _.ThreadDispatcher = threadDispatcher
    member internal _.MaterialTable = materialTable
    member internal _.FilterTable = filterTable
    member internal _.Config = config
    member internal _.EventBuffer = eventBuffer
    member internal _.IsDisposed = disposed
    member internal _.GetGravity() = getGravityFn()
    member internal _.SetGravity(g) = setGravityFn g

    member internal _.ThrowIfDisposed() =
        if disposed then
            raise (ObjectDisposedException("PhysicsWorld", PhysicsError.describe PhysicsError.WorldDisposed))

    interface IDisposable with
        member this.Dispose() =
            if not disposed then
                disposed <- true
                sim.Dispose()
                match threadDispatcher with
                | Some td -> td.Dispose()
                | None -> ()
                pool.Clear()

[<RequireQualifiedAccess>]
module PhysicsWorld =
    let create (config: PhysicsConfig) : PhysicsWorld =
        let pool = new BufferPool()
        let threadCount =
            if config.ThreadCount <= 0 then Environment.ProcessorCount
            else config.ThreadCount
        let threadDispatcher =
            if threadCount > 1 then Some(new ThreadDispatcher(threadCount))
            else None
        let materialTable = Collections.Generic.Dictionary<int, MaterialProperties>()
        let filterTable = Collections.Generic.Dictionary<int, CollisionFilter>()
        let eventBuffer = ContactEvents.ContactEventBuffer()
        let narrowCallbacks =
            Callbacks.DefaultNarrowPhaseCallbacks(materialTable, filterTable, eventBuffer)
        let poseCallbacks =
            Callbacks.DefaultPoseIntegratorCallbacks(config.Gravity)
        let solveDesc =
            SolveDescription(config.SolverIterations, config.SubstepCount)
        let sim =
            Simulation.Create<Callbacks.DefaultNarrowPhaseCallbacks, Callbacks.DefaultPoseIntegratorCallbacks>(
                pool,
                narrowCallbacks,
                poseCallbacks,
                solveDesc,
                null,
                Nullable()
            )
        let typedPoseIntegrator = sim.PoseIntegrator :?> PoseIntegrator<Callbacks.DefaultPoseIntegratorCallbacks>
        let getGravity () = typedPoseIntegrator.Callbacks.Gravity
        let setGravity (g: Vector3) = typedPoseIntegrator.Callbacks.Gravity <- g
        new PhysicsWorld(sim, pool, threadDispatcher, config, eventBuffer, materialTable, filterTable, getGravity, setGravity)

    let destroy (world: PhysicsWorld) : unit =
        (world :> IDisposable).Dispose()

    let step (dt: float32) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        if dt > 0.0f then
            let dispatcher: IThreadDispatcher =
                match world.ThreadDispatcher with
                | Some td -> td :> IThreadDispatcher
                | None -> null
            world.Sim.Timestep(dt, dispatcher)
            world.EventBuffer.SwapBuffers()

    let addShape (shape: PhysicsShape) (world: PhysicsWorld) : ShapeId =
        world.ThrowIfDisposed()
        let sim = world.Sim
        let pool = world.Pool
        let ti =
            match shape with
            | PhysicsShape.Sphere radius ->
                if radius <= 0.0f then
                    raise (exn (PhysicsError.describe (DegenerateShape "Sphere radius must be positive")))
                let mutable s = Sphere(radius)
                sim.Shapes.Add(&s)
            | PhysicsShape.Box(width, height, length) ->
                if width <= 0.0f || height <= 0.0f || length <= 0.0f then
                    raise (exn (PhysicsError.describe (DegenerateShape "Box dimensions must be positive")))
                let mutable s = Box(width, height, length)
                sim.Shapes.Add(&s)
            | PhysicsShape.Capsule(radius, length) ->
                if radius <= 0.0f || length <= 0.0f then
                    raise (exn (PhysicsError.describe (DegenerateShape "Capsule dimensions must be positive")))
                let mutable s = Capsule(radius, length)
                sim.Shapes.Add(&s)
            | PhysicsShape.Cylinder(radius, length) ->
                if radius <= 0.0f || length <= 0.0f then
                    raise (exn (PhysicsError.describe (DegenerateShape "Cylinder dimensions must be positive")))
                let mutable s = Cylinder(radius, length)
                sim.Shapes.Add(&s)
            | PhysicsShape.Triangle(a, b, c) ->
                let mutable ma = a
                let mutable mb = b
                let mutable mc = c
                let mutable s = Triangle(&ma, &mb, &mc)
                sim.Shapes.Add(&s)
            | PhysicsShape.ConvexHull points ->
                if points.Length < 4 then
                    raise (exn (PhysicsError.describe (DegenerateShape "ConvexHull needs at least 4 points")))
                let mutable center = Vector3.Zero
                let mutable hull = Unchecked.defaultof<ConvexHull>
                ConvexHullHelper.CreateShape(Span(points), pool, &center, &hull)
                sim.Shapes.Add(&hull)
            | PhysicsShape.Compound children ->
                if children.Length < 1 then
                    raise (exn (PhysicsError.describe (DegenerateShape "Compound needs at least 1 child")))
                let mutable buffer = Unchecked.defaultof<Buffer<BepuPhysics.Collidables.CompoundChild>>
                pool.Take(children.Length, &buffer)
                for i in 0 .. children.Length - 1 do
                    let child = children.[i]
                    buffer.[i] <-
                        BepuPhysics.Collidables.CompoundChild(
                            ShapeIndex = Interop.shapeIdToTypedIndex child.Shape,
                            LocalPose = Interop.poseToRigid child.LocalPose
                        )
                let mutable s = Compound(buffer)
                sim.Shapes.Add(&s)
            | PhysicsShape.Mesh triangles ->
                if triangles.Length < 1 then
                    raise (exn (PhysicsError.describe (DegenerateShape "Mesh needs at least 1 triangle")))
                let mutable buffer = Unchecked.defaultof<Buffer<Triangle>>
                pool.Take(triangles.Length, &buffer)
                for i in 0 .. triangles.Length - 1 do
                    let (a, b, c) = triangles.[i]
                    let mutable ma = a
                    let mutable mb = b
                    let mutable mc = c
                    buffer.[i] <- Triangle(&ma, &mb, &mc)
                let mutable scale = Vector3.One
                let mutable s = Mesh(buffer, &scale, pool)
                sim.Shapes.Add(&s)
        Interop.typedIndexToShapeId ti

    let removeShape (shapeId: ShapeId) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let ti = Interop.shapeIdToTypedIndex shapeId
        world.Sim.Shapes.RemoveAndDispose(ti, world.Pool)

    let getBodyPose (bodyId: BodyId) (world: PhysicsWorld) : Pose =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        let bodyRef = world.Sim.Bodies.[handle]
        Interop.rigidToPose bodyRef.Pose

    let getBodyVelocity (bodyId: BodyId) (world: PhysicsWorld) : Velocity =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        let bodyRef = world.Sim.Bodies.[handle]
        Interop.bepuToVelocity bodyRef.Velocity

    let setBodyPose (bodyId: BodyId) (pose: Pose) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        let bodyRef = world.Sim.Bodies.[handle]
        bodyRef.Pose <- Interop.poseToRigid pose

    let setBodyVelocity (bodyId: BodyId) (velocity: Velocity) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        let bodyRef = world.Sim.Bodies.[handle]
        bodyRef.Velocity <- Interop.velocityToBepu velocity

    let readPoses (ids: BodyId[]) (poses: Pose[]) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let sim = world.Sim
        for i in 0 .. ids.Length - 1 do
            let handle = Interop.bodyIdToHandle ids.[i]
            let bodyRef = sim.Bodies.[handle]
            poses.[i] <- Interop.rigidToPose bodyRef.Pose

    let readVelocities (ids: BodyId[]) (velocities: Velocity[]) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let sim = world.Sim
        for i in 0 .. ids.Length - 1 do
            let handle = Interop.bodyIdToHandle ids.[i]
            let bodyRef = sim.Bodies.[handle]
            velocities.[i] <- Interop.bepuToVelocity bodyRef.Velocity

    let writePoses (ids: BodyId[]) (poses: Pose[]) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let sim = world.Sim
        for i in 0 .. ids.Length - 1 do
            let handle = Interop.bodyIdToHandle ids.[i]
            let bodyRef = sim.Bodies.[handle]
            bodyRef.Pose <- Interop.poseToRigid poses.[i]

    let writeVelocities (ids: BodyId[]) (velocities: Velocity[]) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let sim = world.Sim
        for i in 0 .. ids.Length - 1 do
            let handle = Interop.bodyIdToHandle ids.[i]
            let bodyRef = sim.Bodies.[handle]
            bodyRef.Velocity <- Interop.velocityToBepu velocities.[i]

    let addBody (desc: DynamicBodyDesc) (world: PhysicsWorld) : BodyId =
        world.ThrowIfDisposed()
        if desc.Mass < 0.0f then
            raise (exn (PhysicsError.describe (NegativeMass desc.Mass)))
        let sim = world.Sim
        let ti = Interop.shapeIdToTypedIndex desc.Shape
        let collidable =
            if desc.ContinuousDetection then
                BepuPhysics.Collidables.CollidableDescription(ti, Collidables.ContinuousDetection.Continuous(1e-3f, 1e-2f))
            else
                BepuPhysics.Collidables.CollidableDescription(ti)
        let pose = Interop.poseToRigid desc.Pose
        let velocity = Interop.velocityToBepu desc.Velocity
        let activity = BodyActivityDescription(desc.SleepThreshold)
        let mutable bodyDesc =
            if desc.Mass = 0.0f then
                BodyDescription.CreateKinematic(pose, velocity, collidable, activity)
            else
                let inertia =
                    if ti.Type = Sphere.Id then
                        sim.Shapes.GetShape<Sphere>(ti.Index).ComputeInertia(desc.Mass)
                    elif ti.Type = Box.Id then
                        sim.Shapes.GetShape<Box>(ti.Index).ComputeInertia(desc.Mass)
                    elif ti.Type = Capsule.Id then
                        sim.Shapes.GetShape<Capsule>(ti.Index).ComputeInertia(desc.Mass)
                    elif ti.Type = Cylinder.Id then
                        sim.Shapes.GetShape<Cylinder>(ti.Index).ComputeInertia(desc.Mass)
                    elif ti.Type = Triangle.Id then
                        sim.Shapes.GetShape<Triangle>(ti.Index).ComputeInertia(desc.Mass)
                    elif ti.Type = ConvexHull.Id then
                        sim.Shapes.GetShape<ConvexHull>(ti.Index).ComputeInertia(desc.Mass)
                    else
                        let mutable fallback = Sphere(1.0f)
                        fallback.ComputeInertia(desc.Mass)
                BodyDescription.CreateDynamic(pose, velocity, inertia, collidable, activity)
        let handle = sim.Bodies.Add(&bodyDesc)
        let key = handle.Value
        world.MaterialTable.[key] <- desc.Material
        world.FilterTable.[key] <- { Group = desc.CollisionGroup; Mask = desc.CollisionMask }
        Interop.handleToBodyId handle

    let addKinematicBody (desc: KinematicBodyDesc) (world: PhysicsWorld) : BodyId =
        world.ThrowIfDisposed()
        let sim = world.Sim
        let ti = Interop.shapeIdToTypedIndex desc.Shape
        let collidable =
            if desc.ContinuousDetection then
                BepuPhysics.Collidables.CollidableDescription(ti, Collidables.ContinuousDetection.Continuous(1e-3f, 1e-2f))
            else
                BepuPhysics.Collidables.CollidableDescription(ti)
        let pose = Interop.poseToRigid desc.Pose
        let velocity = Interop.velocityToBepu desc.Velocity
        let activity = BodyActivityDescription(desc.SleepThreshold)
        let mutable bodyDesc = BodyDescription.CreateKinematic(pose, velocity, collidable, activity)
        let handle = sim.Bodies.Add(&bodyDesc)
        let key = handle.Value
        world.MaterialTable.[key] <- desc.Material
        world.FilterTable.[key] <- { Group = desc.CollisionGroup; Mask = desc.CollisionMask }
        Interop.handleToBodyId handle

    let addStatic (desc: StaticBodyDesc) (world: PhysicsWorld) : StaticId =
        world.ThrowIfDisposed()
        let sim = world.Sim
        let ti = Interop.shapeIdToTypedIndex desc.Shape
        let pose = Interop.poseToRigid desc.Pose
        let mutable staticDesc = StaticDescription(pose, ti)
        let handle = sim.Statics.Add(&staticDesc)
        let key = handle.Value ||| 0x40000000
        world.MaterialTable.[key] <- desc.Material
        world.FilterTable.[key] <- { Group = desc.CollisionGroup; Mask = desc.CollisionMask }
        Interop.handleToStaticId handle

    let removeBody (bodyId: BodyId) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        let bodyRef = world.Sim.Bodies.[handle]
        let constraints = bodyRef.Constraints
        for i in constraints.Count - 1 .. -1 .. 0 do
            let constraintRef = constraints.[i]
            let ch = constraintRef.ConnectingConstraintHandle
            world.Sim.Solver.Remove(ch)
        world.Sim.Bodies.Remove(handle)
        world.MaterialTable.Remove(handle.Value) |> ignore
        world.FilterTable.Remove(handle.Value) |> ignore

    let removeStatic (staticId: StaticId) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let handle = Interop.staticIdToHandle staticId
        world.Sim.Statics.Remove(handle)
        let key = handle.Value ||| 0x40000000
        world.MaterialTable.Remove(key) |> ignore
        world.FilterTable.Remove(key) |> ignore

    let addConstraint (bodyA: BodyId) (bodyB: BodyId) (desc: ConstraintDesc) (world: PhysicsWorld) : ConstraintId =
        world.ThrowIfDisposed()
        let handleA = Interop.bodyIdToHandle bodyA
        let handleB = Interop.bodyIdToHandle bodyB
        let solver = world.Sim.Solver
        let ch =
            match desc with
            | ConstraintDesc.BallSocket(offsetA, offsetB, spring) ->
                let mutable c = BepuPhysics.Constraints.BallSocket(
                    LocalOffsetA = offsetA,
                    LocalOffsetB = offsetB,
                    SpringSettings = Interop.springToBepu spring)
                solver.Add(handleA, handleB, &c)
            | ConstraintDesc.Hinge(hingeA, hingeB, offsetA, offsetB, spring) ->
                let mutable c = BepuPhysics.Constraints.Hinge(
                    LocalHingeAxisA = hingeA,
                    LocalHingeAxisB = hingeB,
                    LocalOffsetA = offsetA,
                    LocalOffsetB = offsetB,
                    SpringSettings = Interop.springToBepu spring)
                solver.Add(handleA, handleB, &c)
            | ConstraintDesc.Weld(offset, orientation, spring) ->
                let mutable c = BepuPhysics.Constraints.Weld(
                    LocalOffset = offset,
                    LocalOrientation = orientation,
                    SpringSettings = Interop.springToBepu spring)
                solver.Add(handleA, handleB, &c)
            | ConstraintDesc.DistanceLimit(offsetA, offsetB, minDist, maxDist, spring) ->
                let mutable c = BepuPhysics.Constraints.DistanceLimit(
                    LocalOffsetA = offsetA,
                    LocalOffsetB = offsetB,
                    MinimumDistance = minDist,
                    MaximumDistance = maxDist,
                    SpringSettings = Interop.springToBepu spring)
                solver.Add(handleA, handleB, &c)
            | ConstraintDesc.DistanceSpring(offsetA, offsetB, targetDist, spring) ->
                let mutable c = BepuPhysics.Constraints.DistanceServo(
                    LocalOffsetA = offsetA,
                    LocalOffsetB = offsetB,
                    TargetDistance = targetDist,
                    SpringSettings = Interop.springToBepu spring)
                solver.Add(handleA, handleB, &c)
            | ConstraintDesc.SwingLimit(axisA, axisB, maxAngle, spring) ->
                let minDot = System.MathF.Cos(maxAngle)
                let mutable c = BepuPhysics.Constraints.SwingLimit(
                    AxisLocalA = axisA,
                    AxisLocalB = axisB,
                    MinimumDot = minDot,
                    SpringSettings = Interop.springToBepu spring)
                solver.Add(handleA, handleB, &c)
            | ConstraintDesc.TwistLimit(axisA, axisB, minAngle, maxAngle, spring) ->
                let mutable c = BepuPhysics.Constraints.TwistLimit(
                    LocalBasisA = Quaternion.CreateFromAxisAngle(axisA, 0.0f),
                    LocalBasisB = Quaternion.CreateFromAxisAngle(axisB, 0.0f),
                    MinimumAngle = minAngle,
                    MaximumAngle = maxAngle,
                    SpringSettings = Interop.springToBepu spring)
                solver.Add(handleA, handleB, &c)
            | ConstraintDesc.LinearAxisMotor(offsetA, offsetB, axis, targetVel, settings) ->
                let mutable c = BepuPhysics.Constraints.LinearAxisMotor(
                    LocalOffsetA = offsetA,
                    LocalOffsetB = offsetB,
                    LocalAxis = axis,
                    TargetVelocity = targetVel,
                    Settings = BepuPhysics.Constraints.MotorSettings(settings.MaxForce, settings.Damping))
                solver.Add(handleA, handleB, &c)
            | ConstraintDesc.AngularMotor(targetVel, settings) ->
                let mutable c = BepuPhysics.Constraints.AngularMotor(
                    TargetVelocityLocalA = targetVel,
                    Settings = BepuPhysics.Constraints.MotorSettings(settings.MaxForce, settings.Damping))
                solver.Add(handleA, handleB, &c)
            | ConstraintDesc.PointOnLine(origin, direction, offset, spring) ->
                let mutable c = BepuPhysics.Constraints.PointOnLineServo(
                    LocalOffsetA = origin,
                    LocalOffsetB = offset,
                    LocalDirection = direction,
                    SpringSettings = Interop.springToBepu spring)
                solver.Add(handleA, handleB, &c)
        Interop.handleToConstraintId ch

    let removeConstraint (constraintId: ConstraintId) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let handle = Interop.constraintIdToHandle constraintId
        world.Sim.Solver.Remove(handle)

    let raycast (origin: Vector3) (direction: Vector3) (maxDistance: float32) (world: PhysicsWorld) : RayHit option =
        world.ThrowIfDisposed()
        let mutable handler = Queries.SingleHitHandler(maxDistance)
        let mutable o = origin
        let mutable d = direction
        world.Sim.RayCast(&o, &d, maxDistance, &handler)
        match handler.Hit with
        | ValueSome hit -> Some hit
        | ValueNone -> None

    let raycastAll (origin: Vector3) (direction: Vector3) (maxDistance: float32) (world: PhysicsWorld) : RayHit[] =
        world.ThrowIfDisposed()
        let mutable handler = Queries.MultiHitHandler(16)
        let mutable o = origin
        let mutable d = direction
        world.Sim.RayCast(&o, &d, maxDistance, &handler)
        let result = handler.Hits.ToArray()
        Array.sortInPlaceBy (fun (h: RayHit) -> h.Distance) result
        result

    let getContactEvents (world: PhysicsWorld) : ContactEvent[] =
        world.ThrowIfDisposed()
        world.EventBuffer.GetEvents()

    let simulation (world: PhysicsWorld) : Simulation =
        world.ThrowIfDisposed()
        world.Sim

    let bufferPool (world: PhysicsWorld) : BufferPool =
        world.ThrowIfDisposed()
        world.Pool

    let bodyExists (bodyId: BodyId) (world: PhysicsWorld) : bool =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        world.Sim.Bodies.BodyExists(handle)

    let staticExists (staticId: StaticId) (world: PhysicsWorld) : bool =
        world.ThrowIfDisposed()
        let handle = Interop.staticIdToHandle staticId
        world.Sim.Statics.StaticExists(handle)

    let tryGetBodyPose (bodyId: BodyId) (world: PhysicsWorld) : Pose voption =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        if world.Sim.Bodies.BodyExists(handle) then
            ValueSome(Interop.rigidToPose world.Sim.Bodies.[handle].Pose)
        else
            ValueNone

    let tryGetBodyVelocity (bodyId: BodyId) (world: PhysicsWorld) : Velocity voption =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        if world.Sim.Bodies.BodyExists(handle) then
            ValueSome(Interop.bepuToVelocity world.Sim.Bodies.[handle].Velocity)
        else
            ValueNone

    let trySetBodyPose (bodyId: BodyId) (pose: Pose) (world: PhysicsWorld) : bool =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        if world.Sim.Bodies.BodyExists(handle) then
            world.Sim.Bodies.[handle].Pose <- Interop.poseToRigid pose
            true
        else
            false

    let trySetBodyVelocity (bodyId: BodyId) (velocity: Velocity) (world: PhysicsWorld) : bool =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        if world.Sim.Bodies.BodyExists(handle) then
            world.Sim.Bodies.[handle].Velocity <- Interop.velocityToBepu velocity
            true
        else
            false

    let applyImpulse (bodyId: BodyId) (impulse: Vector3) (offset: Vector3) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        if world.Sim.Bodies.BodyExists(handle) then
            let mutable imp = impulse
            let mutable off = offset
            world.Sim.Bodies.[handle].ApplyImpulse(&imp, &off)

    let applyLinearImpulse (bodyId: BodyId) (impulse: Vector3) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        if world.Sim.Bodies.BodyExists(handle) then
            let mutable imp = impulse
            world.Sim.Bodies.[handle].ApplyLinearImpulse(&imp)

    let applyAngularImpulse (bodyId: BodyId) (impulse: Vector3) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        if world.Sim.Bodies.BodyExists(handle) then
            let mutable imp = impulse
            world.Sim.Bodies.[handle].ApplyAngularImpulse(&imp)

    let getAllBodyIds (world: PhysicsWorld) : BodyId[] =
        world.ThrowIfDisposed()
        let bodies = world.Sim.Bodies
        let result = Collections.Generic.List<BodyId>()
        for setIndex in 0 .. bodies.Sets.Length - 1 do
            let set = bodies.Sets.[setIndex]
            if set.Allocated then
                for i in 0 .. set.Count - 1 do
                    result.Add(Interop.handleToBodyId set.IndexToHandle.[i])
        result.ToArray()

    let getAllStaticIds (world: PhysicsWorld) : StaticId[] =
        world.ThrowIfDisposed()
        let statics = world.Sim.Statics
        let result = Collections.Generic.List<StaticId>()
        for i in 0 .. statics.Count - 1 do
            result.Add(Interop.handleToStaticId statics.IndexToHandle.[i])
        result.ToArray()

    let setGravity (gravity: Vector3) (world: PhysicsWorld) : unit =
        world.ThrowIfDisposed()
        world.SetGravity(gravity)

    let getGravity (world: PhysicsWorld) : Vector3 =
        world.ThrowIfDisposed()
        world.GetGravity()

    let getBodyShape (bodyId: BodyId) (world: PhysicsWorld) : PhysicsShape option =
        world.ThrowIfDisposed()
        let handle = Interop.bodyIdToHandle bodyId
        if not (world.Sim.Bodies.BodyExists(handle)) then
            None
        else
            let bodyRef = world.Sim.Bodies.[handle]
            let ti = bodyRef.Collidable.Shape
            if ti.Type = Sphere.Id then
                let shape = world.Sim.Shapes.GetShape<Sphere>(ti.Index)
                Some(PhysicsShape.Sphere shape.Radius)
            elif ti.Type = Box.Id then
                let shape = world.Sim.Shapes.GetShape<Box>(ti.Index)
                Some(PhysicsShape.Box(shape.Width, shape.Height, shape.Length))
            elif ti.Type = Capsule.Id then
                let shape = world.Sim.Shapes.GetShape<Capsule>(ti.Index)
                Some(PhysicsShape.Capsule(shape.Radius, shape.Length))
            elif ti.Type = Cylinder.Id then
                let shape = world.Sim.Shapes.GetShape<Cylinder>(ti.Index)
                Some(PhysicsShape.Cylinder(shape.Radius, shape.Length))
            elif ti.Type = Triangle.Id then
                let shape = world.Sim.Shapes.GetShape<Triangle>(ti.Index)
                Some(PhysicsShape.Triangle(shape.A, shape.B, shape.C))
            elif ti.Type = ConvexHull.Id then
                Some(PhysicsShape.ConvexHull [||])
            elif ti.Type = Compound.Id then
                Some(PhysicsShape.Compound [||])
            elif ti.Type = Mesh.Id then
                Some(PhysicsShape.Mesh [||])
            else
                None
