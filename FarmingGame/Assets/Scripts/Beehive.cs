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
    public int workerBees = 4;

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

    // ------------------------------------------------------------
    // Worker bee roster – the "vector" of bees this hive reuses
    // ------------------------------------------------------------

    [Header("Worker Bee Roster")]
    [Tooltip("How many individual worker bees this hive tracks for foraging.")]
    public int visibleWorkerCount = 4;

    [System.Serializable]
    public struct HiveBee
    {
        public string name;         // e.g. Harold, Jenna, Lily, Tyrone
        public GameObject prefab;   // specific bee prefab (personality + stats)
    }

    [Tooltip("Per-hive vector of worker bees. Filled at runtime.")]
    public HiveBee[] workerBeeRoster;

    [Tooltip("Pool of possible bee prefabs (different personalities/stats).")]
    public GameObject[] beePrefabsPool;

    private static readonly string[] DefaultBeeNames = new string[]
    {
        "Harold", "Jenna", "Lily", "Tyrone",
        "Sunny", "Maple", "Peach", "Moss",
        "Poppy", "Willow", "Clover", "Hazel"
    };

    [Header("Bee Spawning")]
    [Tooltip("How often this hive sends out a forager (seconds).")]
    public float spawnInterval = 3f;

    private float spawnTimer = 0f;

    // ---- Derived values used by UI ----

    public int TotalBees => workerBees + droneBees + broodBees;
    public float PopulationRatio => Mathf.Clamp01((float)TotalBees / maxPopulation);

    // ------------------------------------------------------------

    private void Awake()
    {
        InitializeWorkerRoster();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnBee();
        }
    }

    // Fill / validate the hive's bee roster
    private void InitializeWorkerRoster()
    {
        if (visibleWorkerCount <= 0)
            visibleWorkerCount = 4;

        // If already configured in inspector and looks valid, don't overwrite
        if (workerBeeRoster != null && workerBeeRoster.Length == visibleWorkerCount)
        {
            bool allSet = true;
            for (int i = 0; i < workerBeeRoster.Length; i++)
            {
                if (workerBeeRoster[i].prefab == null)
                {
                    allSet = false;
                    break;
                }
            }

            if (allSet)
                return;
        }

        workerBeeRoster = new HiveBee[visibleWorkerCount];

        var namePool = new System.Collections.Generic.List<string>(DefaultBeeNames);

        for (int i = 0; i < visibleWorkerCount; i++)
        {
            // Names
            if (namePool.Count == 0)
                namePool = new System.Collections.Generic.List<string>(DefaultBeeNames);

            int nameIndex = Random.Range(0, namePool.Count);
            string chosenName = namePool[nameIndex];
            namePool.RemoveAt(nameIndex);

            workerBeeRoster[i].name = chosenName;

            // Prefab type (personality/stats) for this bee
            if (beePrefabsPool != null && beePrefabsPool.Length > 0)
            {
                int prefabIndex = Random.Range(0, beePrefabsPool.Length);
                workerBeeRoster[i].prefab = beePrefabsPool[prefabIndex];
            }
            else
            {
                workerBeeRoster[i].prefab = null; // you can assign manually in inspector
            }
        }
    }

    // Sends out one forager based on the roster + closest flower
    private void SpawnBee()
    {
        if (FlowerManager.Instance == null) return;
        if (FlowerManager.Instance.allFlowers.Count == 0) return;

        if (workerBeeRoster == null || workerBeeRoster.Length == 0)
            InitializeWorkerRoster();

        // Find the closest flower that still has a free bee slot
        Flower closestFlower = FlowerManager.Instance.GetClosestFlower(transform.position);
        if (closestFlower == null)
        {
            // All flowers full → don't spawn a bee right now
            return;
        }

        // Pick one worker from this hive's roster in random order
        int index = Random.Range(0, workerBeeRoster.Length);
        HiveBee hiveBee = workerBeeRoster[index];

        if (hiveBee.prefab == null)
        {
            // No prefab assigned for this slot, nothing to spawn
            return;
        }

        Vector3 spawnPos = transform.position + Vector3.up * 1.5f; // lift above hive
        GameObject newBee = Instantiate(hiveBee.prefab, spawnPos, Quaternion.identity);

        BeeForager bee = newBee.GetComponent<BeeForager>();
        if (bee != null)
        {
            bee.homeHive = this;
            // If prefab has its own default name, keep it, otherwise use hive roster name
            if (!string.IsNullOrEmpty(hiveBee.name))
                bee.beeName = hiveBee.name;

            bee.SetFlower(closestFlower.transform);
        }
    }
}
