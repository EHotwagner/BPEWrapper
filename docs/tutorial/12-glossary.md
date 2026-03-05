---
title: Glossary
category: Tutorial
categoryindex: 3
index: 12
---

# Glossary

A quick-reference of every physics and programming term introduced in this tutorial, sorted alphabetically.

| Term | Definition | First Appears |
|------|-----------|---------------|
| **Angular velocity** | Rate of rotation in radians per second around each axis. | [Ch 4](04-simulation-loop.html) |
| **BallSocket** | A constraint that keeps two attachment points locked together while allowing free rotation in all directions. Like a shoulder joint. | [Ch 7](07-constraints.html) |
| **Body** | An object in the physics world with position, orientation, mass, and velocity. Can be dynamic, kinematic, or static. | [Ch 3](03-bodies.html) |
| **Box** | A rectangular shape defined by width (X), height (Y), and length (Z) as full extents, not half-extents. | [Ch 2](02-shapes.html) |
| **Broadphase** | The first stage of collision detection that uses bounding boxes to quickly eliminate non-colliding pairs. | [Ch 4](04-simulation-loop.html) |
| **Capsule** | A cylinder with hemispherical end caps, defined by radius and length. Common for character controllers. | [Ch 2](02-shapes.html) |
| **Collision filter** | A group/mask pair that controls which body pairs generate contacts. Uses bitwise AND logic. | [Ch 8](08-collision-filtering.html) |
| **Collision group** | A bit flag identifying which layer a body belongs to (e.g., Player=1, Enemy=2). | [Ch 8](08-collision-filtering.html) |
| **Collision mask** | A bitmask saying which groups a body can collide with. | [Ch 8](08-collision-filtering.html) |
| **Compound shape** | Multiple shapes combined into one, each with a local offset from the compound origin. | [Ch 2](02-shapes.html) |
| **Constraint** | An invisible rule connecting two bodies that the solver enforces each frame (joints, springs, motors). | [Ch 7](07-constraints.html) |
| **Contact event** | A notification that two objects are touching, with fields for the bodies involved, normal, depth, and lifecycle type. | [Ch 5](05-collisions.html) |
| **Contact normal** | A unit vector pointing in the direction to push overlapping objects apart. | [Ch 5](05-collisions.html) |
| **Convex hull** | A shape computed from a cloud of points as the tightest convex boundary containing all of them. Needs at least 4 non-coplanar points. | [Ch 2](02-shapes.html) |
| **Cylinder** | A flat-ended cylindrical shape defined by radius and length. Good for wheels and barrels. | [Ch 2](02-shapes.html) |
| **Damping ratio** | Controls how quickly spring oscillations stop. 1.0 = critically damped (no bouncing), below 1.0 = oscillation. | [Ch 7](07-constraints.html) |
| **Depth** | The penetration distance between two overlapping objects at a contact point. | [Ch 5](05-collisions.html) |
| **Deterministic** | A simulation mode where identical inputs always produce identical outputs. Enabled via `PhysicsConfig.Deterministic`. | [Ch 1](01-what-is-physics.html) |
| **Discriminated union** | An F# type that can be one of several named cases, each optionally carrying data. Used for `PhysicsShape`, `ConstraintDesc`, etc. | [Ch 1](01-what-is-physics.html) |
| **DistanceLimit** | A constraint that enforces a minimum and maximum distance between two attachment points. Like a chain link. | [Ch 7](07-constraints.html) |
| **DistanceSpring** | A constraint that tries to maintain a target distance with spring behavior. Like a bungee cord. | [Ch 7](07-constraints.html) |
| **Dynamic body** | A body affected by gravity and collisions with finite mass. Examples: thrown ball, ragdoll, debris. | [Ch 3](03-bodies.html) |
| **ECS** | Entity-Component-System architecture. Entities are IDs, components are data structs in flat arrays, systems process batches. | [Ch 10](10-bulk-operations.html) |
| **Friction** | A material property controlling how much objects resist sliding against each other. 0 = ice, 1+ = sandpaper. | [Ch 6](06-materials.html) |
| **Gravity** | A constant acceleration applied to all dynamic bodies each step, typically (0, -9.81, 0) for Earth-like conditions. | [Ch 1](01-what-is-physics.html) |
| **Hinge** | A constraint that allows rotation around a single axis only. Like a door hinge. | [Ch 7](07-constraints.html) |
| **Inertia** | A body's resistance to rotational acceleration, computed automatically from shape and mass. | [Ch 3](03-bodies.html) |
| **Integration** | The simulation stage that applies velocities to update positions, moving everything forward in time. | [Ch 4](04-simulation-loop.html) |
| **Joint** | Common name for a constraint connecting two bodies. See: BallSocket, Hinge, Weld, etc. | [Ch 7](07-constraints.html) |
| **Kinematic body** | A body moved by setting velocity directly. Not affected by gravity. Pushes dynamic objects but is not pushed back. Example: moving platform. | [Ch 3](03-bodies.html) |
| **Let binding** | F# syntax for giving a name to a value: `let x = 5`. Similar to `const` in other languages. | [Ch 1](01-what-is-physics.html) |
| **Linear velocity** | Rate of straight-line motion in units per second along each axis. | [Ch 4](04-simulation-loop.html) |
| **Mass** | How heavy a body is, in kilograms. Affects how collisions and constraints respond. All masses fall at the same rate under gravity. | [Ch 3](03-bodies.html) |
| **Material properties** | Surface characteristics controlling contact response: friction, bounciness, spring stiffness. | [Ch 6](06-materials.html) |
| **MaxRecoveryVelocity** | Material property capping how fast objects bounce apart after collision. Higher = bouncier. | [Ch 6](06-materials.html) |
| **Mesh** | A shape made of triangles for complex static geometry like terrain. | [Ch 2](02-shapes.html) |
| **Narrowphase** | The second stage of collision detection that runs exact geometry tests on candidate pairs from broadphase. | [Ch 4](04-simulation-loop.html) |
| **Orientation** | A body's rotation in world space, stored as a quaternion. | [Ch 3](03-bodies.html) |
| **Physics engine** | Software that simulates forces, collisions, and motion so games do not have to compute them manually. | [Ch 1](01-what-is-physics.html) |
| **Physics world** | The central object holding the simulation, memory pools, and threading. Created with `PhysicsWorld.create`. | [Ch 1](01-what-is-physics.html) |
| **Pipe operator** | F# operator `\|>` that passes a value into the next function, enabling left-to-right reading. | [Ch 1](01-what-is-physics.html) |
| **Pose** | A body's position and orientation in world space. A struct with `Position: Vector3` and `Orientation: Quaternion`. | [Ch 3](03-bodies.html) |
| **Quaternion** | A four-component value representing 3D rotation without gimbal lock. Used for `Pose.Orientation`. | [Ch 3](03-bodies.html) |
| **Raycast** | Firing an invisible ray through the world to find what it hits. Returns hit position, normal, distance, and body/static ID. | [Ch 9](09-raycasting.html) |
| **RayHit** | The result of a raycast: which body/static was hit, where, at what angle, and how far along the ray. | [Ch 9](09-raycasting.html) |
| **Record** | An F# data type with named fields: `type Point = { X: float; Y: float }`. | [Ch 1](01-what-is-physics.html) |
| **Restitution** | How much kinetic energy is preserved after a collision. In BepuFSharp, controlled via `MaxRecoveryVelocity`. | [Ch 6](06-materials.html) |
| **Rigid body** | A body that does not deform — its shape stays constant. All bodies in BepuPhysics2 are rigid. | [Ch 3](03-bodies.html) |
| **Shape** | A collision boundary describing an object's geometry (Sphere, Box, Capsule, etc.). Registered with `addShape`. | [Ch 2](02-shapes.html) |
| **ShapeId** | An opaque identifier returned by `addShape`, composed of a type discriminator and index. | [Ch 2](02-shapes.html) |
| **Simulation step** | One tick of the physics engine: broadphase, narrowphase, solver, integration. Triggered by `PhysicsWorld.step`. | [Ch 4](04-simulation-loop.html) |
| **Solver** | The simulation stage that resolves collisions and constraints by computing forces to push objects apart and enforce joint rules. | [Ch 4](04-simulation-loop.html) |
| **Sphere** | The simplest shape, defined by a single radius. Cheapest for collision detection. | [Ch 2](02-shapes.html) |
| **Spring config** | Controls constraint stiffness via frequency (Hz) and damping ratio. | [Ch 7](07-constraints.html) |
| **Spring damping ratio** | Material property controlling how quickly contact oscillations settle. 1.0 = critically damped. | [Ch 6](06-materials.html) |
| **Spring frequency** | How stiff a spring connection is, in Hz. Higher = more rigid. Used in both constraints and material contacts. | [Ch 6](06-materials.html) |
| **Static body** | A body that never moves. Has infinite mass. Used for floors, walls, terrain. Cheapest to simulate. | [Ch 3](03-bodies.html) |
| **Substep** | A mini-step within a single simulation step. More substeps = better accuracy for fast objects, at higher cost. | [Ch 4](04-simulation-loop.html) |
| **Timestep** | The time delta (dt) in seconds passed to `PhysicsWorld.step`. Common values: 1/60 (60 Hz), 1/120 (120 Hz). | [Ch 4](04-simulation-loop.html) |
| **Triangle** | A single-triangle shape defined by three vertices. Building block for meshes. | [Ch 2](02-shapes.html) |
| **Velocity** | A struct with `Linear: Vector3` (straight-line motion) and `Angular: Vector3` (rotation). | [Ch 4](04-simulation-loop.html) |
| **Weld** | A constraint that locks two bodies so they move as one rigid unit. Like glue. | [Ch 7](07-constraints.html) |
