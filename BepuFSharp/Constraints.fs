namespace BepuFSharp

open System.Numerics

type ConstraintDesc =
    | BallSocket of localOffsetA: Vector3 * localOffsetB: Vector3 * springSettings: SpringConfig
    | Hinge of localHingeAxisA: Vector3 * localHingeAxisB: Vector3 * localOffsetA: Vector3 * localOffsetB: Vector3 * springSettings: SpringConfig
    | Weld of localOffset: Vector3 * localOrientation: Quaternion * springSettings: SpringConfig
    | DistanceLimit of localOffsetA: Vector3 * localOffsetB: Vector3 * minDistance: float32 * maxDistance: float32 * springSettings: SpringConfig
    | DistanceSpring of localOffsetA: Vector3 * localOffsetB: Vector3 * targetDistance: float32 * springSettings: SpringConfig
    | SwingLimit of axisLocalA: Vector3 * axisLocalB: Vector3 * maxSwingAngle: float32 * springSettings: SpringConfig
    | TwistLimit of localAxisA: Vector3 * localAxisB: Vector3 * minAngle: float32 * maxAngle: float32 * springSettings: SpringConfig
    | LinearAxisMotor of localOffsetA: Vector3 * localOffsetB: Vector3 * localAxis: Vector3 * targetVelocity: float32 * settings: MotorSettings
    | AngularMotor of targetVelocity: Vector3 * settings: MotorSettings
    | PointOnLine of localOrigin: Vector3 * localDirection: Vector3 * localOffset: Vector3 * springSettings: SpringConfig
