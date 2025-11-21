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

            // ONLY consider flowers that still have free slots
            if (!flower.HasFreeSlot)
                continue;

            float sqrDist = (flower.transform.position - fromPosition).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = flower;
            }
        }

        // If every flower was full, this will be null
        return closest;
    }



}
