using UnityEngine;
using System.Collections;

public class BeeForager : MonoBehaviour
{
    public enum BeeState { Idle, FlyingToFlower, Pollinating, Returning }
    public BeeState state = BeeState.Idle;

    // Personality type for this bee – set per prefab
    public enum BeePersonality { Calm, Energetic, Sleepy, Diligent }

    [Header("References")]
    public Beehive homeHive;
    public Transform flowerTarget;
    private Flower targetFlower;

    [Header("Movement Settings")]
    public float flightSpeed = 3f;
    public float rotationSpeed = 6f;
    public float hoverAmplitude = 0.1f;
    public float hoverFrequency = 6f;

    [Header("Pollination Settings")]
    public float pollinationDuration = 3f;

    [Header("Personality (set on prefab)")]
    public BeePersonality personality = BeePersonality.Calm;

    [Tooltip("Multiplier applied to flightSpeed based on this bee type.")]
    public float flightSpeedMultiplier = 1f;

    [Tooltip("Multiplier applied to pollinationDuration based on this bee type.")]
    public float pollinationDurationMultiplier = 1f;

    [Tooltip("Multiplier applied to pollen reward based on this bee type.")]
    public float pollenYieldMultiplier = 1f;

    private Vector3 homePosition;

    private void Start()
    {
        if (homeHive != null)
            homePosition = homeHive.transform.position;

        // No random personality here – everything comes from the prefab.
        // You can still add logic later if you want.
    }

    // Called by the hive
    public void SetFlower(Transform flower)
    {
        flowerTarget = flower;

        // Cache the Flower component & reserve a slot
        targetFlower = flower != null ? flower.GetComponent<Flower>() : null;
        if (targetFlower != null)
        {
            targetFlower.RegisterBee();
        }

        FlyToFlower();
    }

    public void FlyToFlower()
    {
        if (flowerTarget == null) return;

        state = BeeState.FlyingToFlower;
        StopAllCoroutines();
        StartCoroutine(FlyRoutine(flowerTarget.position, BeeState.Pollinating));
    }

    public void ReturnToHive()
    {
        state = BeeState.Returning;
        StopAllCoroutines();
        StartCoroutine(FlyRoutine(homePosition, BeeState.Idle));
    }

    private IEnumerator FlyRoutine(Vector3 destination, BeeState nextState)
    {
        destination.y += 0.5f; // always stay above ground

        while (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            Vector3 dir = (destination - transform.position).normalized;

            // Use personality-adjusted speed
            float currentSpeed = flightSpeed * flightSpeedMultiplier;
            transform.position += dir * currentSpeed * Time.deltaTime;

            // Smooth rotation toward movement direction
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Time.deltaTime * rotationSpeed
                );
            }

            // Hover bob
            transform.position += Vector3.up *
                Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude * Time.deltaTime;

            yield return null;
        }

        // Arrived at destination
        state = nextState;

        if (nextState == BeeState.Idle || flowerTarget == null)
        {
            // Bee has returned to hive → destroy it
            Destroy(gameObject);
            yield break;
        }

        if (nextState == BeeState.Pollinating)
        {
            StartCoroutine(PollinateRoutine());
        }
    }

    private IEnumerator PollinateRoutine()
    {
        float timer = 0f;
        Vector3 basePos = transform.position;

        // Personality-adjusted pollination time
        float duration = pollinationDuration * pollinationDurationMultiplier;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.position = basePos + Vector3.up *
                Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            yield return null;
        }

        // Reward hive with personality-adjusted pollen
        float baseReward = Random.Range(0.1f, 0.3f);
        homeHive.storedPollen += baseReward * pollenYieldMultiplier;

        // We are done with this flower: free up its slot
        if (targetFlower != null)
        {
            targetFlower.UnregisterBee();
            targetFlower = null;
        }

        ReturnToHive();
    }

    private void OnDestroy()
    {
        // Safety net: if something destroys the bee unexpectedly,
        // make sure we release the slot on the flower.
        if (targetFlower != null)
        {
            targetFlower.UnregisterBee();
            targetFlower = null;
        }
    }
}
