using UnityEngine;

public class KartingAero : MonoBehaviour
{
    [Header("=== AERO DRAG ===")]
    [SerializeField] private float airDensity = 1.225f;
    [SerializeField] private float dragCoefficient = 0.9f; // Cx
    [SerializeField] private float frontalArea = 0.6f;     // A (м²)

    [Header("=== REAR WING ===")]
    [SerializeField] private Transform rearWing;
    [SerializeField] private float wingArea = 0.4f;
    [SerializeField] private float liftCoefficientSlope = 0.05f; // k
    [SerializeField] private float wingAngleDeg = 10f;

    [Header("=== GROUND EFFECT ===")]
    [SerializeField] private float groundEffectStrength = 3000f;
    [SerializeField] private float groundRayLength = 1.0f;

    private Rigidbody rb;

    // Для телеметрии
    private float currentDragForce;
    private float currentDownforce;
    private float currentGroundEffect;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogError("KartAero: Rigidbody не найден на объекте!");
    }

    private void FixedUpdate()
    {
        ApplyDrag();
        ApplyWingDownforce();
        ApplyGroundEffect();
    }

    // ==================== DRAG (Сопротивление) ====================

    private void ApplyDrag()
    {
        Vector3 v = rb.linearVelocity;
        float speed = v.magnitude;

        if (speed < 0.01f)
        {
            currentDragForce = 0f;
            return;
        }

        // Fd = 0.5 * ρ * Cx * A * v²
        currentDragForce = 0.5f * airDensity * dragCoefficient * frontalArea * speed * speed;

        // Сила против направления движения
        Vector3 drag = -v.normalized * currentDragForce;

        rb.AddForce(drag, ForceMode.Force);
    }

    // ==================== WING DOWNFORCE (Прижимная сила крыла) ====================

    private void ApplyWingDownforce()
    {
        if (rearWing == null)
        {
            currentDownforce = 0f;
            return;
        }

        float speed = rb.linearVelocity.magnitude;
        if (speed < 0.01f)
        {
            currentDownforce = 0f;
            return;
        }

        // Cl(α) = k * α
        float alphaRad = wingAngleDeg * Mathf.Deg2Rad;
        float Cl = liftCoefficientSlope * alphaRad;

        // Fdown = 0.5 * ρ * Cl * A_wing * v²
        currentDownforce = 0.5f * airDensity * Cl * wingArea * speed * speed;

        // Сила направлена вниз
        Vector3 force = -transform.up * currentDownforce;

        rb.AddForceAtPosition(force, rearWing.position, ForceMode.Force);
    }

    // ==================== GROUND EFFECT (Граунд-эффект) ====================

    private void ApplyGroundEffect()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, groundRayLength))
        {
            float h = hit.distance; // высота над землёй
            if (h < 0.01f) h = 0.01f;

            // Fge = Cge / h
            currentGroundEffect = groundEffectStrength / h;

            Vector3 force = -transform.up * currentGroundEffect;

            rb.AddForce(force, ForceMode.Force);
        }
        else
        {
            currentGroundEffect = 0f;
        }
    }

    // ==================== GETTER МЕТОДЫ ДЛЯ ТЕЛЕМЕТРИИ ====================

    public float GetCurrentDragForce() => currentDragForce;
    public float GetCurrentDownforce() => currentDownforce;
    public float GetCurrentGroundEffect() => currentGroundEffect;
    public float GetWingAngle() => wingAngleDeg;

    public void SetWingAngle(float angle)
    {
        wingAngleDeg = Mathf.Clamp(angle, 0f, 30f);
    }
}
