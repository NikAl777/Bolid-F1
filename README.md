# Kart Physics Simulator for Unity

A realistic go-kart physics simulator for Unity featuring detailed engine modeling, tire and suspension physics, aerodynamics, and telemetry visualization. This system provides accurate vehicle dynamics with configurable parameters for fine-tuning kart behavior.

## ✨ Features

### 🏎️ Advanced Physics Simulation
- **Realistic engine model**: torque curve, inertia, rev limiter, throttle response
- **Proper drivetrain**: gear ratio, efficiency calculations
- **Weight distribution** (front/rear axles)
- **Friction circle tire model** (combined longitudinal/lateral forces)
- **Rolling resistance** and engine friction losses
- **Accurate suspension system** with springs, dampers, and anti-roll bars per wheel
- **Aerodynamic effects**: drag, wing downforce, and ground effect

### 🎮 Input System
- Built-in Unity Input System support
- Analog throttle and steering
- Handbrake for controlled drifting
- Smooth input filtering

### 📊 Comprehensive Telemetry
- Real-time RPM, torque, and speed monitoring
- Wheel-specific force and suspension compression visualization
- Suspension travel/force, tire slip, and ride height display
- Aerodynamic drag/downforce display
- On-screen multi-column GUI

### 🔧 Configurable Parameters
- **ScriptableObject KartSettings** for easy tuning
- Adjustable mass, friction coefficients, stiffness values
- Custom engine and wing parameters
- Configurable suspension (stiffness, damping, roll bar, travel)
- Tunable handbrake/drift behavior

## 🛠️ Technical Implementation

### Core Components

1. **KartSettings** (ScriptableObject)
   - Centralized configuration asset (engine, physics, aero, suspension)
   - Easy to create and share

2. **KartEngine**
   - Detailed combustion engine simulation with torque curve, inertia
   - Rev limiter, friction/load calculations

3. **KartController**
   - Main physics controller with Rigidbody integration
   - Per-wheel force calculations (longitudinal/lateral/vertical)
   - Tire and handbrake logic, telemetry and OnGUI display

4. **CarSuspension**
   - Fully simulated independent suspension per wheel
   - Spring and damper calculation, anti-roll bars
   - OnGUI display: suspension travel/force and ride height

5. **KartAero**
   - Aerodynamic drag and wing downforce (rear wing)
   - Ground effect (extra downforce close to ground)
   - Real-time visualization via OnGUI or integrated telemetry

### Physics Model
- **Longitudinal**: Engine torque, rolling resistance, handbrake
- **Lateral**: Tire slip curve, cornering stiffness
- **Normal**: Springs/dampers, static/dynamic load
- **Aerodynamic**: Drag, downforce, ground effect
- **Force limiting**: μN friction circle constraint
- **Wheel kinematics** and full steering geometry

## 🚀 Getting Started

### Installation

1. Import all scripts into your Unity project (`Assets/Scripts`).
2. Create a `KartSettings` asset (menu: `Create > Karting > Kart Settings`)
3. Configure kart engine, physics, aerodynamic, and suspension parameters
4. Attach the following components to your kart GameObject:
   - `KartController`
   - `KartEngine`
   - `CarSuspension`
   - `KartAero`
5. Assign all wheel and wing transforms, set up Unity Rigidbody

### Input Setup

1. Create Input Actions for steering/throttle (Vector2) and handbrake (Button)
2. Assign them in the `KartController` inspector
3. Recommended: use a gamepad for best analog control

## 📈 Telemetry & Visualization

Both physics and real-time telemetry can be monitored via rich OnGUI overlays integrated into the simulator.

- **CarSuspension.cs** — displays:
  - Individual wheel suspension forces (spring, damper, total)
  - Ride height for each wheel
  - Suspension compression status (color-graded)
  - Center of mass height
- **KartAero.cs** — integrates:
  - Real-time drag and downforce calculation (applied to Rigidbody)
- **Engine/RPM/wheel data** — via `KartController` or main telemetry GUI

### Example:

On play, an on-screen (in-game) panel will show:

- Suspension forces per wheel (with color coding by magnitude)
- Aero drag force and rear wing downforce
- Wheel travel (compression/extension), ride height and center of mass

