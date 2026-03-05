namespace BepuFSharp

type PhysicsError =
    | InvalidBodyHandle of BodyId
    | InvalidStaticHandle of StaticId
    | InvalidShapeHandle of ShapeId
    | InvalidConstraintHandle of ConstraintId
    | NegativeMass of float32
    | DegenerateShape of string
    | ShapeInUse of ShapeId
    | WorldDisposed
    | BufferPoolExhausted

type PhysicsDiagnosticEvent =
    | WorldCreated
    | WorldDisposed
    | ConstraintAutoRemoved of ConstraintId: ConstraintId * bodyId: BodyId
    | ShapeRemovalBlocked of ShapeId
    | BufferPoolWarning of message: string

[<RequireQualifiedAccess>]
module PhysicsError =
    let describe (error: PhysicsError) : string =
        match error with
        | InvalidBodyHandle(BodyId id) -> $"Invalid body handle: %d{id}"
        | InvalidStaticHandle(StaticId id) -> $"Invalid static handle: %d{id}"
        | InvalidShapeHandle shape -> $"Invalid shape handle: type=%d{shape.TypeId} index=%d{shape.Index}"
        | InvalidConstraintHandle(ConstraintId id) -> $"Invalid constraint handle: %d{id}"
        | NegativeMass mass -> $"Negative mass not allowed: %f{mass}"
        | DegenerateShape msg -> $"Degenerate shape: %s{msg}"
        | ShapeInUse shape -> $"Shape still in use: type=%d{shape.TypeId} index=%d{shape.Index}"
        | PhysicsError.WorldDisposed -> "Physics world has been disposed"
        | BufferPoolExhausted -> "Buffer pool memory exhausted"
