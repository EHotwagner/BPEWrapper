// Contract: New PhysicsShape module addition
// This module will be added to BepuFSharp/Shapes.fsi

namespace BepuFSharp

module PhysicsShape =

    /// Returns a human-readable description of the shape with its parameters.
    /// Examples: "Sphere(r=0.5)", "Box(w=1, h=2, l=3)", "Capsule(r=0.3, l=1.2)"
    val describe: PhysicsShape -> string
