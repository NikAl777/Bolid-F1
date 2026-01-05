using UnityEngine;

public class CarSuspension : MonoBehaviour
{
    [Header("Suspension Points")]
    [SerializeField] private Transform fl;
    [SerializeField] private Transform fr;
    [SerializeField] private Transform rl;
    [SerializeField] private Transform rr;

    [Header("Suspension Settings")]
    [SerializeField] private float restLength = 0.4f;
    [SerializeField] private float springTravel = 0.2f;
    [SerializeField] private float springStiffness = 20000f;
    [SerializeField] private float damperStiffness = 3500f;
    [SerializeField] private float wheelRadius = 0.35f;

    [Header("Anti-Roll Bar")]
    [SerializeField] private float frontAntiRollStiffness = 8000f;
    [SerializeField] private float rearAntiRollStiffness = 6000f;

    // Данные для телеметрии
    public float flDistance, frDistance, rlDistance, rrDistance;
    public float flCompression, frCompression, rlCompression, rrCompression;
    public float flSpringForce, frSpringForce, rlSpringForce, rrSpringForce;
    public float flDamperForce, frDamperForce, rlDamperForce, rrDamperForce;

    public Rigidbody rb;
    private LayerMask groundLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        groundLayer = LayerMask.GetMask("Default");
    }

    private void FixedUpdate()
    {
        SimulateWheel(fl, ref flCompression, out flDistance, out flSpringForce, out flDamperForce);
        SimulateWheel(fr, ref frCompression, out frDistance, out frSpringForce, out frDamperForce);
        SimulateWheel(rl, ref rlCompression, out rlDistance, out rlSpringForce, out rlDamperForce);
        SimulateWheel(rr, ref rrCompression, out rrDistance, out rrSpringForce, out rrDamperForce);

        ApplyAntiRollBars();
    }

    private void SimulateWheel(Transform pivot, ref float compression,
        out float distance, out float springForce, out float damperForce)
    {
        distance = 0f;
        springForce = 0f;
        damperForce = 0f;

        if (pivot == null) return;

        Vector3 origin = pivot.position;
        Vector3 direction = -pivot.up;
        float maxDist = restLength + springTravel + wheelRadius;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDist, groundLayer))
        {
            distance = hit.distance;

            float currentLength = hit.distance - wheelRadius;
            currentLength = Mathf.Clamp(currentLength,
                restLength - springTravel,
                restLength + springTravel);

            float newCompression = restLength - currentLength;

            springForce = newCompression * springStiffness;

            float compressionVelocity = (newCompression - compression) / Time.fixedDeltaTime;
            damperForce = compressionVelocity * damperStiffness;

            compression = newCompression;

            float totalForce = springForce + damperForce;
            rb.AddForceAtPosition(pivot.up * totalForce, pivot.position, ForceMode.Force);
        }
        else
        {
            compression = 0f;
            distance = maxDist;
        }
    }

    private void ApplyAntiRollBars()
    {
        float frontDiff = flCompression - frCompression;
        float frontForce = frontDiff * frontAntiRollStiffness;

        if (flCompression > -0.0001f)
            rb.AddForceAtPosition(-transform.up * frontForce, fl.position, ForceMode.Force);
        if (frCompression > -0.0001f)
            rb.AddForceAtPosition(transform.up * frontForce, fr.position, ForceMode.Force);

        float rearDiff = rlCompression - rrCompression;
        float rearForce = rearDiff * rearAntiRollStiffness;

        if (rlCompression > -0.0001f)
            rb.AddForceAtPosition(-transform.up * rearForce, rl.position, ForceMode.Force);
        if (rrCompression > -0.0001f)
            rb.AddForceAtPosition(transform.up * rearForce, rr.position, ForceMode.Force);
    }

   
}