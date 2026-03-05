namespace BepuFSharp

open System.Numerics

/// Constraint descriptor variants for connecting bodies.
type ConstraintDesc =
    /// Ball-and-socket joint.
    | BallSocket of localOffsetA: Vector3 * localOffsetB: Vector3 * springSettings: SpringConfig
    /// Hinge joint with axis.
    | Hinge of localHingeAxisA: Vector3 * localHingeAxisB: Vector3 * localOffsetA: Vector3 * localOffsetB: Vector3 * springSettings: SpringConfig
    /// Rigid weld attachment.
    | Weld of localOffset: Vector3 * localOrientation: Quaternion * springSettings: SpringConfig
    /// Distance range constraint.
    | DistanceLimit of localOffsetA: Vector3 * localOffsetB: Vector3 * minDistance: float32 * maxDistance: float32 * springSettings: SpringConfig
    /// Spring-like distance constraint.
    | DistanceSpring of localOffsetA: Vector3 * localOffsetB: Vector3 * targetDistance: float32 * springSettings: SpringConfig
    /// Cone-shaped angular limit.
    | SwingLimit of axisLocalA: Vector3 * axisLocalB: Vector3 * maxSwingAngle: float32 * springSettings: SpringConfig
    /// Twist range limit.
    | TwistLimit of localAxisA: Vector3 * localAxisB: Vector3 * minAngle: float32 * maxAngle: float32 * springSettings: SpringConfig
    /// Linear motor along axis.
    | LinearAxisMotor of localOffsetA: Vector3 * localOffsetB: Vector3 * localAxis: Vector3 * targetVelocity: float32 * settings: MotorSettings
    /// Angular velocity motor.
    | AngularMotor of targetVelocity: Vector3 * settings: MotorSettings
    /// Point-on-line servo.
    | PointOnLine of localOrigin: Vector3 * localDirection: Vector3 * localOffset: Vector3 * springSettings: SpringConfig
