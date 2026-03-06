# Feature Specification: Data-Oriented Wrapper API & README Attribution

**Feature Branch**: `003-data-oriented-wrapper`
**Created**: 2026-03-06
**Status**: Draft
**Input**: User description: "The game engine wrapper (BPEWrapper) is for BepuPhysics2, a data-oriented physics engine that is NOT an ECS. The project wraps BepuPhysics2 with idiomatic F# APIs. Also add a section at the beginning of the README that the project was created using speckit using Claude Code with Opus 4.6, with links to speckit, the constitution and doc skills."

## Clarifications

### Session 2026-03-06

- Q: Does this feature include code changes to the bulk API, or is it strictly documentation and terminology updates? → A: Docs only — update README, doc comments, and terminology; validate (but don't change) that bulk API already works with plain arrays.
- Q: What should the "documentation skills" link in the attribution section point to? → A: Link to the constitution file, anchored to the "Comprehensive Documentation" section.
- Q: What canonical terms should replace ECS-specific phrases like "Bulk ECS sync" and "ECS integration"? → A: "game loop sync" and "game loop integration".

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Data-Oriented Integration Without ECS (Priority: P1)

As a game developer using a data-oriented engine that does not follow the Entity-Component-System pattern, I want the BepuFSharp wrapper to provide bulk physics operations (pose/velocity read/write, contact event queries) that work with plain arrays and struct layouts, so I can integrate physics into my engine without being forced into ECS abstractions.

**Why this priority**: The wrapper's primary value proposition is making BepuPhysics2 ergonomic in F#. If the API assumes ECS patterns, developers using data-oriented-but-not-ECS engines face unnecessary friction. Ensuring the API is ECS-agnostic removes the largest adoption barrier for the target audience.

**Independent Test**: Can be fully tested by creating a physics world, adding multiple bodies, stepping the simulation, and performing bulk reads/writes using plain arrays without referencing any ECS framework. Delivers value by proving the API stands alone as a data-oriented interface.

**Acceptance Scenarios**:

1. **Given** a physics world with 100 dynamic bodies, **When** the developer calls bulk pose read with a pre-allocated array, **Then** all poses are written to the array without requiring any ECS type or adapter.
2. **Given** a physics world with active contacts, **When** the developer queries contact events, **Then** events are returned as a flat array of value types usable in any engine architecture.
3. **Given** existing API documentation and README, **When** a developer reads them, **Then** no language implies ECS is required — terms like "ECS sync" are replaced with canonical architecture-neutral phrasing ("game loop sync", "game loop integration").

---

### User Story 2 - README Speckit Attribution (Priority: P2)

As a visitor to the BPEWrapper repository, I want to see at the top of the README that the project was created using speckit with Claude Code (Opus 4.6), with links to the speckit repository, the project constitution, and the documentation skills, so I can understand the development methodology and tooling used.

**Why this priority**: Attribution is important for transparency and for showcasing the speckit workflow, but it does not affect the library's functionality. It is a documentation-only change with no code impact.

**Independent Test**: Can be fully tested by reading the README and verifying the attribution section appears before the existing content, contains the correct tool name and model version, and includes working links.

**Acceptance Scenarios**:

1. **Given** the current README.md, **When** the attribution section is added, **Then** it appears as the first section after the H1 title, before the "Quick Start" section.
2. **Given** the attribution section, **When** a reader views it, **Then** it contains: the text "Created using speckit", the text "Claude Code with Opus 4.6", a link to the speckit repository, a link to the project constitution file, and a link to the documentation skills.
3. **Given** the attribution section links, **When** a reader clicks each link, **Then** each link resolves to the correct target (relative paths for in-repo files, absolute URLs for external resources).

---

### User Story 3 - ECS-Neutral API Naming (Priority: P2)

As a library maintainer, I want all public API names, documentation, and README content to use architecture-neutral terminology instead of ECS-specific terms, so that the wrapper accurately represents its compatibility with any data-oriented engine, not just ECS engines.

**Why this priority**: The current README uses "ECS" in several places (e.g., "Bulk ECS sync", "ECS integration"). This creates a misleading impression that the wrapper is ECS-specific. Correcting terminology aligns documentation with the library's actual design: it works with any struct-based, array-oriented engine architecture.

**Independent Test**: Can be fully tested by searching all public-facing text (README, XML doc comments, .fsi signatures) for the term "ECS" and verifying it either does not appear or is used only in a clarifying context (e.g., "works with ECS and non-ECS engines alike").

**Acceptance Scenarios**:

1. **Given** the current README.md, **When** all ECS-specific terms are audited, **Then** each instance is replaced with architecture-neutral phrasing that preserves meaning.
2. **Given** the public `.fsi` signature files, **When** searched for ECS-specific terminology, **Then** no function name, parameter name, or doc comment implies ECS is required.

---

### Edge Cases

- What happens when a developer searches the README or docs for "ECS"? The term should still appear in comparative context (e.g., "works with ECS and non-ECS engines") so that ECS users can discover the library, but it must not imply ECS is a prerequisite.
- What happens when external documentation or blog posts reference old ECS-specific function names? Public API function names that change must be documented in a migration note or changelog entry.
- What if speckit repository URLs change? Attribution links should use stable URLs; relative paths for in-repo files, canonical GitHub URLs for external tools.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The README MUST contain an attribution section crediting speckit and Claude Code (Opus 4.6) as the first content section after the H1 heading.
- **FR-002**: The attribution section MUST include a link to the speckit repository.
- **FR-003**: The attribution section MUST include a link to the project constitution file (`.specify/memory/constitution.md`).
- **FR-004**: The attribution section MUST include a link to the documentation skills by linking to the constitution file anchored to the "Comprehensive Documentation" section (Principle VI).
- **FR-005**: All public-facing text (README, doc comments, API names) MUST use architecture-neutral terminology instead of ECS-specific terms where the wrapper's functionality is being described.
- **FR-006**: The term "ECS" MAY appear in comparative or clarifying contexts (e.g., "supports both ECS and non-ECS architectures") but MUST NOT appear as a requirement or assumption.
- **FR-007**: Bulk operations (pose read/write, velocity read/write) MUST be validated to confirm they already accept plain arrays without requiring any ECS-specific types or adapters. No code changes are in scope; this is a verification-only requirement.
- **FR-008**: The README summary table MUST be updated to replace ECS-specific descriptions with architecture-neutral equivalents.

### Key Entities

- **Attribution Section**: A new README section containing tool/methodology credits with external and internal links.
- **Bulk Operations API**: The set of functions (`readPoses`, `writePoses`, `readVelocities`, `writeVelocities`) that transfer data between the physics world and user-owned arrays. These must remain ECS-agnostic.
- **Terminology Map**: A mapping of ECS-specific terms to their canonical architecture-neutral replacements: "ECS sync" -> "game loop sync", "ECS integration" -> "game loop integration", "Bulk ECS sync" -> "Bulk game loop sync".

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Zero instances of "ECS" appear in the README or public API documentation as a prerequisite or assumed architecture — verified by text search.
- **SC-002**: The README attribution section is present and contains all required links (speckit, constitution, doc skills) — verified by visual inspection and link validation.
- **SC-003**: All bulk operation functions accept plain F# arrays without requiring ECS framework types — verified by compiling and running existing tests after terminology changes.
- **SC-004**: Existing tests continue to pass with no functional regressions after all changes — verified by `dotnet test` returning 0 failures.

## Assumptions

- The speckit repository URL is `https://github.com/github/spec-kit`.
- The documentation skills are referenced via the `.specify/` directory structure within the repo (relative links).
- This feature is documentation-only in scope. No public API function signatures or code will change. If any function names are found to contain "ECS", that would be a separate feature requiring its own spec with migration planning.
- The bulk operations API already accepts plain arrays without ECS types; this feature validates that assumption but does not modify the code.
- The project constitution at `.specify/memory/constitution.md` is the correct file to link for the constitution reference.
