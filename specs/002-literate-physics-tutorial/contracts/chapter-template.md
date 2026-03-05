# Contract: Tutorial Chapter Template

**Feature**: 002-literate-physics-tutorial
**Date**: 2026-03-05

## Purpose

Defines the structural contract that every tutorial chapter (01-11) MUST conform to. This ensures consistency across all chapters and serves as the authoring guide.

## YAML Frontmatter Contract

Every `.fsx` chapter MUST begin with:

```fsharp
(**
---
title: {Chapter Title}
category: Tutorial
categoryindex: 3
index: {SequenceNumber (1-11)}
---
*)
```

## Required Sections (in order)

### 1. Prerequisites Block

```fsharp
(**
# {Chapter Title}

**Prerequisites**: {Chapters N, M} or "None — this is the starting point."

{One-sentence summary of what this chapter covers.}
*)
```

### 2. NuGet and DLL References

```fsharp
#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp
```

### 3. Concept Introduction (prose)

Plain-language explanation of the physics concept. MUST include:
- Real-world analogy before formal terminology (FR-008)
- ASCII diagram if spatial concept involved (FR-011)
- No code in this section

### 4. Code Examples (interleaved prose + code)

One or more prose-then-code pairs. Each code block MUST:
- Create its own fresh `PhysicsWorld` (FR-004, first occurrence)
- Be executable in isolation
- Use only BepuFSharp wrapper API (FR-007)
- Include `(*** include-value: ... ***)` to show output where useful

### 5. Experiment Section

```fsharp
(**
## Experiment

{Description of what the reader should modify and what to observe.}
{At least one concrete suggestion per FR-010.}
*)
```

### 6. Cleanup and Summary

```fsharp
PhysicsWorld.destroy world

(**
## Summary

{2-3 sentences recapping what was learned.}

**Next**: [{Next Chapter Title}]({next-chapter-slug}.html)
*)
```

## Glossary Chapter Contract (chapter 12 only)

The glossary is a `.md` file (not `.fsx`). Structure:

```markdown
---
title: Glossary
category: Tutorial
categoryindex: 3
index: 12
---

# Glossary

| Term | Definition | First Appears |
|------|-----------|---------------|
| {Term} | {Plain-language definition} | [Chapter N: {Title}]({slug}.html) |
```

Terms MUST be sorted alphabetically. Every physics term introduced in chapters 1-11 MUST have an entry.
