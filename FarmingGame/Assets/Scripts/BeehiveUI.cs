using UnityEngine;
using TMPro;

public class BeehiveUI : MonoBehaviour
{
    public static BeehiveUI Instance;

    [Header("UI Roots")]
    public GameObject panelRoot;

    [Header("Text Fields")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI queenText;
    public TextMeshProUGUI temperamentText;
    public TextMeshProUGUI issuesText;
    public TextMeshProUGUI resourcesText;

    private Beehive currentHive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Start hidden
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void ShowHive(Beehive hive)
    {
        currentHive = hive;
        if (panelRoot != null)
            panelRoot.SetActive(true);

        Refresh();
    }

    public void Hide()
    {
        currentHive = null;
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        if (currentHive == null) return;

        // Title
        if (titleText != null)
            titleText.text = $"{currentHive.hiveName} ({currentHive.hiveType})";

        // Health
        if (healthText != null)
            healthText.text = $"Health: {currentHive.hiveHealth:0}%";

        // Population
        if (populationText != null)
        {
            populationText.text =
                $"Workers: {currentHive.workerBees}\n" +
                $"Drones: {currentHive.droneBees}\n" +
                $"Brood: {currentHive.broodBees}\n" +
                $"Total: {currentHive.TotalBees}";
        }

        // Queen
        if (queenText != null)
        {
            if (currentHive.hasQueen)
            {
                queenText.text =
                    $"Queen: Yes\n" +
                    $"Age: {currentHive.queenAgeDays:0} days\n" +
                    $"Productivity: {currentHive.queenProductivity:0.0}x";
            }
            else
            {
                queenText.text = "Queen: Missing";
            }
        }

        // Temperament / yield
        if (temperamentText != null)
        {
            temperamentText.text =
                $"Aggression: {currentHive.aggression:0}\n" +
                $"Calmness: {currentHive.calmness:0}\n" +
                $"Yield Multiplier: {currentHive.honeyYieldPotential:0.0}x";
        }

        // Issues
        if (issuesText != null)
        {
            issuesText.text =
                $"Mites: {currentHive.miteLevel:0}\n" +
                $"Disease: {currentHive.diseaseLevel:0}\n" +
                $"Moisture: {currentHive.moistureLevel:0}";
        }

        // Stored resources
        if (resourcesText != null)
        {
            resourcesText.text =
                $"Honey: {currentHive.storedHoney:0.0}\n" +
                $"Pollen: {currentHive.storedPollen:0.0}\n" +
                $"Wax: {currentHive.storedWax:0.0}\n" +
                $"Propolis: {currentHive.storedPropolis:0.0}";
        }
    }
}
