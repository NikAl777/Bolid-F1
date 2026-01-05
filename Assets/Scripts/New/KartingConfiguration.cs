using UnityEngine;

[CreateAssetMenu(fileName = "KartingConfiguration", menuName = "Karting/Karting Configuration")]
public class KartingConfiguration : ScriptableObject
{
    [Header("=== CHASSIS ===")]
    public float mass = 80f;

    [Header("=== WHEEL PHYSICS ===")]
    public float frictionCoefficient = 4.0f;
    public float frontLateralStiffness = 1000f;
    public float rearLateralStiffness = 80f;
    public float rollingResistance = 0.5f;

    [Header("=== STEERING ===")]
    public float maxSteerAngle = 30f;

    [Header("=== ENGINE ===")]
    public AnimationCurve engineTorqueCurve;
    public float engineInertia = 0.2f;
    public float maxRpm = 8000f;

    [Header("=== DRIVETRAIN ===")]
    public float gearRatio = 8f;
    public float wheelRadius = 0.3f;

    [Header("=== WEIGHT DISTRIBUTION ===")]
    [Range(0f, 1f)]
    public float frontAxleShare = 0.4f;

    [Header("=== AERO: DRAG ===")]
    public float airDensity = 1.225f;
    public float dragCoefficient = 0.9f;
    public float frontalArea = 0.6f;

    [Header("=== AERO: WING ===")]
    public float wingArea = 0.4f;
    public float liftCoefficientSlope = 0.05f;
    [Range(0f, 30f)]
    public float wingAngleDeg = 10f;

    [Header("=== AERO: GROUND EFFECT ===")]
    public float groundEffectStrength = 3000f;
    public float groundRayLength = 1.0f;

    private void OnEnable()
    {
        if (engineTorqueCurve == null || engineTorqueCurve.keys.Length == 0)
        {
            CreateDefaultTorqueCurve();
        }
    }

    private void CreateDefaultTorqueCurve()
    {
        engineTorqueCurve = new AnimationCurve();
        engineTorqueCurve.AddKey(new Keyframe(0, 0));
        engineTorqueCurve.AddKey(new Keyframe(1000f, 200f));
        engineTorqueCurve.AddKey(new Keyframe(3000f, 380f));
        engineTorqueCurve.AddKey(new Keyframe(6000f, 350f));
        engineTorqueCurve.AddKey(new Keyframe(8000f, 250f));
    }
}