Default panel is rendered via `OnGUI` in `CarSuspension.cs`, Aero info available via both `CarSuspension` and `KartAero`.

## 🛠️ Detailed Scripts

- **CarSuspension.cs**: Implements spring-damper for each wheel, anti-roll bar, and GUI overlay for force & geometry visualization.
- **KartAero.cs**: Simulates air drag, rear wing downforce, and ground effect; updates Rigidbody forces every frame.
- **SmoothCameraFollow.cs**: Optional camera for dynamic chasing view.

## 📝 Notes

- Designed for Unity FixedUpdate physics cycle
- All units: metric (N, m/s, kg)
- Use ForceMode.Force for gradual/realistic forces
- All transforms (wheels, wing) need to be properly placed/assigned
- Telemetry (OnGUI) is customizable and usable for debugging & tuning
- Compatible with Unity's Input System

## 🎯 Tuning Tips

#### For More Grip
- Increase friction coefficient
- Raise lateral stiffness (suspension & tires)
- Shift weight distribution rearward for traction

#### For Drift
- Soften rear suspension/tires
- Increase handbrake force
- Lower rear lateral stiffness

#### Engine/Aero
- Adjust throttle response & torque curve for desired power
- Increase wing angle/area for more rear downforce at speed
- Increase drag/frontal area for more challenge at higher speeds

## 🎮 Controls

- **Steering**: Horizontal axis/input action
- **Throttle/Brake**: Vertical axis/input
- **Handbrake**: Assigned button (default: space/shift)
- All keys configurable via the Unity Input System

## 🔮 Future Enhancements

- Advanced aero (multiple wings, DRS, yaw effects)
- Terrain-aware suspension and contact model
- Pacejka tire model
- Transmission with multiple gear ratios
- Multiplayer support, logging, replays

## 📄 License

Provided "as is" for educational and development use. Modify and extend for your own needs!

---

# Физический симулятор картинга для Unity

Реалистичный симулятор физики картинга для Unity с моделированием двигателя, подвески, аэродинамики и подробной телеметрией.

## ✨ Особенности

### 🏎️ Продвинутая физика

- **Двигатель**: инерция, кривая момента, отсечка оборотов
- **Трансмиссия**: передаточные числа, КПД
- **Модель шин по фрикционному кругу**
- **Полная подвеска**: пружины, амортизаторы, стабилизаторы крена
- **Аэродинамика**: сопротивление, прижимная сила, эффект земли

### 🎮 Система ввода
- Встроенная поддержка Input System Unity
- Аналоговое управление
- Ручной тормоз и сглаживание сигналов

### 📊 Телеметрия и визуализация

- OnGUI панель в реальном времени (по CarSuspension.cs):
  - Силы подвески (пружина, демпфер)
  - Высота и сжатие по каждому колесу (цветовое кодирование)
  - Сопротивление воздуха и прижимная сила крыла
  - Высота центра масс

### 🔧 Параметры настройки

- Централизованный ScriptableObject (KartSettings)
- Масса, трение, жесткость, геометрия подвески
- Мощность крыла и углы атаки

## 🛠️ Основные компоненты

1. **CarSuspension.cs** — На каждое колесо: расчет сил подвески, демпфирование, стабилизатор. Визуализация сил, геометрии.
2. **KartAero.cs** — Расчет и применение аэродинамических сил по физической формуле.
3. **KartController & KartEngine** — Основная физика движения.

## 🚀 Установка и настройка

1. Импортируйте все скрипты в `/Assets/Scripts`
2. Добавьте компоненты ко всем нужным объектам (KartController, KartEngine, CarSuspension, KartAero)
3. Задайте параметры и привяжите трансформы колес/крыльев
4. Настройте Rigidbody для карты
5. Для управления используйте InputActions и Gamepad/клавиатуру

## 📈 Телеметрия

- Мониторинг в реальном времени: силы подвески, высота, компрессия, аэродинамика, RPM, моменты, скорость
- Переключение отображения `OnGUI`, кастомизация через код
- Используйте для тонкой настройки поведения автомобиля

## 📄 License / Лицензия

Available as-is for learning, prototyping, and development.  
Доступно для обучения, экспериментов и разработки.
