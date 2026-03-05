# Quickstart: Literate Physics Tutorial Book

**Feature**: 002-literate-physics-tutorial
**Date**: 2026-03-05

## What This Feature Delivers

A 12-chapter tutorial book authored as literate F# scripts, teaching 3D physics engine concepts from scratch using the BepuFSharp wrapper. Published as a "Tutorial" category on the existing FSharp.Formatting documentation site.

## File Locations

All tutorial content goes under `docs/tutorial/`:

```
docs/tutorial/
├── 01-what-is-physics.fsx
├── 02-shapes.fsx
├── 03-bodies.fsx
├── 04-simulation-loop.fsx
├── 05-collisions.fsx
├── 06-materials.fsx
├── 07-constraints.fsx
├── 08-collision-filtering.fsx
├── 09-raycasting.fsx
├── 10-bulk-operations.fsx
├── 11-capstone.fsx
└── 12-glossary.md
```

## How to Build and Preview

```bash
# Build the library first (required for DLL references)
dotnet build BepuFSharp/BepuFSharp.fsproj -c Release

# Build full docs site
dotnet fsdocs build --properties Configuration=Release

# Or live-preview with auto-reload
dotnet fsdocs watch --properties Configuration=Release
```

Output appears in `output/` — open `output/index.html` and navigate to the "Tutorial" category.

## Authoring a New Chapter

1. Copy the structure from `contracts/chapter-template.md`
2. Use `categoryindex: 3` and set `index` to the chapter number
3. Begin with a concept explanation using plain language and analogies
4. Add ASCII diagrams for spatial concepts
5. Interleave prose and code — create a fresh `PhysicsWorld` in each chapter
6. Include at least one experiment suggestion
7. Destroy the world at the end
8. Link to the next chapter in the summary

## Verifying Tutorial Content

```bash
# Full build verifies all .fsx scripts compile and evaluate
dotnet fsdocs build --properties Configuration=Release
```

Any broken script will cause the build to fail, catching regressions automatically.

## Key Constraints

- Each chapter is self-contained (no shared state between scripts)
- Use BepuFSharp wrapper API only — never raw BepuPhysics2 API
- DLL reference path from `docs/tutorial/` is `../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll`
- ASCII diagrams only (no image files)
- The glossary (chapter 12) is `.md`, not `.fsx`
