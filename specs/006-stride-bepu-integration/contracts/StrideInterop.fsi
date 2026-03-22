/// Bidirectional type conversions between BepuFSharp and Stride.BepuPhysics types.
/// All functions are pure (no side effects or state).
[<RequireQualifiedAccess>]
module BepuFSharp.StrideInterop

open BepuFSharp

// --- Shape Conversions (FR-015) ---

/// Convert a BepuFSharp PhysicsShape to the corresponding Stride.BepuPhysics collider.
/// Raises for unsupported shape types (e.g., Triangle has no Stride equivalent).
val toStrideCollider: PhysicsShape -> Stride.BepuPhysics.Definitions.Colliders.ColliderBase

/// Convert a Stride.BepuPhysics collider to a BepuFSharp PhysicsShape.
/// Raises for unrecognized collider types.
val fromStrideCollider: Stride.BepuPhysics.Definitions.Colliders.ColliderBase -> PhysicsShape

// --- CollisionFilter Conversions (FR-016) ---

/// Convert a BepuFSharp CollisionFilter to Stride collision group representation.
val toStrideCollisionGroup: CollisionFilter -> Stride.BepuPhysics.Definitions.CollisionGroup

/// Convert a Stride collision group to BepuFSharp CollisionFilter.
val fromStrideCollisionGroup: Stride.BepuPhysics.Definitions.CollisionGroup -> CollisionFilter

// --- MaterialProperties Conversions (FR-016) ---

/// Convert BepuFSharp MaterialProperties to Stride-compatible material values.
val toStrideMaterial: MaterialProperties -> Stride.BepuPhysics.Definitions.MaterialProperties

/// Convert Stride material values to BepuFSharp MaterialProperties.
val fromStrideMaterial: Stride.BepuPhysics.Definitions.MaterialProperties -> MaterialProperties

// --- ConstraintDesc Conversions (FR-016) ---

/// Convert a BepuFSharp ConstraintDesc to the corresponding Stride constraint component.
val toStrideConstraint: ConstraintDesc -> Stride.BepuPhysics.Definitions.Constraints.ConstraintBase

/// Convert a Stride constraint component to BepuFSharp ConstraintDesc.
val fromStrideConstraint: Stride.BepuPhysics.Definitions.Constraints.ConstraintBase -> ConstraintDesc
