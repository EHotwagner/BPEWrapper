---
title: "ADR-003: Opaque Typed Handles"
category: Architecture Decisions
categoryindex: 2
index: 3
---

# ADR-003: Opaque Typed Handles

## Status

Accepted

## Context

BepuPhysics2 uses separate handle types (`BodyHandle`, `StaticHandle`, `ConstraintHandle`) that
are all thin wrappers around `int`. In C#, it's easy to accidentally pass a `BodyHandle` where
a `StaticHandle` is expected since both expose `.Value` as an int.

## Decision

The wrapper defines opaque typed identifiers as single-case struct discriminated unions:

```fsharp
[<Struct>] type BodyId = BodyId of int
[<Struct>] type StaticId = StaticId of int
[<Struct>] type ConstraintId = ConstraintId of int

[<Struct>]
type ShapeId = { TypeId: int; Index: int }
```

These types:
- Are zero-cost at runtime (struct, single int field)
- Prevent cross-type handle confusion at compile time
- Pattern-match for extraction: `let (BodyId raw) = bodyId`

Internal conversion functions in `Interop.fs` convert between wrapper types and BepuPhysics2
handles using `[<MethodImpl(MethodImplOptions.AggressiveInlining)>]`.

## Consequences

- Compile-time safety prevents a class of handle-misuse bugs
- Zero runtime overhead (struct DU with single field is optimized by the JIT)
- Users cannot accidentally pass a BodyId where a StaticId is expected
- The `ShapeId` uses a struct record instead of a DU because it carries two fields (type + index)
- Internal code must explicitly convert at the boundary via Interop functions
