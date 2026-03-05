module internal BepuFSharp.Interop

open System.Numerics
open System.Runtime.CompilerServices
open BepuPhysics
open BepuPhysics.Collidables
open BepuPhysics.Constraints

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let poseToRigid (pose: Pose) : RigidPose =
    RigidPose(pose.Position, pose.Orientation)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let rigidToPose (rp: RigidPose) : Pose =
    { Position = rp.Position; Orientation = rp.Orientation }

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let velocityToBepu (v: Velocity) : BodyVelocity =
    BodyVelocity(v.Linear, v.Angular)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let bepuToVelocity (bv: BodyVelocity) : Velocity =
    { Linear = bv.Linear; Angular = bv.Angular }

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let springToBepu (sc: SpringConfig) : SpringSettings =
    SpringSettings(sc.Frequency, sc.DampingRatio)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let bepuToSpring (ss: SpringSettings) : SpringConfig =
    { Frequency = ss.Frequency; DampingRatio = ss.DampingRatio }

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let shapeIdToTypedIndex (sid: ShapeId) : TypedIndex =
    TypedIndex(sid.TypeId, sid.Index)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let typedIndexToShapeId (ti: TypedIndex) : ShapeId =
    { TypeId = ti.Type; Index = ti.Index }

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let bodyIdToHandle (BodyId id) : BodyHandle =
    BodyHandle(id)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let handleToBodyId (h: BodyHandle) : BodyId =
    BodyId h.Value

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let staticIdToHandle (StaticId id) : StaticHandle =
    StaticHandle(id)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let handleToStaticId (h: StaticHandle) : StaticId =
    StaticId h.Value

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let constraintIdToHandle (ConstraintId id) : ConstraintHandle =
    ConstraintHandle(id)

[<MethodImpl(MethodImplOptions.AggressiveInlining)>]
let handleToConstraintId (h: ConstraintHandle) : ConstraintId =
    ConstraintId h.Value
