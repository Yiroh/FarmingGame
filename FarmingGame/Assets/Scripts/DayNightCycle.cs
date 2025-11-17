using UnityEngine;
using TMPro;   // <-- IMPORTANT for TextMeshPro

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Length of a full day in real-time minutes.")]
    public float dayLengthInMinutes = 10f;

    [Range(0f, 1f)]
    [Tooltip("Current time of day (0 = start of day, 1 = next day).")]
    public float timeOfDay = 0f;

    [Tooltip("World hour when timeOfDay = 0 (e.g. 6 = 06:00).")]
    public float startHour = 6f;   // 06:00 at timeOfDay = 0

    [Header("Sun Settings")]
    public Light sun;
    public float sunBaseIntensity = 1f;
    public Gradient sunColorOverDay;
    public AnimationCurve sunIntensityOverDay;

    [Header("UI (TextMeshPro)")]
    public TextMeshProUGUI clockText;   // TMP text for clock
    public TextMeshProUGUI phaseText;   // TMP text for phase

    private float dayLengthInSeconds;

    public enum DayPhase { Night, Dawn, Day, Dusk }
    public DayPhase CurrentPhase { get; private set; }

    void Start()
    {
        dayLengthInSeconds = dayLengthInMinutes * 60f;

        if (sunIntensityOverDay == null || sunIntensityOverDay.keys.Length == 0)
        {
            sunIntensityOverDay = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),   // midnight
                new Keyframe(0.25f, 1.0f),  // morning
                new Keyframe(0.5f, 1.0f),   // noon
                new Keyframe(0.75f, 1.0f),  // evening
                new Keyframe(1.0f, 0.0f)    // next midnight
            );
        }
    }

    void Update()
    {
        if (dayLengthInSeconds <= 0f || sun == null)
            return;

        // --- TIME PROGRESSION ---
        timeOfDay += Time.deltaTime / dayLengthInSeconds;
        if (timeOfDay > 1f)
            timeOfDay -= 1f;

        // --- SUN ROTATION & LIGHTING ---
        float sunAngle = timeOfDay * 360f - 90f;  // 0 at horizon
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        if (sunColorOverDay != null)
        {
            sun.color = sunColorOverDay.Evaluate(timeOfDay);
        }

        float intensityMultiplier = sunIntensityOverDay.Evaluate(timeOfDay);
        sun.intensity = sunBaseIntensity * intensityMultiplier;

        RenderSettings.ambientLight = sun.color * 0.3f;

        // --- UI & PHASE ---
        UpdatePhaseAndClockUI();
    }

    void UpdatePhaseAndClockUI()
    {
        // Convert 0–1 timeOfDay into 24h clock
        float currentHour = Mathf.Repeat(startHour + timeOfDay * 24f, 24f);
        int hour = Mathf.FloorToInt(currentHour);
        int minute = Mathf.FloorToInt((currentHour - hour) * 60f);

        // Military time (HH:MM)
        if (clockText != null)
        {
            clockText.text = $"{hour:00}:{minute:00}";
        }

        // Calculate phase from currentHour
        CurrentPhase = GetPhase(currentHour);

        if (phaseText != null)
        {
            phaseText.text = CurrentPhase.ToString();  // "Dawn", "Day", etc.
        }
    }

    DayPhase GetPhase(float hour)
    {
        // 24h ranges – tweak however you like
        if (hour >= 5f && hour < 8f)
            return DayPhase.Dawn;

        if (hour >= 8f && hour < 18f)
            return DayPhase.Day;

        if (hour >= 18f && hour < 21f)
            return DayPhase.Dusk;

        return DayPhase.Night;
    }
}
