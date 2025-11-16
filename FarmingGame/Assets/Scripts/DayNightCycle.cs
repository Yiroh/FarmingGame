using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Length of a full day in real-time minutes.")]
    public float dayLengthInMinutes = 10f;

    [Range(0f, 1f)]
    [Tooltip("Current time of day (0 = sunrise, 0.5 = sunset, 1 = next sunrise).")]
    public float timeOfDay = 0f;

    [Header("Sun Settings")]
    public Light sun;
    public float sunBaseIntensity = 1f;
    public Gradient sunColorOverDay;
    public AnimationCurve sunIntensityOverDay;

    private float dayLengthInSeconds;

    void Start()
    {
        dayLengthInSeconds = dayLengthInMinutes * 60f;

        // If no gradient is set in the inspector, create a simple fallback
        if (sunColorOverDay == null)
        {
            sunColorOverDay = new Gradient();
        }

        // If no curve is set, make a simple "on during the day, off at night" curve
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

        // Advance timeOfDay 0 → 1
        timeOfDay += Time.deltaTime / dayLengthInSeconds;
        if (timeOfDay > 1f)
            timeOfDay -= 1f;

        // 1) Rotate the sun around the scene
        // 0 = sunrise horizon, 0.5 = sunset horizon, etc.
        float sunAngle = timeOfDay * 360f - 90f; // -90 so 0 starts at horizon
        sun.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // 2) Set sun color from gradient
        if (sunColorOverDay != null)
        {
            sun.color = sunColorOverDay.Evaluate(timeOfDay);
        }

        // 3) Set sun intensity from curve
        float intensityMultiplier = sunIntensityOverDay.Evaluate(timeOfDay);
        sun.intensity = sunBaseIntensity * intensityMultiplier;

        // 4) Optional: match ambient light a bit
        RenderSettings.ambientLight = sun.color * 0.3f;
    }
}
