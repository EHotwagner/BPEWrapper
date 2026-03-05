# Research: Literate Physics Tutorial Book

**Feature**: 002-literate-physics-tutorial
**Date**: 2026-03-05

## R-001: FSharp.Formatting Category & Navigation System

**Decision**: Use `categoryindex: 3` for the "Tutorial" category, with `index` values 1-12 for chapter ordering.

**Rationale**: The existing site uses categoryindex 0 (Overview), 1 (Guides), 2 (Architecture Decisions). Using 3 places the tutorial after reference material but is easily discoverable. FSharp.Formatting automatically discovers `.fsx` and `.md` files in subdirectories of `docs/` and uses YAML frontmatter for ordering.

**Alternatives considered**:
- categoryindex 1 (between Overview and Guides) — rejected because it would renumber existing guides.
- Separate site — rejected because FSharp.Formatting handles multi-category navigation natively.

## R-002: File Location and DLL Reference Pattern

**Decision**: Place tutorial files under `docs/tutorial/` as `NN-topic-name.fsx`. Each file uses the existing reference pattern:

```fsharp
#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)
```

**Rationale**: Matches the convention established by `getting-started.fsx` and `ecs-integration.fsx`. The path is `../../` instead of `../` because tutorial files are one level deeper (`docs/tutorial/`). The `(*** hide ***)` directive keeps the DLL path invisible in rendered output.

**Alternatives considered**:
- Using NuGet package reference for BepuFSharp — rejected because the library is not published; local DLL reference is the established pattern.
- Flat files in `docs/` without subdirectory — rejected because 12 tutorial files would clutter the root docs folder.

## R-003: Chapter Self-Containment Strategy

**Decision**: Each chapter creates its own `PhysicsWorld`, performs its demonstrations, and disposes at the end. Setup boilerplate is kept to 5-10 lines with a brief comment like "Set up a world as we learned in Chapter 1."

**Rationale**: Per clarification session, chapters must be independently compilable and testable. FSharp.Formatting evaluates each `.fsx` in isolation during `dotnet fsdocs build --eval`. Shared state across files is not supported.

**Alternatives considered**:
- `#load` shared setup script — rejected because it adds a hidden dependency and complicates the build.
- Cumulative state — rejected because FSharp.Formatting cannot chain script execution.

## R-004: ASCII Diagram Approach

**Decision**: Use ASCII art within FSharp.Formatting prose blocks (inside `(**` `*)` delimiters) rendered as preformatted text via markdown code fences.

**Rationale**: FSharp.Formatting renders prose blocks as markdown. Code-fenced blocks within prose render as monospace preformatted text, preserving ASCII diagram alignment. No images or external tooling required.

**Alternatives considered**:
- SVG/PNG images — rejected per clarification; ASCII is lower maintenance and version-control friendly.
- Mermaid diagrams — rejected; not supported by FSharp.Formatting's default template.

## R-005: BepuFSharp API Coverage per Chapter

**Decision**: Map API surface to chapters as follows:

| Chapter | Key API Elements |
|---------|-----------------|
| 01 Introduction | PhysicsWorld.create, PhysicsConfig.defaults, PhysicsWorld.step, PhysicsWorld.destroy |
| 02 Shapes | PhysicsShape (all 8 variants), PhysicsWorld.addShape, PhysicsWorld.removeShape |
| 03 Bodies | DynamicBodyDesc, KinematicBodyDesc, StaticBodyDesc, PhysicsWorld.addBody/addStatic/addKinematicBody |
| 04 Simulation Loop | PhysicsWorld.step, PhysicsWorld.getBodyPose, PhysicsWorld.getBodyVelocity, Pose, Velocity |
| 05 Collisions | PhysicsWorld.getContactEvents, ContactEvent, ContactEventType |
| 06 Materials | MaterialProperties, DynamicBodyDesc with custom material, friction/spring config |
| 07 Constraints | ConstraintDesc (all 10 variants), SpringConfig, MotorSettings, PhysicsWorld.addConstraint/removeConstraint |
| 08 Filtering | CollisionFilter, CollisionGroup/CollisionMask fields on body descriptors |
| 09 Raycasting | PhysicsWorld.raycast, PhysicsWorld.raycastAll, RayHit |
| 10 Bulk Ops | PhysicsWorld.readPoses/writePoses/readVelocities/writeVelocities |
| 11 Capstone | All of the above combined |
| 12 Glossary | Reference only (no code) |

**Rationale**: Progressive API introduction mirrors the spec's topic order. Each chapter adds 2-5 new API elements, keeping cognitive load manageable.

## R-006: Glossary Chapter Format

**Decision**: The glossary chapter will be a `.md` file (not `.fsx`) since it contains no executable code — only term definitions with back-references.

**Rationale**: FSharp.Formatting supports both `.fsx` and `.md` files in the docs tree. A pure-markdown glossary avoids unnecessary script boilerplate for a reference-only page.

**Alternatives considered**:
- `.fsx` with only prose blocks — works but adds unnecessary `#r` directives and compilation overhead for zero code content.

## R-007: Documentation Build Verification

**Decision**: The existing GitHub Actions workflow (`docs.yml`) triggers on changes to `docs/**`. Adding files under `docs/tutorial/` will automatically trigger doc rebuilds. No workflow changes needed.

**Rationale**: The glob `docs/**` already covers subdirectories. The build command `dotnet fsdocs build --properties Configuration=Release` will discover and evaluate all new `.fsx` files.

**Alternatives considered**:
- Separate CI job for tutorial — rejected; unnecessary complexity when the existing workflow handles it.

## R-008: FSharp.Formatting Tool Version

**Decision**: Use fsdocs-tool v21.0.0 (already installed via dotnet-tools.json).

**Rationale**: No version change needed. v21.0.0 supports all required features: YAML frontmatter, subdirectory discovery, literate script evaluation, markdown rendering within prose blocks.
