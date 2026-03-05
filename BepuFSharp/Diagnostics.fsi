namespace BepuFSharp

/// Errors that can occur during physics operations.
type PhysicsError =
    /// The body handle does not refer to a valid body.
    | InvalidBodyHandle of BodyId
    /// The static handle does not refer to a valid static.
    | InvalidStaticHandle of StaticId
    /// The shape handle does not refer to a valid shape.
    | InvalidShapeHandle of ShapeId
    /// The constraint handle does not refer to a valid constraint.
    | InvalidConstraintHandle of ConstraintId
    /// A body was created with negative mass.
    | NegativeMass of float32
    /// A shape has degenerate dimensions (zero or negative).
    | DegenerateShape of string
    /// A shape is still referenced by bodies and cannot be removed.
    | ShapeInUse of ShapeId
    /// The physics world has been disposed and cannot be used.
    | WorldDisposed
    /// The buffer pool has been exhausted.
    | BufferPoolExhausted

/// Diagnostic events emitted during physics operations for observability.
type PhysicsDiagnosticEvent =
    /// A new physics world was created.
    | WorldCreated
    /// A physics world was disposed.
    | WorldDisposed
    /// A constraint was automatically removed when its body was removed.
    | ConstraintAutoRemoved of ConstraintId: ConstraintId * bodyId: BodyId
    /// Shape removal was blocked because bodies still reference it.
    | ShapeRemovalBlocked of ShapeId
    /// The buffer pool is running low on memory.
    | BufferPoolWarning of message: string

/// Functions for working with physics errors.
[<RequireQualifiedAccess>]
module PhysicsError =
    /// Get a human-readable description of a physics error.
    val describe: PhysicsError -> string
