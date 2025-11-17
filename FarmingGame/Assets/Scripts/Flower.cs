using UnityEngine;

public class Flower : MonoBehaviour
{
    public float pollenAmount = 1f; // for later gameplay use

    private void OnEnable()
    {
        if (FlowerManager.Instance != null)
            FlowerManager.Instance.allFlowers.Add(this);
    }

    private void OnDisable()
    {
        if (FlowerManager.Instance != null)
            FlowerManager.Instance.allFlowers.Remove(this);
    }
}