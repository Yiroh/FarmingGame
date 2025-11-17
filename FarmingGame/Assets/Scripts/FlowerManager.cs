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
}
