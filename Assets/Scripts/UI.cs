using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private KartController kartController; // Drag in Inspector

    [System.Obsolete]
    void Start()
    {
        // Auto-find if not assigned (adjust tag/layer as needed)
        if (kartController == null)
        {
            kartController = FindObjectOfType<KartController>();
            
        }

        if (kartController == null)
        {
            Debug.LogError("KartController not found! Assign in Inspector.", this);
        }
    }

    private void OnGUI()
    {
        if (kartController == null)
        {
            GUI.Label(new Rect(10, 10, 300, 22), "ERROR: KartController missing!", GUI.skin.label);
            return;
        }

        float speedMs = kartController._rb.linearVelocity.magnitude;
        float speedKmh = speedMs * 3.6f;

        float x = 10;
        float y = 10;
        float lineHeight = 22;

        // === СТИЛИ ===
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = Color.yellow;

        GUIStyle valueStyle = new GUIStyle(GUI.skin.label);
        valueStyle.fontSize = 12;
        valueStyle.normal.textColor = Color.white;

        GUIStyle warningStyle = new GUIStyle(GUI.skin.label);
        warningStyle.fontStyle = FontStyle.Bold;
        warningStyle.fontSize = 13;
        warningStyle.normal.textColor = new Color(1f, 0.5f, 0f); // Оранжевый

        GUIStyle criticalStyle = new GUIStyle(GUI.skin.label);
        criticalStyle.fontStyle = FontStyle.Bold;
        criticalStyle.fontSize = 13;
        criticalStyle.normal.textColor = Color.red;

        // === ФОН ===
        GUI.Box(new Rect(x - 5, y - 5, 340, 250), "", GUI.skin.box);

        // === ЗАГОЛОВОК ===
        GUI.Label(new Rect(x, y, 300, lineHeight), "🏎️ ТЕЛЕМЕТРИЯ КАРТА", headerStyle);
        y += lineHeight + 5;

        // Разделитель
        GUI.Box(new Rect(x, y, 320, 1), "");
        y += 10;

        // 1. Скорость (с цветовой индикацией)
        Color speedColor = speedKmh < 30 ? Color.white :
                          speedKmh < 60 ? Color.green :
                          speedKmh < 90 ? Color.yellow : Color.red;
        valueStyle.normal.textColor = speedColor;
        GUI.Label(new Rect(x, y, 300, lineHeight), $"🚀 Скорость: {speedKmh:F1} км/ч ({speedMs:F1} м/с)", valueStyle);
        y += lineHeight;

        // 2. RPM (с цветовой индикацией)
        Color rpmColor = kartController._engine.CurrentRpm < 3000 ? Color.white :
                        kartController._engine.CurrentRpm < 5000 ? Color.green :
                        kartController._engine.CurrentRpm < 7000 ? Color.yellow : Color.red;
        valueStyle.normal.textColor = rpmColor;
        GUI.Label(new Rect(x, y, 300, lineHeight), $"⚙️ RPM: {kartController._engine.CurrentRpm:F0} об/мин", valueStyle);
        y += lineHeight;

        // 3. Момент двигателя
        valueStyle.normal.textColor = Color.cyan;
        GUI.Label(new Rect(x, y, 300, lineHeight), $"🔧 Момент: {kartController._engine.CurrentTorque:F0} Н·м", valueStyle);
        y += lineHeight;

        // 4. Ускорение (с цветовой индикацией)
        Color accelColor = Mathf.Abs(kartController._acceleration) < 5 ? Color.white :
                          Mathf.Abs(kartController._acceleration) < 10 ? Color.yellow : Color.red;
        valueStyle.normal.textColor = accelColor;
        string accelIcon = kartController._acceleration > 0 ? "📈" : kartController._acceleration < 0 ? "📉" : "➡️";
        GUI.Label(new Rect(x, y, 300, lineHeight), $"{accelIcon} Ускорение: {kartController._acceleration:F1} м/с²", valueStyle);
        y += lineHeight;

        // 5. Продольная сила задней оси
        valueStyle.normal.textColor = new Color(0.8f, 0.4f, 1f); // Фиолетовый
        GUI.Label(new Rect(x, y, 300, lineHeight), $"🔽 Fx задняя ось: {kartController._totalRearLongitudinalForce:F0} Н", valueStyle);
        y += lineHeight;

        // 6. Боковая сила передней оси
        valueStyle.normal.textColor = new Color(1f, 0.6f, 0f); // Оранжевый
        GUI.Label(new Rect(x, y, 300, lineHeight), $"🔄 Fy передняя ось: {kartController._totalFrontLateralForce:F0} Н", valueStyle);
        y += lineHeight;

        // Разделитель
        y += 5;
        GUI.Box(new Rect(x, y, 320, 1), "");
        y += 10;

        // 7-8. Боковое скольжение колес
        GUI.Label(new Rect(x, y, 300, lineHeight), "🛞 Скольжение колес:", headerStyle);
        y += lineHeight;

        valueStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(x + 20, y, 300, lineHeight),
            $"Перед: L={kartController._frontLeftVLat:F2} | R={kartController._frontRightVLat:F2}", valueStyle);
        y += lineHeight;

        GUI.Label(new Rect(x + 20, y, 300, lineHeight),
            $"Зад:  L={kartController._rearLeftVLat:F2}  | R= {kartController._rearRightVLat:F2}", valueStyle);
        y += lineHeight + 5;

        // 9. Ручной тормоз
        if (kartController._isHandbrakePressed)
        {
            GUI.Box(new Rect(x - 5, y - 5, 330, 40), "", GUI.skin.box);
            warningStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(x, y, 300, lineHeight), "⚠️ РУЧНОЙ ТОРМОЗ: АКТИВЕН", warningStyle);
            y += lineHeight;
            warningStyle.normal.textColor = new Color(1f, 0.7f, 0f);
            GUI.Label(new Rect(x, y, 300, lineHeight), "Боковое сцепление снижено", warningStyle);
        }
        else
        {
            valueStyle.normal.textColor = Color.gray;
            GUI.Label(new Rect(x, y, 300, lineHeight), "✅ Ручной тормоз: неактивен", valueStyle);
        }
    }
}

