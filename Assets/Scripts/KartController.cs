using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class KartController : MonoBehaviour
{

    [Header("Configuration")]
    [SerializeField] private KartConfig _kartConfig;

    [Header("Physics")]
    [SerializeField] private float _gravity = 9.81f;

    [Header("Engine & drivetrain")]
    [SerializeField] public KartEngine _engine;
    [SerializeField] private float _drivetrainEfficiency = 0.9f;

    [Header("Wheel attachment points")]
    [SerializeField] private Transform _frontLeftWheel;
    [SerializeField] private Transform _frontRightWheel;
    [SerializeField] private Transform _rearLeftWheel;
    [SerializeField] private Transform _rearRightWheel;

    private Quaternion _frontLeftInitialLocalRot;
    private Quaternion _frontRightInitialLocalRot;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference _moveActionRef;

    private float _throttleInput;
    private float _steerInput; 

    [Header("Handbrake")]
    [SerializeField] private InputActionReference _handbrakeActionRef;
    [SerializeField] private float _handbrakeDragMultiplier = 3f;
    [SerializeField] private bool _handbrakeEnabled = true;

    // Для сбора данных телеметрии
    public float _totalRearLongitudinalForce;
    public float _totalFrontLateralForce;
    public float _frontLeftVLat, _frontRightVLat, _rearLeftVLat, _rearRightVLat;
    private Vector3 _lastVelocity;
    public float _acceleration;

    public bool _isHandbrakePressed;
    public float _originalLateralStiffness; 

    public Rigidbody _rb;

    public float _frontLeftNormalForce;
    public float _frontRightNormalForce;
    public float _rearLeftNormalForce;
    public float _rearRightNormalForce;


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
        RotateFrontWheels(); // Поворачиваем колеса
    }

    private void FixedUpdate()
    {
        // Считаем ускорение
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
        // 1. Получаем массу из Rigidbody
        float mass = _rb.mass;

        // 2. Рассчитываем общий вес
        float totalWeight = mass * _gravity;

        // 3. Распределяем вес по осям
        float frontWeight = totalWeight * _kartConfig.frontAxleShare;
        float rearWeight = totalWeight * (1f - _kartConfig.frontAxleShare);

        // 4. Делим поровну между левым и правым колесом на каждой оси
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

        // Включаем ручной тормоз 
        if (_handbrakeActionRef != null && _handbrakeActionRef.action != null)
            _handbrakeActionRef.action.Enable();
    }

    private void OnDisable()
    {
        if (_moveActionRef != null && _moveActionRef.action != null)
            _moveActionRef.action.Disable();

        // Выключаем ручной тормоз 
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

        // Записываем боковое скольжение для телеметрии
        if (wheel == _frontLeftWheel) _frontLeftVLat = vLat;
        if (wheel == _frontRightWheel) _frontRightVLat = vLat;
        if (wheel == _rearLeftWheel) _rearLeftVLat = vLat;
        if (wheel == _rearRightWheel) _rearRightVLat = vLat;

        // 1) продольная сила от двигателя
        if (isDriven)
        {
            Vector3 bodyForward = transform.forward;
            float speedAlongForward = Vector3.Dot(_rb.linearVelocity, bodyForward);

            // Получаем момент от двигателя
            float engineTorque = _engine.Simulate(
                _throttleInput,
                speedAlongForward,
                Time.fixedDeltaTime
            );

            float totalWheelTorque = engineTorque * _kartConfig.gearRatio * _drivetrainEfficiency;
            float wheelTorque = totalWheelTorque * 0.5f; 

            if (!(_isHandbrakePressed && isDriven && _handbrakeEnabled))
            {
                Fx += wheelTorque / _kartConfig.wheelRadius;
            }
        }

        // 2) сопротивление качению
        float currentRollingResistance = _kartConfig.rollingResistance;

        // Ручной тормоз: увеличиваем сопротивление для задних колес
        if (_isHandbrakePressed && isDriven && _handbrakeEnabled)
        {
            currentRollingResistance *= _handbrakeDragMultiplier;

            // Сила, направленная против движения колеса
            float brakeForce = -Mathf.Sign(vLong) * normalForce * _kartConfig.frictionCoefficient * 0.8f;
            Fx += brakeForce;
        }

        Fx += -currentRollingResistance * vLong;

        // 3) боковая сила
        float currentLateralStiffness = _kartConfig.frontLateralStiffness;

        // Ручной тормоз: убираем боковое сцепление у задних колес
        if (_isHandbrakePressed && isDriven && _handbrakeEnabled)
        {
            currentLateralStiffness = 0f; // Полное отсутствие бокового сцепления
        }

        Fy += -currentLateralStiffness * vLat;

        // 4) фрикционный круг 
        // При ручном тормозе ослабляем фрикционный круг для задних колес
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

        // Собираем силы для телеметрии
        if (isDriven) 
        {
            _totalRearLongitudinalForce += Fx;
        }
        else 
        {
            _totalFrontLateralForce += Mathf.Abs(Fy);
        }

        // 5) мировая сила
        Vector3 force = wheelForward * Fx + wheelRight * Fy;

        _rb.AddForceAtPosition(force, wheelPos, ForceMode.Force);
    }

    
}