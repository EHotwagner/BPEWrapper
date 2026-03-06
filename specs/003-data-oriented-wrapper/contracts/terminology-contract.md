# Terminology Contract: ECS-Neutral Documentation

**Feature**: `003-data-oriented-wrapper`

## Purpose

This contract defines the terminology rules for all public-facing documentation in BepuFSharp. It ensures consistent, architecture-neutral language across README, API doc comments, literate scripts, tutorials, and example scripts.

## Rules

### MUST NOT (prerequisite/assumption context)

The following patterns MUST NOT appear in public-facing text where they imply ECS is required:

- "ECS sync" (use "game loop sync")
- "ECS integration" (use "game loop integration")
- "ECS operations" (use "game loop operations")
- "ECS transforms" (use "engine transforms")
- "ECS component storage" (use "component storage")
- "for ECS" as a purpose statement (use "for game loop" or "for data-oriented engines")

### MAY (comparative/clarifying context)

The term "ECS" MAY appear when:

- Defining what ECS is (educational glossary entries, "What Is ECS?" sections)
- Comparing architectures ("works with ECS and non-ECS engines alike")
- Acknowledging ECS as one supported pattern ("whether ECS-based or not")

### File naming

- No file in `docs/` or `scripts/examples/` should have "ecs" in its filename
- Use "game-loop" as the neutral equivalent in filenames

## Validation

Run the following to verify compliance:

```bash
# Should return zero results outside of specs/, .specify/, and comparative contexts
grep -rn "ECS" README.md BepuFSharp/*.fsi docs/ scripts/examples/ \
  | grep -v "specs/" | grep -v ".specify/" \
  | grep -v "What Is ECS" | grep -v "glossary"
```
