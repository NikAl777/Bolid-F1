using Unity.VisualScripting;
using UnityEngine;

public class UISuspension : MonoBehaviour
{
    [SerializeField] private CarSuspension CarSuspension;

    [System.Obsolete]
    void Start()
    {
        
        if (CarSuspension == null)
        {
            CarSuspension = FindObjectOfType<CarSuspension>();

        }

        if (CarSuspension == null)
        {
            Debug.LogError("CarSuspension not found! Assign in Inspector.", this);
        }
    }

    private void OnGUI()
    {
        if (CarSuspension == null)
        {
            GUI.Label(new Rect(10, 10, 300, 22), "ERROR: KartController missing!", GUI.skin.label);
            return;
        }
        if (!Application.isPlaying) return;

        float screenWidth = Screen.width;
        float x = screenWidth - 370; // Правая сторона с отступом
        float y = 10;
        float lineHeight = 22;

        // === СТИЛИ ===
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.fontSize = 14;
        headerStyle.normal.textColor = new Color(0.2f, 0.8f, 1f); // Голубой

        GUIStyle valueStyle = new GUIStyle(GUI.skin.label);
        valueStyle.fontSize = 12;
        valueStyle.normal.textColor = Color.white;

        GUIStyle wheelStyle = new GUIStyle(GUI.skin.label);
        wheelStyle.fontSize = 11;

        // === ФОН И ЗАГОЛОВОК ===
        GUI.Box(new Rect(x - 10, y - 10, 360, 380), "", GUI.skin.box);

        GUI.Label(new Rect(x, y, 350, lineHeight), "🔩 СИСТЕМА ПОДВЕСКИ", headerStyle);
        y += lineHeight + 5;

        // Разделитель
        GUI.Box(new Rect(x, y, 340, 1), "");
        y += 10;

        // === 1. АЭРОДИНАМИКА ===
        float speed = CarSuspension.rb.linearVelocity.magnitude;
        float dragForce = 0.5f * 1.225f * 0.9f * 0.6f * speed * speed;
        float downforce = speed * 70f;

        GUI.Label(new Rect(x, y, 340, lineHeight), "✈️ АЭРОДИНАМИКА", headerStyle);
        y += lineHeight;

        // Сопротивление
        Color dragColor = dragForce < 500 ? Color.white :
                         dragForce < 1000 ? Color.yellow : Color.red;
        valueStyle.normal.textColor = dragColor;
        GUI.Label(new Rect(x + 20, y, 320, lineHeight), $"Сопротивление: {dragForce:F0} Н", valueStyle);
        y += lineHeight;

        // Прижим
        Color downforceColor = downforce < 1000 ? Color.white :
                              downforce < 2000 ? Color.green : Color.cyan;
        valueStyle.normal.textColor = downforceColor;
        GUI.Label(new Rect(x + 20, y, 320, lineHeight), $"Прижим крыла: {downforce:F0} Н", valueStyle);
        y += lineHeight + 10;

        // === 2. СИЛЫ ПОДВЕСКИ ===
        GUI.Label(new Rect(x, y, 340, lineHeight), "🛠️ СИЛЫ ПОДВЕСКИ (Н)", headerStyle);
        y += lineHeight;

        float flTotal = CarSuspension.flSpringForce + CarSuspension.flDamperForce;
        float frTotal = CarSuspension.frSpringForce + CarSuspension.frDamperForce;
        float rlTotal = CarSuspension.rlSpringForce + CarSuspension.rlDamperForce;
        float rrTotal = CarSuspension.rrSpringForce + CarSuspension.rrDamperForce;

        // Цветовая градация по силе
        wheelStyle.normal.textColor = GetForceColor(flTotal);
        GUI.Label(new Rect(x + 20, y, 160, lineHeight), $"FL: {flTotal:F0}", wheelStyle);

        wheelStyle.normal.textColor = GetForceColor(frTotal);
        GUI.Label(new Rect(x + 180, y, 160, lineHeight), $"FR: {frTotal:F0}", wheelStyle);
        y += lineHeight;

        wheelStyle.normal.textColor = GetForceColor(rlTotal);
        GUI.Label(new Rect(x + 20, y, 160, lineHeight), $"RL: {rlTotal:F0}", wheelStyle);

        wheelStyle.normal.textColor = GetForceColor(rrTotal);
        GUI.Label(new Rect(x + 180, y, 160, lineHeight), $"RR: {rrTotal:F0}", wheelStyle);
        y += lineHeight + 10;

        // === 3. ВЫСОТА КОЛЕС ===
        GUI.Label(new Rect(x, y, 340, lineHeight), "📏 ВЫСОТА КОЛЕС (м)", headerStyle);
        y += lineHeight;

        wheelStyle.normal.textColor = GetHeightColor(CarSuspension.flDistance);
        GUI.Label(new Rect(x + 20, y, 160, lineHeight), $"FL: {CarSuspension.flDistance:F3}", wheelStyle);

        wheelStyle.normal.textColor = GetHeightColor(CarSuspension.frDistance);
        GUI.Label(new Rect(x + 180, y, 160, lineHeight), $"FR: {CarSuspension.frDistance:F3}", wheelStyle);
        y += lineHeight;

        wheelStyle.normal.textColor = GetHeightColor(CarSuspension.rlDistance);
        GUI.Label(new Rect(x + 20, y, 160, lineHeight), $"RL: {CarSuspension.rlDistance:F3}", wheelStyle);

        wheelStyle.normal.textColor = GetHeightColor(CarSuspension.rrDistance);
        GUI.Label(new Rect(x + 180, y, 160, lineHeight), $"RR: {CarSuspension.rrDistance:F3}", wheelStyle);
        y += lineHeight + 10;

        // === 4. СЖАТИЕ ПОДВЕСКИ ===
        GUI.Label(new Rect(x, y, 340, lineHeight), "📐 СЖАТИЕ ПОДВЕСКИ", headerStyle);
        y += lineHeight;

        wheelStyle.normal.textColor = GetCompressionColor(CarSuspension.flCompression);
        GUI.Label(new Rect(x + 20, y, 160, lineHeight), $"FL: {CarSuspension.flCompression:F3}", wheelStyle);

        wheelStyle.normal.textColor = GetCompressionColor(CarSuspension.frCompression);
        GUI.Label(new Rect(x + 180, y, 160, lineHeight), $"FR: {CarSuspension.frCompression:F3}", wheelStyle);
        y += lineHeight;

        wheelStyle.normal.textColor = GetCompressionColor(CarSuspension.rlCompression);
        GUI.Label(new Rect(x + 20, y, 160, lineHeight), $"RL: {CarSuspension.rlCompression:F3}", wheelStyle);

        wheelStyle.normal.textColor = GetCompressionColor(CarSuspension.rrCompression);
        GUI.Label(new Rect(x + 180, y, 160, lineHeight), $"RR: {CarSuspension.rrCompression:F3}", wheelStyle);
        y += lineHeight + 10;

        // === 5. ВЫСОТА ЦЕНТРА МАСС ===
        float comHeight = CarSuspension.rb.worldCenterOfMass.y;
        Color comColor = comHeight > 0.5f ? Color.yellow :
                        comHeight > 0.3f ? Color.white : Color.green;
        valueStyle.normal.textColor = comColor;
        GUI.Label(new Rect(x, y, 340, lineHeight), $"⚖️ Высота ЦМ: {comHeight:F3} м", valueStyle);
    }

    // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ДЛЯ ЦВЕТОВОЙ ИНДИКАЦИИ ===

    private Color GetForceColor(float force)
    {
        float absForce = Mathf.Abs(force);
        if (absForce < 1000) return Color.white;
        if (absForce < 2000) return Color.green;
        if (absForce < 3000) return Color.yellow;
        if (absForce < 4000) return new Color(1f, 0.5f, 0f); // Оранжевый
        return Color.red;
    }

    private Color GetHeightColor(float height)
    {
        if (height < 0.1f) return Color.red;
        if (height < 0.2f) return Color.yellow;
        if (height < 0.3f) return Color.green;
        if (height < 0.4f) return Color.cyan;
        return new Color(0.5f, 0.5f, 1f); // Сиреневый
    }

    private Color GetCompressionColor(float compression)
    {
        if (compression > 0.15f) return Color.red;
        if (compression > 0.1f) return Color.yellow;
        if (compression > 0.05f) return Color.white;
        if (compression > -0.05f) return Color.green;
        if (compression > -0.1f) return Color.cyan;
        return new Color(0.5f, 0.5f, 1f); // Сиреневый (отбой)
    }
}
