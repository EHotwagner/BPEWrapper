module internal BepuFSharp.Callbacks

open System
open System.Numerics
open System.Runtime.CompilerServices
open BepuPhysics
open BepuPhysics.Collidables
open BepuPhysics.CollisionDetection
open BepuPhysics.Trees
open BepuUtilities

[<Struct>]
type DefaultNarrowPhaseCallbacks =
    val mutable MaterialLookup: Collections.Generic.Dictionary<int, MaterialProperties>
    val mutable FilterLookup: Collections.Generic.Dictionary<int, CollisionFilter>
    val mutable EventBuffer: ContactEvents.ContactEventBuffer

    new(materials: Collections.Generic.Dictionary<int, MaterialProperties>,
        filters: Collections.Generic.Dictionary<int, CollisionFilter>,
        events: ContactEvents.ContactEventBuffer) =
        { MaterialLookup = materials; FilterLookup = filters; EventBuffer = events }

    interface INarrowPhaseCallbacks with
        member _.Initialize(_simulation: Simulation) = ()

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.AllowContactGeneration(
            _workerIndex: int,
            a: CollidableReference,
            b: CollidableReference,
            _speculativeMargin: float32 byref) : bool =

            if isNull this.FilterLookup then true
            else
                let aIndex =
                    if a.Mobility = CollidableMobility.Static then a.RawHandleValue ||| 0x40000000
                    else a.RawHandleValue

                let bIndex =
                    if b.Mobility = CollidableMobility.Static then b.RawHandleValue ||| 0x40000000
                    else b.RawHandleValue

                let mutable filterA = Unchecked.defaultof<CollisionFilter>
                let mutable filterB = Unchecked.defaultof<CollisionFilter>
                let hasA = this.FilterLookup.TryGetValue(aIndex, &filterA)
                let hasB = this.FilterLookup.TryGetValue(bIndex, &filterB)

                if not hasA || not hasB then true
                else
                    (filterA.Mask &&& (1u <<< int filterB.Group)) <> 0u
                    && (filterB.Mask &&& (1u <<< int filterA.Group)) <> 0u

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.ConfigureContactManifold<'TManifold
            when 'TManifold : unmanaged
            and 'TManifold : (new : unit -> 'TManifold)
            and 'TManifold :> ValueType
            and 'TManifold :> IContactManifold<'TManifold>>(
            _workerIndex: int,
            pair: CollidablePair,
            _manifold: 'TManifold byref,
            pairMaterial: PairMaterialProperties byref) : bool =

            // Look up per-body materials
            let aKey =
                if pair.A.Mobility = CollidableMobility.Static then pair.A.RawHandleValue ||| 0x40000000
                else pair.A.RawHandleValue
            let bKey =
                if pair.B.Mobility = CollidableMobility.Static then pair.B.RawHandleValue ||| 0x40000000
                else pair.B.RawHandleValue

            let mutable matA = Unchecked.defaultof<MaterialProperties>
            let mutable matB = Unchecked.defaultof<MaterialProperties>
            let hasA = not (isNull this.MaterialLookup) && this.MaterialLookup.TryGetValue(aKey, &matA)
            let hasB = not (isNull this.MaterialLookup) && this.MaterialLookup.TryGetValue(bKey, &matB)

            let mat =
                if hasA && hasB then
                    { Friction = MathF.Min(matA.Friction, matB.Friction)
                      MaxRecoveryVelocity = MathF.Max(matA.MaxRecoveryVelocity, matB.MaxRecoveryVelocity)
                      SpringFrequency = (matA.SpringFrequency + matB.SpringFrequency) * 0.5f
                      SpringDampingRatio = (matA.SpringDampingRatio + matB.SpringDampingRatio) * 0.5f }
                elif hasA then matA
                elif hasB then matB
                else MaterialProperties.defaults

            pairMaterial.FrictionCoefficient <- mat.Friction
            pairMaterial.MaximumRecoveryVelocity <- mat.MaxRecoveryVelocity
            pairMaterial.SpringSettings <- BepuPhysics.Constraints.SpringSettings(mat.SpringFrequency, mat.SpringDampingRatio)

            // Record contact event
            if not (isNull this.EventBuffer) then
                let aHandle = pair.A.RawHandleValue
                let bHandle = pair.B.RawHandleValue
                let aIsStatic = pair.A.Mobility = CollidableMobility.Static
                let bIsStatic = pair.B.Mobility = CollidableMobility.Static

                let aTableKey = if aIsStatic then aHandle ||| 0x40000000 else aHandle
                let bTableKey = if bIsStatic then bHandle ||| 0x40000000 else bHandle

                this.EventBuffer.TrackPair(aTableKey, bTableKey)

                let evt : ContactEvent =
                    { BodyA = if not aIsStatic then ValueSome(BodyId aHandle) else ValueNone
                      StaticA = if aIsStatic then ValueSome(StaticId aHandle) else ValueNone
                      BodyB = if not bIsStatic then ValueSome(BodyId bHandle) else ValueNone
                      StaticB = if bIsStatic then ValueSome(StaticId bHandle) else ValueNone
                      Normal = Vector3.UnitY
                      Depth = 0.0f
                      EventType = Began }
                this.EventBuffer.AppendContact(evt)

            true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member _.AllowContactGeneration(
            _workerIndex: int,
            _pair: CollidablePair,
            _childIndexA: int,
            _childIndexB: int) : bool = true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member _.ConfigureContactManifold(
            _workerIndex: int,
            _pair: CollidablePair,
            _childIndexA: int,
            _childIndexB: int,
            _manifold: ConvexContactManifold byref) : bool = true

        member _.Dispose() = ()


[<Struct>]
type DefaultPoseIntegratorCallbacks =
    val mutable Gravity: Vector3
    val mutable GravityWideDt: Vector3Wide

    new(gravity: Vector3) =
        { Gravity = gravity; GravityWideDt = Unchecked.defaultof<Vector3Wide> }

    interface IPoseIntegratorCallbacks with
        member _.AngularIntegrationMode = AngularIntegrationMode.Nonconserving

        member _.AllowSubstepsForUnconstrainedBodies = false

        member _.IntegrateVelocityForKinematics = false

        member _.Initialize(_simulation: Simulation) = ()

        member this.PrepareForIntegration(dt: float32) =
            let gravityDt = this.Gravity * dt
            Vector3Wide.Broadcast(&gravityDt, &this.GravityWideDt)

        member this.IntegrateVelocity(
            _bodyIndices: Vector<int>,
            _position: Vector3Wide,
            _orientation: QuaternionWide,
            _localInertia: BodyInertiaWide,
            _integrationMask: Vector<int>,
            _workerIndex: int,
            _dt: Vector<float32>,
            velocity: BodyVelocityWide byref) =
            Vector3Wide.Add(&velocity.Linear, &this.GravityWideDt, &velocity.Linear)
