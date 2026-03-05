(**
---
title: Constraints and Joints
category: Tutorial
categoryindex: 3
index: 7
---
*)

(**
# Constraints and Joints

**Prerequisites**: [Chapter 1](01-what-is-physics.html),
[Chapter 3: Bodies](03-bodies.html),
[Chapter 4: Simulation Loop](04-simulation-loop.html)

This chapter explains how to connect bodies together using
**constraints**. Constraints are invisible rules that limit how bodies
can move relative to each other.

## What Is a Constraint?

Think of real-world joints:

- A **door hinge** allows rotation around one axis only.
- A **shoulder joint** allows rotation in all directions from a fixed point.
- **Glue** welds two pieces together rigidly.
- A **bungee cord** pulls two objects toward a target distance.

In a physics engine, a constraint is a mathematical rule the solver
enforces every frame. Each constraint connects exactly two bodies.

```text
  Body A            Constraint           Body B
  +------+     ~~~~[spring]~~~~     +------+
  |      |<========================>|      |
  |      |     attachment points    |      |
  +------+                          +------+
       ^                                ^
  localOffsetA                    localOffsetB
  (relative to A's center)        (relative to B's center)
```

The **local offsets** define where the constraint attaches on each body,
measured from the body's center.

## Spring Settings

Most constraints use a `SpringConfig` to control their behavior:

- **Frequency** (Hz): How stiff the connection is. Higher = more rigid.
  Think of it as spring tightness.
- **Damping Ratio**: How quickly oscillations stop. A value of 1.0 means
  critically damped (no bouncing). Below 1.0 means oscillation.

```text
  Damping Ratio = 0.3        Damping Ratio = 1.0
  (underdamped, bouncy)      (critically damped, smooth)

  Position                   Position
  |    /\                    |
  |   /  \   /\             |   ---___
  |  /    \_/  \_/\         |         ----___
  |_/              \_       |                ----___
  +-----> time              +-----> time
```

## Constraint Types

BepuFSharp provides these constraint types through `ConstraintDesc`:

| Type | Real-World Analogy | Use Case |
|------|--------------------|----------|
| BallSocket | Shoulder joint | Ragdolls, chains |
| Hinge | Door hinge | Doors, wheels on axle |
| Weld | Glued together | Rigid attachments |
| DistanceSpring | Bungee cord | Soft connections |
| DistanceLimit | Chain link | Min/max distance |
| SwingLimit | Cone-shaped limit | Joint range limits |
| TwistLimit | Twist range | Prevent over-rotation |
| LinearAxisMotor | Piston | Sliding platforms |
| AngularMotor | Electric motor | Spinning wheels |
| PointOnLine | Rail slider | Objects on tracks |

Let's demonstrate the four most common ones.
*)

#r "nuget: BepuPhysics, 2.4.0"
#r "nuget: BepuUtilities, 2.4.0"
(*** hide ***)
#r "../../BepuFSharp/bin/Release/net10.0/BepuFSharp.dll"
(*** show ***)

open System.Numerics
open BepuFSharp

let world = PhysicsWorld.create PhysicsConfig.defaults

let sphereShape = PhysicsWorld.addShape (PhysicsShape.Sphere 0.5f) world
let boxShape = PhysicsWorld.addShape (PhysicsShape.Box(1.0f, 1.0f, 1.0f)) world
let floorShape = PhysicsWorld.addShape (PhysicsShape.Box(20.0f, 1.0f, 20.0f)) world

let _floor =
    PhysicsWorld.addStatic
        (StaticBodyDesc.create floorShape (Pose.ofPosition (Vector3(0.0f, -0.5f, 0.0f))))
        world

(**
### BallSocket: A Shoulder Joint

A BallSocket keeps two points (one on each body) locked together while
allowing free rotation in all directions. Think of a ball-and-socket
joint in your shoulder.
*)

let anchor =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            boxShape
            (Pose.ofPosition (Vector3(0.0f, 8.0f, 0.0f)))
            100.0f) // Heavy anchor
        world

let pendulum =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(0.0f, 5.0f, 0.0f)))
            1.0f)
        world

// Connect the bottom of the anchor to the top of the pendulum
let spring = SpringConfig.create 30.0f 1.0f

let ballSocket =
    PhysicsWorld.addConstraint
        anchor
        pendulum
        (ConstraintDesc.BallSocket(
            Vector3(0.0f, -0.5f, 0.0f),  // bottom of anchor
            Vector3(0.0f, 0.5f, 0.0f),   // top of pendulum
            spring))
        world

(**
The pendulum ball is now attached to the anchor box. If we give the
pendulum a sideways push, it will swing:
*)

PhysicsWorld.setBodyVelocity
    pendulum
    (Velocity.create (Vector3(3.0f, 0.0f, 0.0f)) Vector3.Zero)
    world

for _ in 1..120 do
    PhysicsWorld.step (1.0f / 60.0f) world

let pendulumPose = PhysicsWorld.getBodyPose pendulum world

(*** include-value: pendulumPose ***)

