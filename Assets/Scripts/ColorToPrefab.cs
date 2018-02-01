using UnityEngine;
using System.Collections;

[System.Serializable]
public class ColorToPrefab
{
    public string name;
    public int number;
    public Color color;
    public GameObject Prefab;
    public bool isWalkable = true;
    public float movementCost = 1;
}
