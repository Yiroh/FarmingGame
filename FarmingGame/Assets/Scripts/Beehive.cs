using UnityEngine;

public enum HiveType
{
    Basic,
    Langstroth,
    Flow,
    TopBar
}

public class Beehive : MonoBehaviour
{
    [Header("Identity")]
    public string hiveName = "Beehive";
    public HiveType hiveType = HiveType.Basic;

    [Header("Core State")]
    [Tooltip("0 = dead hive, 100 = perfect health.")]
    [Range(0f, 100f)] public float hiveHealth = 100f;

    [Tooltip("Is there a living, laying queen in this hive?")]
    public bool hasQueen = true;

    [Tooltip("Age of the queen in days.")]
    public float queenAgeDays = 0f;

    [Tooltip("Queen productivity multiplier (0 = no laying, 1 = normal, >1 = very strong).")]
    [Range(0f, 3f)] public float queenProductivity = 1f;

    [Header("Population")]
    [Tooltip("Number of worker bees in this hive.")]
    public int workerBees = 5000;

    [Tooltip("Number of drone bees in this hive.")]
    public int droneBees = 300;

    [Tooltip("Number of brood (eggs/larvae/pupae).")]
    public int broodBees = 1000;

    [Tooltip("Maximum bees this hive can comfortably support.")]
    public int maxPopulation = 20000;

    [Header("Hive Temperament")]
    [Tooltip("0 = totally calm, 100 = extremely aggressive.")]
    [Range(0f, 100f)] public float aggression = 20f;

    [Tooltip("0 = very unstable, 100 = very stable/forgiving.")]
    [Range(0f, 100f)] public float calmness = 60f;

    [Tooltip("Base honey yield potential of this hive (multiplier).")]
    [Range(0.1f, 3f)] public float honeyYieldPotential = 1f;

    [Header("Health Issues")]
    [Tooltip("0 = no mites, 100 = catastrophic infestation.")]
    [Range(0f, 100f)] public float miteLevel = 0f;

    [Tooltip("0 = healthy, 100 = severe disease.")]
    [Range(0f, 100f)] public float diseaseLevel = 0f;

    [Tooltip("0 = very dry, 100 = flooded/condensation everywhere.")]
    [Range(0f, 100f)] public float moistureLevel = 20f;

    [Header("Stored Resources (in the hive, not player inventory)")]
    public float storedHoney = 0f;     // kg or arbitrary units
    public float storedPollen = 0f;
    public float storedWax = 0f;
    public float storedPropolis = 0f;

    [Header("Environment Modifiers (for future use)")]
    [Tooltip("How good the surrounding flowers are (0–100).")]
    [Range(0f, 100f)] public float localFlowerQuality = 50f;

    [Tooltip("Local climate suitability (0–100).")]
    [Range(0f, 100f)] public float localClimateSuitability = 70f;

    [Header("Bee Spawning")]
    public GameObject beePrefab;
    public int beesToSpawn = 1;
    public float spawnInterval = 3f;

    private float spawnTimer = 0f;

    // ---- Derived values / helpers ----

    public int TotalBees => workerBees + droneBees + broodBees;

    public float PopulationRatio => Mathf.Clamp01((float)TotalBees / maxPopulation);


    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnBee();
        }
    }

    // Bee Spawning
    private void SpawnBee()
    {
        if (beePrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 1.5f; // lift above hive

        GameObject newBee = Instantiate(beePrefab, spawnPos, Quaternion.identity);

        BeeForager bee = newBee.GetComponent<BeeForager>();
        bee.homeHive = this;

        Flower randomFlower = FlowerManager.Instance.GetRandomFlower();
        if (randomFlower != null)
            bee.SetFlower(randomFlower.transform);
    }

    // ---- Example helper methods to use later in gameplay ----

    public void ReplaceQueen()
    {
        hasQueen = true;
        queenAgeDays = 0f;
        queenProductivity = 1f;
        // You could also slightly lower aggression when replacing with a gentle queen
    }

    public void AddBees(int workers, int drones, int brood = 0)
    {
        workerBees = Mathf.Max(0, workerBees + workers);
        droneBees = Mathf.Max(0, droneBees + drones);
        broodBees = Mathf.Max(0, broodBees + brood);

        // Clamp to max population
        int total = TotalBees;
        if (total > maxPopulation)
        {
            float scale = (float)maxPopulation / total;
            workerBees = Mathf.RoundToInt(workerBees * scale);
            droneBees  = Mathf.RoundToInt(droneBees * scale);
            broodBees  = Mathf.RoundToInt(broodBees * scale);
        }
    }

    public void DamageHive(float amount)
    {
        hiveHealth = Mathf.Clamp(hiveHealth - amount, 0f, 100f);
    }

    public void HealHive(float amount)
    {
        hiveHealth = Mathf.Clamp(hiveHealth + amount, 0f, 100f);
    }

    public void ApplyMiteTreatment(float effectiveness)
    {
        // effectiveness = 0–1 fraction of mites removed
        effectiveness = Mathf.Clamp01(effectiveness);
        miteLevel = Mathf.Clamp(miteLevel * (1f - effectiveness), 0f, 100f);
    }

    public void ApplyDiseaseTreatment(float effectiveness)
    {
        effectiveness = Mathf.Clamp01(effectiveness);
        diseaseLevel = Mathf.Clamp(diseaseLevel * (1f - effectiveness), 0f, 100f);
    }

    public void AdjustMoisture(float delta)
    {
        moistureLevel = Mathf.Clamp(moistureLevel + delta, 0f, 100f);
    }

    // You can call this once per in-game day later
    public void DailyUpdate(float daysPassed = 1f)
    {
        // Queen ages
        queenAgeDays += daysPassed;

        // Simple example: very old queen becomes less productive & hive more aggressive
        if (queenAgeDays > 365f) // older than 1 year
        {
            queenProductivity = Mathf.Clamp(queenProductivity - 0.05f * daysPassed, 0.2f, 1.5f);
            aggression = Mathf.Clamp(aggression + 2f * daysPassed, 0f, 100f);
        }

        // Untreated mites/disease slowly hurt health
        float healthPenalty = (miteLevel + diseaseLevel) * 0.01f * daysPassed;
        DamageHive(healthPenalty);
    }
}