(**
The pendulum should have swung to the side and back, ending up
somewhere along its arc.

### Hinge: A Door

A Hinge allows rotation around a single axis only. Perfect for doors,
lids, and wheels on axles.
*)

let doorFrame =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            boxShape
            (Pose.ofPosition (Vector3(5.0f, 4.0f, 0.0f)))
            100.0f)
        world

let doorPanelShape = PhysicsWorld.addShape (PhysicsShape.Box(2.0f, 3.0f, 0.2f)) world

let doorPanel =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            doorPanelShape
            (Pose.ofPosition (Vector3(6.0f, 4.0f, 0.0f)))
            5.0f)
        world

let _hinge =
    PhysicsWorld.addConstraint
        doorFrame
        doorPanel
        (ConstraintDesc.Hinge(
            Vector3.UnitY,                 // hinge axis on frame (vertical)
            Vector3.UnitY,                 // hinge axis on door (vertical)
            Vector3(0.5f, 0.0f, 0.0f),    // right edge of frame
            Vector3(-1.0f, 0.0f, 0.0f),   // left edge of door
            spring))
        world

(**
### Weld: Glued Together

A Weld locks two bodies so they move as one rigid unit. Useful for
attaching decorations or creating breakable connections (remove the
constraint to "break" the weld).
*)

let bodyA =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            boxShape
            (Pose.ofPosition (Vector3(-5.0f, 6.0f, 0.0f)))
            2.0f)
        world

let bodyB =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(-5.0f, 7.5f, 0.0f)))
            1.0f)
        world

let weld =
    PhysicsWorld.addConstraint
        bodyA
        bodyB
        (ConstraintDesc.Weld(
            Vector3(0.0f, 0.75f, 0.0f),  // top of box
            Quaternion.Identity,           // no relative rotation
            spring))
        world

(**
The sphere is now rigidly attached to the top of the box. They will
fall and tumble together as a single unit.

### DistanceSpring: A Bungee Cord

A DistanceSpring tries to maintain a target distance between two
attachment points, with springy behavior:
*)

let post =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            boxShape
            (Pose.ofPosition (Vector3(-10.0f, 8.0f, 0.0f)))
            100.0f)
        world

let hanging =
    PhysicsWorld.addBody
        (DynamicBodyDesc.create
            sphereShape
            (Pose.ofPosition (Vector3(-10.0f, 4.0f, 0.0f)))
            1.0f)
        world

// Soft spring (low frequency = stretchy)
let softSpring = SpringConfig.create 5.0f 0.3f

let _bungee =
    PhysicsWorld.addConstraint
        post
        hanging
        (ConstraintDesc.DistanceSpring(
            Vector3(0.0f, -0.5f, 0.0f),  // bottom of post
            Vector3(0.0f, 0.5f, 0.0f),   // top of hanging ball
            3.0f,                          // target distance
            softSpring))
        world

// Step to see it bounce
for _ in 1..180 do
    PhysicsWorld.step (1.0f / 60.0f) world

let hangingPose = PhysicsWorld.getBodyPose hanging world

(*** include-value: hangingPose ***)

(**
The hanging ball should oscillate up and down (low damping ratio of 0.3
means it bounces for a while before settling).

## Removing Constraints

When you no longer need a constraint, remove it to free resources:
*)

PhysicsWorld.removeConstraint ballSocket world
PhysicsWorld.removeConstraint weld world

(**
After removal, the previously-connected bodies move independently.
Note: removing a *body* automatically removes all its constraints too.

## Other Constraint Types

The remaining variants in `ConstraintDesc` serve specialized purposes:

- **DistanceLimit**: Like DistanceSpring but with min/max range
  (a chain link that can be slack but not stretch beyond a limit).
- **SwingLimit**: Limits the angle between two axes (cone-shaped range
  of motion, like a neck joint).
- **TwistLimit**: Limits rotation around a shared axis (prevents
  an arm from twisting more than 180 degrees).
- **LinearAxisMotor**: Applies force along an axis (a piston or
  sliding platform).
- **AngularMotor**: Applies torque to spin a body (an electric motor
  driving a wheel).
- **PointOnLine**: Constrains a point on body B to slide along a line
  defined on body A (a rail or track).
*)

PhysicsWorld.destroy world

(**
## Experiment

Try these modifications:

1. Change the DistanceSpring frequency from `5.0f` to `30.0f` — the
   connection becomes much stiffer (less bouncy).
2. Set the damping ratio to `1.0f` on the DistanceSpring — the ball
   will settle immediately with no oscillation.
3. Create a chain of 5 balls connected by BallSocket constraints.
   Give the first ball a push and watch the chain swing.

## Summary

Constraints connect two bodies with rules the solver enforces each
frame. BallSocket allows free rotation from a point, Hinge restricts
to one axis, Weld locks bodies rigidly, and DistanceSpring maintains
a springy distance. SpringConfig controls stiffness (frequency) and
settling speed (damping ratio). Use `addConstraint` to create and
`removeConstraint` to destroy them.

**Next**: [Collision Filtering with Layers](08-collision-filtering.html)
*)
