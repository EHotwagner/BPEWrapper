# Feature Specification: Literate Physics Tutorial Book

**Feature Branch**: `002-literate-physics-tutorial`
**Created**: 2026-03-05
**Status**: Draft
**Input**: User description: "Add a tutorial book using literate programming using this wrapper to explain the workings of bepuphysics and 3d physics engines in general. Do not assume expert knowledge. Start small and grow to more complex topics."

## Clarifications

### Session 2026-03-05

- Q: Should the tutorial replace, link to, or coexist with the existing getting-started and ecs-integration guides? → A: Coexist as a separate learning path under its own documentation category. The existing guides are API-oriented references; the tutorial is concept-oriented pedagogy serving a different audience.
- Q: Should later chapters build on state from earlier chapters, or should each chapter create its own fresh world? → A: Each chapter creates its own fresh world from scratch. This ensures every script is independently compilable and testable during the doc build. Repeated setup is kept minimal with brief references to earlier chapters.
- Q: Should chapters include visual aids to illustrate spatial concepts? → A: Use ASCII/text diagrams embedded in prose sections (e.g., coordinate axes, box layouts, joint attachment points). These are low-effort, version-controlled, and render natively in FSharp.Formatting.
- Q: Should the tutorial include a brief F# syntax primer for non-F# readers? → A: Yes, include a brief primer in Chapter 1 covering only the F# constructs used in the tutorial (pipes, records, DUs, let bindings), plus links to further F# learning materials for readers who want to go deeper.
- Q: Should the tutorial include a physics glossary? → A: Yes, include a standalone glossary chapter at the end that defines all physics terms introduced across the tutorial, with back-references to the chapter where each term first appears.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Beginner Learns Physics Fundamentals (Priority: P1)

A newcomer to game physics reads the first chapters of the tutorial and understands what a physics engine does, how objects are represented, what a simulation step is, and how gravity works — all through runnable F# code examples using BepuFSharp.

**Why this priority**: Without a solid conceptual foundation, later chapters become incomprehensible. This is the entry point for the entire tutorial and must stand alone as a useful resource.

**Independent Test**: A reader with no prior physics engine experience can follow chapters 1-3, run the scripts, and correctly predict what happens when a sphere is dropped onto a floor.

**Acceptance Scenarios**:

1. **Given** a reader with basic programming knowledge but no physics engine experience, **When** they read the introductory chapter, **Then** they understand what a physics engine does, why it exists, and what problems it solves.
2. **Given** the reader has completed the introduction, **When** they read the shapes chapter and then the bodies chapter, **Then** they can create different shapes, understand the difference between dynamic/static/kinematic bodies, and explain why each type exists.
3. **Given** the reader has completed the shapes chapter, **When** they read the simulation loop chapter, **Then** they can step a simulation, read back poses, and understand what a timestep means physically.

---

### User Story 2 - Intermediate Learner Explores Interactions (Priority: P2)

A reader who has completed the fundamentals moves on to chapters covering collisions, contact events, materials (friction/restitution), and constraints. They learn how objects interact with each other beyond just falling under gravity.

**Why this priority**: Object interaction is the core reason physics engines exist. Once readers understand individual objects, they need to understand how objects affect each other.

**Independent Test**: A reader who has completed the fundamentals chapters can follow the interaction chapters, build a simple scene with constrained objects and collision responses, and explain why objects behave the way they do.

**Acceptance Scenarios**:

1. **Given** a reader who understands bodies and stepping, **When** they read the collision and contacts chapter, **Then** they can set up contact event listeners and explain the began/persisted/ended lifecycle.
2. **Given** a reader who understands contacts, **When** they read the materials chapter, **Then** they can configure friction and spring properties and predict how a bouncy ball vs. a heavy crate behaves differently.
3. **Given** a reader who understands materials, **When** they read the constraints chapter, **Then** they can connect bodies with joints and describe real-world analogues for each constraint type.

---

### User Story 3 - Advanced Reader Tackles Queries and Performance (Priority: P3)

A reader who has mastered the fundamentals and interactions progresses to collision filtering, raycasting, bulk operations for ECS integration, and performance considerations. They learn how to query the physics world and how to use it efficiently in a real game engine.

**Why this priority**: These are essential for shipping a real game but require solid grounding in earlier material. They represent the bridge from learning to production use.

**Independent Test**: A reader can implement raycasts to detect line-of-sight, perform bulk pose reads for an ECS sync loop, and explain why bulk operations outperform per-body reads.

**Acceptance Scenarios**:

1. **Given** a reader who understands bodies and collisions, **When** they read the raycasting chapter, **Then** they can perform single-hit and multi-hit raycasts and use results for gameplay logic like line-of-sight checks.
2. **Given** a reader who understands raycasting, **When** they read the collision filtering chapter, **Then** they can set up layer-based filtering using group/mask fields and explain bitwise collision logic.
3. **Given** a reader who understands the full API, **When** they read the bulk operations and ECS chapter, **Then** they can use readPoses/writePoses for efficient data-oriented sync and explain why this pattern matters for performance.

---

### User Story 4 - Reader Builds a Capstone Project (Priority: P4)

In the final chapter, the reader combines everything learned into a complete physics scenario: a multi-body scene with different materials, constraints, contact events, raycasting, and collision filtering — demonstrating how all concepts work together.

**Why this priority**: A capstone ties all concepts together and gives readers confidence they can apply the knowledge independently. It is valuable but depends on all earlier content.

