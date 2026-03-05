module internal BepuFSharp.Queries

open System.Numerics
open System.Runtime.CompilerServices
open BepuPhysics
open BepuPhysics.Collidables
open BepuPhysics.Trees

[<Struct>]
type SingleHitHandler =
    val mutable Hit: RayHit voption
    val mutable MaxT: float32

    new(maxT: float32) = { Hit = ValueNone; MaxT = maxT }

    interface IRayHitHandler with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member _.AllowTest(_collidable: CollidableReference) : bool = true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member _.AllowTest(_collidable: CollidableReference, _childIndex: int) : bool = true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.OnRayHit(ray: RayData inref, maximumT: float32 byref, t: float32, normal: Vector3 inref, collidable: CollidableReference, _childIndex: int) =
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

    new(capacity: int) = { Hits = ResizeArray<RayHit>(capacity) }

    interface IRayHitHandler with
        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member _.AllowTest(_collidable: CollidableReference) : bool = true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member _.AllowTest(_collidable: CollidableReference, _childIndex: int) : bool = true

        [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
        member this.OnRayHit(ray: RayData inref, _maximumT: float32 byref, t: float32, normal: Vector3 inref, collidable: CollidableReference, _childIndex: int) =
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
