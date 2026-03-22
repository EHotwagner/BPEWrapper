namespace BepuFSharp

open Stride.BepuPhysics.Definitions.Colliders

/// Bidirectional type conversions between BepuFSharp and Stride.BepuPhysics types.
/// All functions are pure (no side effects or state).
[<RequireQualifiedAccess>]
module StrideInterop =

    /// Convert a BepuFSharp PhysicsShape to the corresponding Stride.BepuPhysics collider.
    /// Raises for unsupported shape types (Compound, Mesh require registered shapes).
    val toStrideCollider: PhysicsShape -> ColliderBase

    /// Convert a Stride.BepuPhysics collider to a BepuFSharp PhysicsShape.
    /// Raises for unrecognized collider types.
    val fromStrideCollider: ColliderBase -> PhysicsShape

    /// Convert a BepuFSharp CollisionFilter group to a Stride CollisionLayer enum value.
    val toStrideCollisionLayer: CollisionFilter -> Stride.BepuPhysics.CollisionLayer

    /// Convert a Stride CollisionLayer to a BepuFSharp CollisionFilter.
    val fromStrideCollisionLayer: Stride.BepuPhysics.CollisionLayer -> CollisionFilter

    /// Write BepuFSharp MaterialProperties onto a Stride CollidableComponent.
    /// Sets SpringFrequency, SpringDampingRatio, MaximumRecoveryVelocity.
    /// Friction has no Stride CollidableComponent equivalent and is not written.
    val applyMaterialToComponent: MaterialProperties -> Stride.BepuPhysics.CollidableComponent -> unit

    /// Read material properties from a Stride CollidableComponent.
    /// Friction defaults to 1.0 since Stride does not expose per-component friction.
    val readMaterialFromComponent: Stride.BepuPhysics.CollidableComponent -> MaterialProperties
