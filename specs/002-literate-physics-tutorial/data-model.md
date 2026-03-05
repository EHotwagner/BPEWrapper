# Data Model: Literate Physics Tutorial Book

**Feature**: 002-literate-physics-tutorial
**Date**: 2026-03-05

## Entities

This feature produces documentation content, not runtime data structures. The "data model" describes the content entities and their structure.

### Chapter

A single literate script file representing one tutorial topic.

| Attribute | Description |
|-----------|-------------|
| SequenceNumber | Two-digit number (01-12) determining reading and navigation order |
| Title | Human-readable chapter title shown in navigation and page header |
| Category | Always "Tutorial" (categoryindex: 3) |
| CategoryIndex | Always 3 |
| Index | Matches SequenceNumber (1-12) for navigation ordering |
| Prerequisites | List of prior chapter numbers the reader should have completed |
| FileFormat | `.fsx` for chapters 01-11 (literate F# script), `.md` for chapter 12 (glossary) |
| FilePath | `docs/tutorial/{SequenceNumber}-{slug}.fsx` |

### Chapter Internal Structure

Each `.fsx` chapter follows this structure:

```
1. YAML frontmatter (title, category, categoryindex, index)
2. Prerequisites statement (prose block)
3. NuGet #r directives (visible) + DLL #r directive (hidden)
4. open statements
5. Concept introduction (prose — plain language, real-world analogy)
6. ASCII diagram (where spatial concepts need illustration)
7. Code example with inline prose explaining each step
8. Output display using (*** include-value: ... ***) directives
9. Additional concept + code pairs as needed
10. Experiment section (prose describing what reader should modify and try)
11. PhysicsWorld.destroy cleanup
12. Summary and link to next chapter (prose)
```

### Glossary Entry

A single term definition within the glossary chapter.

| Attribute | Description |
|-----------|-------------|
| Term | The physics term being defined |
| Definition | Plain-language explanation |
| FirstAppears | Chapter number and title where the term is first introduced |

### File Inventory

| File | Type | Category |
|------|------|----------|
| `docs/tutorial/01-what-is-physics.fsx` | Literate F# | Tutorial |
| `docs/tutorial/02-shapes.fsx` | Literate F# | Tutorial |
| `docs/tutorial/03-bodies.fsx` | Literate F# | Tutorial |
| `docs/tutorial/04-simulation-loop.fsx` | Literate F# | Tutorial |
| `docs/tutorial/05-collisions.fsx` | Literate F# | Tutorial |
| `docs/tutorial/06-materials.fsx` | Literate F# | Tutorial |
| `docs/tutorial/07-constraints.fsx` | Literate F# | Tutorial |
| `docs/tutorial/08-collision-filtering.fsx` | Literate F# | Tutorial |
| `docs/tutorial/09-raycasting.fsx` | Literate F# | Tutorial |
| `docs/tutorial/10-bulk-operations.fsx` | Literate F# | Tutorial |
| `docs/tutorial/11-capstone.fsx` | Literate F# | Tutorial |
| `docs/tutorial/12-glossary.md` | Markdown | Tutorial |

### Relationships

- Each **Chapter** (01-11) references the **BepuFSharp API** elements listed in research.md R-005.
- The **Glossary** (12) references all other chapters via "First appears in" back-links.
- **Chapter 01** includes the F# syntax primer (FR-012) — no separate file needed.
- The **Tutorial category** is independent of the existing **Guides** and **Architecture Decisions** categories.

### State Transitions

Not applicable — all content is static documentation with no runtime state.
