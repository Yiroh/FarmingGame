using UnityEngine;
using System.Collections.Generic;

public class FlowerManager : MonoBehaviour
{
    public static FlowerManager Instance;

    public List<Flower> allFlowers = new List<Flower>();

    private void Awake()
    {
        Instance = this;
    }

    public Flower GetRandomFlower()
    {
        if (allFlowers.Count == 0) return null;
        return allFlowers[Random.Range(0, allFlowers.Count)];
    }
    public Flower GetClosestFlower(Vector3 fromPosition)
    {
        if (allFlowers.Count == 0) return null;

        Flower closest = null;
        float closestSqrDist = Mathf.Infinity;

        foreach (var flower in allFlowers)
        {
            if (flower == null) continue;

            float sqrDist = (flower.transform.position - fromPosition).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = flower;
            }
        }

        return closest;
    }

}