**Independent Test**: The capstone script runs successfully, produces physically plausible results, and references concepts from every earlier chapter.

**Acceptance Scenarios**:

1. **Given** a reader who has completed all prior chapters, **When** they read the capstone chapter, **Then** they can follow the construction of a complete physics scene and understand how each piece fits together.
2. **Given** the capstone script, **When** it is executed, **Then** it runs without errors and produces output that demonstrates gravity, collisions, constraints, materials, filtering, and raycasting.

---

### Edge Cases

- What happens when a reader skips ahead to a later chapter? Each chapter should state its prerequisites clearly.
- How does the tutorial handle API changes in future BepuFSharp versions? Code examples should be tested as part of the documentation build so breakages are caught automatically.
- What if a reader has no F# experience? Chapter 1 includes a brief F# syntax primer and links to further learning materials. The tutorial does not teach F# comprehensively.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The tutorial MUST be authored as literate F# scripts (`.fsx` files) using FSharp.Formatting, so that prose and executable code are interwoven in a single source file.
- **FR-002**: The tutorial MUST be organized into chapters that follow a progressive learning path, from foundational concepts to advanced topics.
- **FR-003**: Each chapter MUST begin with a plain-language explanation of the physics concept before introducing any code, so readers understand the "why" before the "how."
- **FR-004**: Every code example in the tutorial MUST be executable — the literate scripts must compile and run as part of the documentation build process. Each chapter MUST be self-contained: it creates its own fresh `PhysicsWorld` and does not depend on state from other chapters.
- **FR-005**: The tutorial MUST cover these topics in approximately this order:
  1. What is a physics engine and why you need one
  2. Shapes: how geometry is represented
  3. Bodies: dynamic, static, and kinematic
  4. The simulation loop: timesteps, stepping, reading results
  5. Collisions and contact events
  6. Materials: friction and restitution
  7. Constraints and joints
  8. Collision filtering with layers
  9. Raycasting and spatial queries
  10. Bulk operations and ECS integration
  11. Capstone: putting it all together
  12. Glossary of physics terms (reference chapter)
- **FR-006**: Each chapter MUST state its prerequisites (which prior chapters the reader should have completed).
- **FR-007**: The tutorial MUST use the BepuFSharp wrapper API exclusively (not raw BepuPhysics2 API) so that readers learn the idiomatic F# interface.
- **FR-008**: Physics concepts MUST be explained using real-world analogies and plain language before introducing formal terminology.
- **FR-011**: Chapters MUST use ASCII/text diagrams embedded in prose sections to illustrate spatial concepts (coordinate systems, object layouts, joint attachment points, ray directions) where text alone would be insufficient for a beginner.
- **FR-012**: Chapter 1 MUST include a brief F# syntax primer covering only the constructs used throughout the tutorial (pipe operator, records, discriminated unions, let bindings) followed by links to further F# learning materials.
- **FR-013**: The tutorial MUST include a glossary chapter at the end that defines all physics terms introduced across the tutorial, with back-references to the chapter where each term first appears.
- **FR-009**: The tutorial MUST integrate into the existing FSharp.Formatting documentation site under a dedicated "Tutorial" category, appearing in the site navigation separately from the existing Guides category.
- **FR-010**: Each chapter MUST include at least one "experiment" — a suggested modification the reader can try to deepen understanding (e.g., "Try changing the mass to 10 and observe the difference").

### Key Entities

- **Chapter**: A single `.fsx` literate script covering one topic. Has a sequence number, title, prerequisites, prose sections, code sections, and experiments.
- **Tutorial**: The ordered collection of all chapters forming the complete learning path. Published as a category within the documentation site.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The tutorial contains at least 11 content chapters covering the full topic progression from introduction to capstone, plus a glossary reference chapter.
- **SC-002**: Every literate script compiles and executes successfully during the documentation build, with zero failures.
- **SC-003**: A reader with basic programming knowledge (but no physics or F# engine experience) can complete the first three chapters and correctly answer: what a physics engine does, what the three body types are, and what happens during a simulation step.
- **SC-004**: Each chapter takes no longer than 20 minutes to read and work through, keeping content focused and digestible.
- **SC-005**: The tutorial appears in the documentation site navigation under its own category, with chapters listed in sequential order.
- **SC-006**: Every chapter includes at least one hands-on experiment that the reader can modify and re-run.

## Assumptions

- Readers have basic programming literacy (variables, loops, functions) but may not know F# specifically. Chapter 1 includes a brief F# syntax primer covering constructs used in the tutorial (pipe operator, records, discriminated unions, let bindings) plus links to further F# learning materials. The tutorial does not attempt to teach F# comprehensively.
- The existing FSharp.Formatting documentation pipeline (with `dotnet fsdocs build`) will be used to compile and publish the tutorial alongside existing docs.
- The BepuFSharp wrapper API is stable enough that tutorial content will not need frequent updates due to API changes.
- Chapters will be numbered to enforce reading order (e.g., `docs/tutorial/01-what-is-physics.fsx`, `docs/tutorial/02-shapes.fsx`).
- The tutorial focuses on understanding concepts and API usage, not on rendering or visual output. Results are shown as printed values and prose descriptions.
- The tutorial coexists alongside the existing getting-started and ecs-integration guides. The existing guides serve as API-oriented references while the tutorial provides concept-oriented pedagogy for a different audience.
