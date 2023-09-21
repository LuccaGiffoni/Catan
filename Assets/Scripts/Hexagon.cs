using System.Collections.Generic;
using UnityEngine;

public class Hexagon : MonoBehaviour
{
    public Material Material { get; private set; }
    public float InnerSize { get; private set; }
    public float OuterSize { get; private set; }
    public float Height { get; private set; }
    public bool IsFlatTopped { get; private set; }

    public Hexagon(Material material, float innerSize, float outerSize, float height, bool isFlatTopped)
    {
        Material = material;
        InnerSize = innerSize;
        OuterSize = outerSize;
        Height = height;
        IsFlatTopped = isFlatTopped;
    }

    public void SetMaterial(Material material) {  Material = material; }
}
