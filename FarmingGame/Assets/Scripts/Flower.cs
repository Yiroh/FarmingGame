using UnityEngine;

public class Flower : MonoBehaviour
{
    [Header("Flower Properties")]
    [Tooltip("Total amount of pollen this flower can provide in its lifetime.")]
    public float pollenAmount = 1f;

    [Header("Flower Stats")]
    [Tooltip("Age of this flower in in-game days (approx).")]
    public float ageDays = 0f;

    [Tooltip("Maximum number of bees that can target this flower at once.")]
    public int maxConcurrentBees = 3;

    [Tooltip("Current number of bees that are assigned to this flower.")]
    [SerializeField] private int currentBees = 0;

    [Tooltip("How many bees have ever used this flower.")]
    public int totalBeeVisits = 0;

    /// <summary>
    /// True if another bee is allowed to path to this flower.
    /// </summary>
    public bool HasFreeSlot => currentBees < maxConcurrentBees;

    /// <summary>
    /// Read-only access for debugging/UI if you need it.
    /// </summary>
    public int CurrentBeeCount => currentBees;

    private void Update()
    {
        // Simple real-time aging: 60 real seconds = 1 "day". Adjust divisor as you like.
        ageDays += Time.deltaTime / 60f;
    }

    /// <summary>
    /// Called when a bee reserves this flower as its target.
    /// </summary>
    public void RegisterBee()
    {
        currentBees = Mathf.Clamp(currentBees + 1, 0, maxConcurrentBees);
        totalBeeVisits++;
    }

    /// <summary>
    /// Called when a bee is done with this flower.
    /// </summary>
    public void UnregisterBee()
    {
        currentBees = Mathf.Max(0, currentBees - 1);
    }
}
