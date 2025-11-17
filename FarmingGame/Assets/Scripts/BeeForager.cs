using UnityEngine;
using System.Collections;

public class BeeForager : MonoBehaviour
{
    public enum BeeState { Idle, FlyingToFlower, Pollinating, Returning }
    public BeeState state = BeeState.Idle;

    [Header("References")]
    public Beehive homeHive;
    public Transform flowerTarget;

    [Header("Movement Settings")]
    public float flightSpeed = 3f;
    public float rotationSpeed = 6f;
    public float hoverAmplitude = 0.1f;
    public float hoverFrequency = 6f;

    [Header("Pollination Settings")]
    public float pollinationDuration = 3f;

    private Vector3 homePosition;

    private void Start()
    {
        if (homeHive != null)
            homePosition = homeHive.transform.position;
    }

    // Called by the hive
    public void SetFlower(Transform flower)
    {
        flowerTarget = flower;
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

            // --- FIX: Move directly toward destination, NOT forward ---
            transform.position += dir * flightSpeed * Time.deltaTime;

            // --- Smooth rotation toward movement direction ---
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

        while (timer < pollinationDuration)
        {
            timer += Time.deltaTime;
            transform.position = basePos + Vector3.up *
                Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            yield return null;
        }

        // Reward hive
        homeHive.storedPollen += Random.Range(0.1f, 0.3f);

        ReturnToHive();
    }
}
