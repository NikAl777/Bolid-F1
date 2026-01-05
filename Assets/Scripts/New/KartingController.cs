using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class KartingController : MonoBehaviour
{
    [Header("=== Configuration ===")]
    [SerializeField] private KartConfig _kartConfig;

    [Header("=== Physics ===")]
    [SerializeField] private float _gravity = 9.81f;

    [Header("=== Engine & Drivetrain ===")]
    [SerializeField] private KartEngine _engine;
    [SerializeField] private KartingAero _kartAero;
    [SerializeField] private float _drivetrainEfficiency = 0.9f;

    [Header("=== Wheel Attachment Points ===")]
    [SerializeField] private Transform _frontLeftWheel;
    [SerializeField] private Transform _frontRightWheel;
    [SerializeField] private Transform _rearLeftWheel;
    [SerializeField] private Transform _rearRightWheel;

    private Quaternion _frontLeftInitialLocalRot;
    private Quaternion _frontRightInitialLocalRot;

    [Header("=== Input (New Input System) ===")]
    [SerializeField] private InputActionReference _moveActionRef;

    private float _throttleInput;
    private float _steerInput;

    [Header("=== Handbrake ===")]
    [SerializeField] private InputActionReference _handbrakeActionRef;
    [SerializeField] private float _handbrakeDragMultiplier = 3f;
    [SerializeField] private bool _handbrakeEnabled = true;

    // Телеметрия
    private float _totalRearLongitudinalForce;
    private float _totalFrontLateralForce;
    private float _frontLeftVLat, _frontRightVLat, _rearLeftVLat, _rearRightVLat;
    private Vector3 _lastVelocity;
    private float _acceleration;

    private bool _isHandbrakePressed;
    private float _originalLateralStiffness;

    private Rigidbody _rb;

    private float _frontLeftNormalForce;
    private float _frontRightNormalForce;
    private float _rearLeftNormalForce;
    private float _rearRightNormalForce;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        ComputeStaticWheelLoads();
        Initialize();
        _originalLateralStiffness = _kartConfig.frontLateralStiffness;
    }

    private void Update()
    {
        ReadInput();
        RotateFrontWheels();

        // Регулировка угла крыла в реальном времени
        if (Input.GetKey(KeyCode.E))
            _kartAero.SetWingAngle(_kartAero.GetWingAngle() + Time.deltaTime * 15f);
        if (Input.GetKey(KeyCode.Q))
            _kartAero.SetWingAngle(_kartAero.GetWingAngle() - Time.deltaTime * 15f);
    }

    private void FixedUpdate()
    {
        Vector3 velocityChange = _rb.linearVelocity - _lastVelocity;
        _acceleration = velocityChange.magnitude / Time.fixedDeltaTime;
        _lastVelocity = _rb.linearVelocity;

        _totalRearLongitudinalForce = 0f;
        _totalFrontLateralForce = 0f;

        ApplyWheelForces(_frontLeftWheel, _frontLeftNormalForce, isDriven: false);
        ApplyWheelForces(_frontRightWheel, _frontRightNormalForce, isDriven: false);
        ApplyWheelForces(_rearLeftWheel, _rearLeftNormalForce, isDriven: true);
        ApplyWheelForces(_rearRightWheel, _rearRightNormalForce, isDriven: true);
    }

    private void ComputeStaticWheelLoads()
    {
        float mass = _rb.mass;
        float totalWeight = mass * _gravity;

        float frontWeight = totalWeight * _kartConfig.frontAxleShare;
        float rearWeight = totalWeight * (1f - _kartConfig.frontAxleShare);

        _frontLeftNormalForce = frontWeight * 0.5f;
        _frontRightNormalForce = frontWeight * 0.5f;
        _rearLeftNormalForce = rearWeight * 0.5f;
        _rearRightNormalForce = rearWeight * 0.5f;
    }

    private void Initialize()
    {
        if (_frontLeftWheel != null)
            _frontLeftInitialLocalRot = _frontLeftWheel.localRotation;
        if (_frontRightWheel != null)
            _frontRightInitialLocalRot = _frontRightWheel.localRotation;
    }

    private void ReadInput()
    {
        Vector2 move = _moveActionRef.action.ReadValue<Vector2>();
        _steerInput = Mathf.Clamp(move.x, -1f, 1f);
        _throttleInput = Mathf.Clamp(move.y, -1f, 1f);
        _isHandbrakePressed = _handbrakeActionRef.action.ReadValue<float>() > 0.5f;
    }

    private void RotateFrontWheels()
    {
        float steerAngle = _kartConfig.maxSteerAngle * _steerInput;
        Quaternion steerRotation = Quaternion.Euler(0f, steerAngle, 0f);

        if (_frontLeftWheel != null)
            _frontLeftWheel.localRotation = _frontLeftInitialLocalRot * steerRotation;
        if (_frontRightWheel != null)
            _frontRightWheel.localRotation = _frontRightInitialLocalRot * steerRotation;
    }

    private void OnEnable()
    {
        if (_moveActionRef != null && _moveActionRef.action != null)
            _moveActionRef.action.Enable();
        if (_handbrakeActionRef != null && _handbrakeActionRef.action != null)
            _handbrakeActionRef.action.Enable();
    }

    private void OnDisable()
    {
        if (_moveActionRef != null && _moveActionRef.action != null)
            _moveActionRef.action.Disable();
        if (_handbrakeActionRef != null && _handbrakeActionRef.action != null)
            _handbrakeActionRef.action.Disable();
    }

    private void ApplyWheelForces(Transform wheel, float normalForce, bool isDriven)
    {
        if (wheel == null || _rb == null) return;

        Vector3 wheelPos = wheel.position;
        Vector3 wheelForward = wheel.forward;
        Vector3 wheelRight = wheel.right;
        Vector3 v = _rb.GetPointVelocity(wheelPos);

        float vLong = Vector3.Dot(v, wheelForward);
        float vLat = Vector3.Dot(v, wheelRight);

        float Fx = 0f;
        float Fy = 0f;

        if (wheel == _frontLeftWheel) _frontLeftVLat = vLat;
        if (wheel == _frontRightWheel) _frontRightVLat = vLat;
        if (wheel == _rearLeftWheel) _rearLeftVLat = vLat;
        if (wheel == _rearRightWheel) _rearRightVLat = vLat;

        // 1) Продольная сила от двигателя
        if (isDriven)
        {
            Vector3 bodyForward = transform.forward;
            float speedAlongForward = Vector3.Dot(_rb.linearVelocity, bodyForward);

            float engineTorque = _engine.Simulate(_throttleInput, speedAlongForward, Time.fixedDeltaTime);
            float totalWheelTorque = engineTorque * _kartConfig.gearRatio * _drivetrainEfficiency;
            float wheelTorque = totalWheelTorque * 0.5f;

            if (!(_isHandbrakePressed && isDriven && _handbrakeEnabled))
            {
                Fx += wheelTorque / _kartConfig.wheelRadius;
            }
        }

        // 2) Сопротивление качению
        float currentRollingResistance = _kartConfig.rollingResistance;

        if (_isHandbrakePressed && isDriven && _handbrakeEnabled)
        {
            currentRollingResistance *= _handbrakeDragMultiplier;
            float brakeForce = -Mathf.Sign(vLong) * normalForce * _kartConfig.frictionCoefficient * 0.8f;
            Fx += brakeForce;
        }

        Fx += -currentRollingResistance * vLong;

        // 3) Боковая сила
        float currentLateralStiffness = _kartConfig.frontLateralStiffness;

        if (_isHandbrakePressed && isDriven && _handbrakeEnabled)
        {
            currentLateralStiffness = 0f;
        }

        Fy += -currentLateralStiffness * vLat;

        // 4) Фрикционный круг
        float currentFrictionCoefficient = _kartConfig.frictionCoefficient;
        if (_isHandbrakePressed && isDriven && _handbrakeEnabled)
        {
            currentFrictionCoefficient *= 0.3f;
        }

        float frictionLimit = currentFrictionCoefficient * normalForce;
        float forceLength = Mathf.Sqrt(Fx * Fx + Fy * Fy);

        if (forceLength > frictionLimit && forceLength > 1e-6f)
        {
            float scale = frictionLimit / forceLength;
            Fx *= scale;
            Fy *= scale;
        }

        if (isDriven)
        {
            _totalRearLongitudinalForce += Fx;
        }
        else
        {
            _totalFrontLateralForce += Mathf.Abs(Fy);
        }

        Vector3 force = wheelForward * Fx + wheelRight * Fy;
        _rb.AddForceAtPosition(force, wheelPos, ForceMode.Force);
    }

    private void OnGUI()
    {
        float speedMs = _rb.linearVelocity.magnitude;
        float speedKmh = speedMs * 3.6f;

        float x = 10;
        float y = 10;
        float lineHeight = 22;

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = Color.yellow;

        GUIStyle valueStyle = new GUIStyle(GUI.skin.label);
        valueStyle.fontSize = 12;
        valueStyle.normal.textColor = Color.white;

        GUI.Box(new Rect(x - 5, y - 5, 450, 400), "", GUI.skin.box);

        GUI.Label(new Rect(x, y, 400, lineHeight), "🏎️ КАРТИНГ - ПОЛНАЯ ТЕЛЕМЕТРИЯ", headerStyle);
        y += lineHeight + 5;

        GUI.Box(new Rect(x, y, 430, 1), "");
        y += 10;

        // === СКОРОСТЬ ===
        Color speedColor = speedKmh < 30 ? Color.white :
                          speedKmh < 60 ? Color.green :
                          speedKmh < 90 ? Color.yellow : Color.red;
        valueStyle.normal.textColor = speedColor;
        GUI.Label(new Rect(x, y, 400, lineHeight), $"🚀 Скорость: {speedKmh:F1} км/ч ({speedMs:F1} м/с)", valueStyle);
        y += lineHeight;

        // === RPM ===
        Color rpmColor = _engine.CurrentRpm < 3000 ? Color.white :
                        _engine.CurrentRpm < 5000 ? Color.green :
                        _engine.CurrentRpm < 7000 ? Color.yellow : Color.red;
        valueStyle.normal.textColor = rpmColor;
        GUI.Label(new Rect(x, y, 400, lineHeight), $"⚙️ RPM: {_engine.CurrentRpm:F0} об/мин", valueStyle);
        y += lineHeight;

        // === МОМЕНТ ===
        valueStyle.normal.textColor = Color.cyan;
        GUI.Label(new Rect(x, y, 400, lineHeight), $"🔧 Момент: {_engine.CurrentTorque:F0} Н·м", valueStyle);
        y += lineHeight;

        // === Fx и Fy ===
        valueStyle.normal.textColor = new Color(0.8f, 0.4f, 1f);
        GUI.Label(new Rect(x, y, 400, lineHeight), $"🔽 Fx задней оси: {_totalRearLongitudinalForce:F0} Н", valueStyle);
        y += lineHeight;

        valueStyle.normal.textColor = new Color(1f, 0.6f, 0f);
        GUI.Label(new Rect(x, y, 400, lineHeight), $"🔄 Fy передней оси: {_totalFrontLateralForce:F0} Н", valueStyle);
        y += lineHeight;

        y += 10;
        GUI.Box(new Rect(x, y, 430, 1), "");
        y += 10;

        // === АЭРОДИНАМИКА ===
        headerStyle.normal.textColor = new Color(0.2f, 0.8f, 1f);
        GUI.Label(new Rect(x, y, 400, lineHeight), "✈️ АЭРОДИНАМИКА", headerStyle);
        y += lineHeight;

        valueStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x + 20, y, 400, lineHeight), $"• Drag (сопротивление): {_kartAero.GetCurrentDragForce():F0} Н", valueStyle);
        y += lineHeight;

        GUI.Label(new Rect(x + 20, y, 400, lineHeight), $"• Downforce (прижим крыла): {_kartAero.GetCurrentDownforce():F0} Н", valueStyle);
        y += lineHeight;

        Color geColor = _kartAero.GetCurrentGroundEffect() > 500 ? Color.green : Color.white;
        valueStyle.normal.textColor = geColor;
        GUI.Label(new Rect(x + 20, y, 400, lineHeight), $"• Ground Effect: {_kartAero.GetCurrentGroundEffect():F0} Н", valueStyle);
        y += lineHeight;

        valueStyle.normal.textColor = Color.cyan;
        GUI.Label(new Rect(x + 20, y, 400, lineHeight), $"• Угол крыла (Q/E): {_kartAero.GetWingAngle():F1}°", valueStyle);
        y += lineHeight + 10;

        // === УПРАВЛЕНИЕ ===
        GUI.Box(new Rect(x, y, 430, 1), "");
        y += 10;

        headerStyle.normal.textColor = Color.green;
        GUI.Label(new Rect(x, y, 400, lineHeight), "🎮 УПРАВЛЕНИЕ", headerStyle);
        y += lineHeight;

        valueStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x + 20, y, 400, lineHeight), "W/S - газ/тормоз", valueStyle);
        y += lineHeight;

        GUI.Label(new Rect(x + 20, y, 400, lineHeight), "A/D - повороты", valueStyle);
        y += lineHeight;

        if (_isHandbrakePressed)
        {
            valueStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(x + 20, y, 400, lineHeight), "SPACE - ручной тормоз (⚠️ АКТИВЕН)", valueStyle);
        }
        else
        {
            valueStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(x + 20, y, 400, lineHeight), "SPACE - ручной тормоз", valueStyle);
        }
        y += lineHeight;

        valueStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x + 20, y, 400, lineHeight), "Q/E - регулировка угла крыла", valueStyle);
    }
}
