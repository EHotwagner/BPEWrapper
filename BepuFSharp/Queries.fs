module internal BepuFSharp.Queries

open System.Numerics
open System.Runtime.CompilerServices
open BepuPhysics
open BepuPhysics.Collidables
open BepuPhysics.Trees
open BepuUtilities.Memory

/// Check whether a query filter allows testing against a collidable.
/// Uses the bidirectional mask logic: both sides must have the other's group bit set.
[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let checkFilter
    (queryFilter: CollisionFilter)
    (collidable: CollidableReference)
    (filterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>)
    : bool =
    let key =
        if collidable.Mobility = CollidableMobility.Static then collidable.RawHandleValue ||| 0x40000000
        else collidable.RawHandleValue
    let mutable bodyFilter = Unchecked.defaultof<CollisionFilter>
    if filterLookup.TryGetValue(key, &bodyFilter) then
        (queryFilter.Mask &&& (1u <<< int bodyFilter.Group)) <> 0u
        && (bodyFilter.Mask &&& (1u <<< int queryFilter.Group)) <> 0u
    else
        true // no filter registered = allow

[<Struct>]
type SingleHitHandler =
    val mutable Hit: RayHit voption
    val mutable MaxT: float32
    val mutable Filter: CollisionFilter voption
    val mutable FilterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>

    new(maxT: float32, filter: CollisionFilter voption, filterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>) =
        { Hit = ValueNone; MaxT = maxT; Filter = filter; FilterLookup = filterLookup }

    interface IRayHitHandler with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.AllowTest(collidable: CollidableReference) : bool =
            match this.Filter with
            | ValueSome f -> checkFilter f collidable this.FilterLookup
            | ValueNone -> true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.AllowTest(collidable: CollidableReference, _childIndex: int) : bool =
            match this.Filter with
            | ValueSome f -> checkFilter f collidable this.FilterLookup
            | ValueNone -> true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.OnRayHit(ray: RayData inref, maximumT: float32 byref, t: float32, normal: Vector3, collidable: CollidableReference, _childIndex: int) =
            if t < this.MaxT then
                this.MaxT <- t
                maximumT <- t
                let hitPos = ray.Origin + ray.Direction * t
                let body, static' =
                    if collidable.Mobility <> CollidableMobility.Static then
                        ValueSome(Interop.handleToBodyId (BodyHandle(collidable.RawHandleValue))),
                        ValueNone
                    else
                        ValueNone,
                        ValueSome(Interop.handleToStaticId (StaticHandle(collidable.RawHandleValue)))
                this.Hit <- ValueSome {
                    Body = body
                    Static = static'
                    Position = hitPos
                    Normal = normal
                    Distance = t
                }

[<Struct>]
type MultiHitHandler =
    val mutable Hits: ResizeArray<RayHit>
    val mutable Filter: CollisionFilter voption
    val mutable FilterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>

    new(capacity: int, filter: CollisionFilter voption, filterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>) =
        { Hits = ResizeArray<RayHit>(capacity); Filter = filter; FilterLookup = filterLookup }

    interface IRayHitHandler with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.AllowTest(collidable: CollidableReference) : bool =
            match this.Filter with
            | ValueSome f -> checkFilter f collidable this.FilterLookup
            | ValueNone -> true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.AllowTest(collidable: CollidableReference, _childIndex: int) : bool =
            match this.Filter with
            | ValueSome f -> checkFilter f collidable this.FilterLookup
            | ValueNone -> true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.OnRayHit(ray: RayData inref, _maximumT: float32 byref, t: float32, normal: Vector3, collidable: CollidableReference, _childIndex: int) =
            let hitPos = ray.Origin + ray.Direction * t
            let body, static' =
                if collidable.Mobility <> CollidableMobility.Static then
                    ValueSome(Interop.handleToBodyId (BodyHandle(collidable.RawHandleValue))),
                    ValueNone
                else
                    ValueNone,
                    ValueSome(Interop.handleToStaticId (StaticHandle(collidable.RawHandleValue)))
            this.Hits.Add({
                Body = body
                Static = static'
                Position = hitPos
                Normal = normal
                Distance = t
            })

[<Struct>]
type SweepHitHandler =
    val mutable Hit: SweepHit voption
    val mutable MaxT: float32
    val mutable Filter: CollisionFilter voption
    val mutable FilterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>

    new(maxT: float32, filter: CollisionFilter voption, filterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>) =
        { Hit = ValueNone; MaxT = maxT; Filter = filter; FilterLookup = filterLookup }

    interface ISweepHitHandler with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.AllowTest(collidable: CollidableReference) : bool =
            match this.Filter with
            | ValueSome f -> checkFilter f collidable this.FilterLookup
            | ValueNone -> true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.AllowTest(collidable: CollidableReference, _childIndex: int) : bool =
            match this.Filter with
            | ValueSome f -> checkFilter f collidable this.FilterLookup
            | ValueNone -> true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.OnHit(maximumT: float32 byref, t: float32, hitLocation: Vector3, hitNormal: Vector3, collidable: CollidableReference) =
            if t < this.MaxT && t > 0.0f then
                this.MaxT <- t
                maximumT <- t
                let body, static' =
                    if collidable.Mobility <> CollidableMobility.Static then
                        ValueSome(Interop.handleToBodyId (BodyHandle(collidable.RawHandleValue))),
                        ValueNone
                    else
                        ValueNone,
                        ValueSome(Interop.handleToStaticId (StaticHandle(collidable.RawHandleValue)))
                this.Hit <- ValueSome {
                    Body = body
                    Static = static'
                    Position = hitLocation
                    Normal = hitNormal
                    Distance = t
                }

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member _.OnHitAtZeroT(_maximumT: float32 byref, _collidable: CollidableReference) =
            () // Ignore contacts at start position

let dispatchSweep
    (shape: PhysicsShape)
    (pose: RigidPose)
    (velocity: BodyVelocity)
    (maxT: float32)
    (sim: Simulation)
    (pool: BufferPool)
    (handler: SweepHitHandler byref)
    : unit =
    match shape with
    | PhysicsShape.Sphere radius ->
        if radius <= 0.0f then () // degenerate — no sweep
        else
            let mutable s = Sphere(radius)
            sim.Sweep(&s, &pose, &velocity, maxT, pool, &handler)
    | PhysicsShape.Box(width, height, length) ->
        if width <= 0.0f || height <= 0.0f || length <= 0.0f then ()
        else
            let mutable s = Box(width, height, length)
            sim.Sweep(&s, &pose, &velocity, maxT, pool, &handler)
    | PhysicsShape.Capsule(radius, length) ->
        if radius <= 0.0f || length <= 0.0f then ()
        else
            let mutable s = Capsule(radius, length)
            sim.Sweep(&s, &pose, &velocity, maxT, pool, &handler)
    | PhysicsShape.Cylinder(radius, length) ->
        if radius <= 0.0f || length <= 0.0f then ()
        else
            let mutable s = Cylinder(radius, length)
            sim.Sweep(&s, &pose, &velocity, maxT, pool, &handler)
    | PhysicsShape.Triangle(a, b, c) ->
        let mutable s = Triangle(a, b, c)
        sim.Sweep(&s, &pose, &velocity, maxT, pool, &handler)
    | PhysicsShape.ConvexHull points ->
        if points.Length < 4 then ()
        else
            let mutable center = Vector3.Zero
            let mutable hull = Unchecked.defaultof<ConvexHull>
            ConvexHullHelper.CreateShape(System.Span(points), pool, &center, &hull) |> ignore
            sim.Sweep(&hull, &pose, &velocity, maxT, pool, &handler)
            hull.Dispose(pool)
    | PhysicsShape.Compound children ->
        // Decompose: sweep each child convex shape with its local pose composed onto the overall pose.
        // The child's ShapeId references a registered convex shape in the simulation's shape batch.
        // We dispatch based on the registered shape's type ID.
        for i in 0 .. children.Length - 1 do
            let child = children.[i]
            let childLocalPose = Interop.poseToRigid child.LocalPose
            // Compose child local pose with overall sweep pose
            let mutable composedPose = RigidPose()
            RigidPose.MultiplyWithoutOverlap(&childLocalPose, &pose, &composedPose)
            let ti = Interop.shapeIdToTypedIndex child.Shape
            if ti.Type = Sphere.Id then
                let mutable s = sim.Shapes.GetShape<Sphere>(ti.Index)
                sim.Sweep(&s, &composedPose, &velocity, maxT, pool, &handler)
            elif ti.Type = Box.Id then
                let mutable s = sim.Shapes.GetShape<Box>(ti.Index)
                sim.Sweep(&s, &composedPose, &velocity, maxT, pool, &handler)
            elif ti.Type = Capsule.Id then
                let mutable s = sim.Shapes.GetShape<Capsule>(ti.Index)
                sim.Sweep(&s, &composedPose, &velocity, maxT, pool, &handler)
            elif ti.Type = Cylinder.Id then
                let mutable s = sim.Shapes.GetShape<Cylinder>(ti.Index)
                sim.Sweep(&s, &composedPose, &velocity, maxT, pool, &handler)
            elif ti.Type = Triangle.Id then
                let mutable s = sim.Shapes.GetShape<Triangle>(ti.Index)
                sim.Sweep(&s, &composedPose, &velocity, maxT, pool, &handler)
            elif ti.Type = ConvexHull.Id then
                let mutable s = sim.Shapes.GetShape<ConvexHull>(ti.Index)
                sim.Sweep(&s, &composedPose, &velocity, maxT, pool, &handler)
    | PhysicsShape.Mesh triangles ->
        // Sweep each triangle individually — a Triangle is IConvexShape.
        for i in 0 .. triangles.Length - 1 do
            let (a, b, c) = triangles.[i]
            let mutable s = Triangle(a, b, c)
            sim.Sweep(&s, &pose, &velocity, maxT, pool, &handler)

/// Collects broad-phase overlap candidates from Tree.GetOverlaps.
/// Receives leaf indices which map to CollidableReferences via the BroadPhase leaves buffer.
[<Struct>]
type ActiveOverlapCollector =
    val mutable Candidates: ResizeArray<CollidableReference>
    val mutable Filter: CollisionFilter voption
    val mutable FilterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>
    val mutable BroadPhase: BepuPhysics.CollisionDetection.BroadPhase

    new(filter: CollisionFilter voption,
        filterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>,
        broadPhase: BepuPhysics.CollisionDetection.BroadPhase) =
        { Candidates = ResizeArray<CollidableReference>(16)
          Filter = filter
          FilterLookup = filterLookup
          BroadPhase = broadPhase }

    interface BepuUtilities.IBreakableForEach<int> with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.LoopBody(leafIndex: int) : bool =
            let collidable = this.BroadPhase.ActiveLeaves.[leafIndex]
            match this.Filter with
            | ValueSome f ->
                if checkFilter f collidable this.FilterLookup then
                    this.Candidates.Add(collidable)
            | ValueNone ->
                this.Candidates.Add(collidable)
            true

/// Collector for the static tree overlap pass.
[<Struct>]
type StaticOverlapCollector =
    val mutable Candidates: ResizeArray<CollidableReference>
    val mutable Filter: CollisionFilter voption
    val mutable FilterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>
    val mutable BroadPhase: BepuPhysics.CollisionDetection.BroadPhase

    new(candidates: ResizeArray<CollidableReference>,
        filter: CollisionFilter voption,
        filterLookup: System.Collections.Generic.Dictionary<int, CollisionFilter>,
        broadPhase: BepuPhysics.CollisionDetection.BroadPhase) =
        { Candidates = candidates; Filter = filter; FilterLookup = filterLookup; BroadPhase = broadPhase }

    interface BepuUtilities.IBreakableForEach<int> with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.LoopBody(leafIndex: int) : bool =
            let collidable = this.BroadPhase.StaticLeaves.[leafIndex]
            match this.Filter with
            | ValueSome f ->
                if checkFilter f collidable this.FilterLookup then
                    this.Candidates.Add(collidable)
            | ValueNone ->
                this.Candidates.Add(collidable)
            true
