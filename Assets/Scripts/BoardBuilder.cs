using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardBuilder : MonoBehaviour
{
    [Header("Hexagon Settings")]
    [SerializeField] private Material hexagonMaterial;
    [SerializeField] private float innerSize = 0f;
    [SerializeField] private float outerSize = 1f;
    [SerializeField] private float height = 0.1f;
    [SerializeField] private bool isFlatTopped = true;
    [SerializeField] private float spacing;

    [Header("Roads")]
    [SerializeField] private Transform roads;
    [SerializeField] private GameObject roadGameObject;

    private void OnEnable()
    {
        LayoutGrid();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) LayoutGrid();
    }

    private void LayoutGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < roads.childCount; i++)
        {
            DestroyImmediate(roads.GetChild(i).gameObject);
        }

        int[] ringCounts = { 1, 6, 12 };
        int hexagonIndex = 0;

        for (int ring = 0; ring < 3; ring++)
        {
            int count = ringCounts[ring];

            for (int i = 0; i < count; i++)
            {
                Vector2Int coordinate = GetHexagonCoordinate(ring, i);
                GameObject land = new($"Land {hexagonIndex}", typeof(HexagonMeshBuilder));
                land.transform.position = GetPositionForHexagonFromCoordinate(coordinate);

                HexagonMeshBuilder builder = land.GetComponent<HexagonMeshBuilder>();
                builder.ConfigureHexagon(innerSize, outerSize, height, isFlatTopped);
                builder.SetMaterial(hexagonMaterial);
                builder.GenerateMesh();

                if (ring > 0)
                {
                    Vector2Int coordA = GetHexagonCoordinate(ring, i);
                    Vector2Int coordB = GetHexagonCoordinate(ring - 1, i / ring * (ring - 1));

                    if (i % ring == 0)
                    {
                        if (ring == 2 && i == count - 1)
                        {
                            Vector2Int coordC = GetHexagonCoordinate(ring - 1, 0);
                            CreateEmptyGameObjectBetweenHexagons(coordA, coordC);
                        }
                        else
                        {
                            Vector2Int coordC = GetHexagonCoordinate(ring - 1, (i / ring) * (ring - 1) - 1);
                            CreateEmptyGameObjectBetweenHexagons(coordA, coordC);
                        }
                    }
                    else
                    {
                        CreateEmptyGameObjectBetweenHexagons(coordA, coordB);
                    }
                }

                land.transform.SetParent(transform, true);
                hexagonIndex++;
            }
        }
    }

    private void CreateEmptyGameObjectBetweenHexagons(Vector2Int coordA, Vector2Int coordB)
    {
        Vector3 positionA = GetPositionForHexagonFromCoordinate(coordA);
        Vector3 positionB = GetPositionForHexagonFromCoordinate(coordB);
        Vector3 middlePoint = (positionA + positionB) / 2f;

        GameObject emptyObject = new($"Road");
        //var road = Instantiate(roadGameObject, roads);
        //road.transform.position = middlePoint;
        emptyObject.transform.position = middlePoint;
        emptyObject.transform.SetParent(roads);
    }

    private Vector2Int GetHexagonCoordinate(int ring, int index)
    {
        if (ring == 0) return new Vector2Int(0, 0);

        int side = index / ring;
        int positionInSide = index % ring;

        int x = 0, y = 0;

        switch (side)
        {
            case 0:
                x = ring - positionInSide;
                y = positionInSide;
                break;
            case 1:
                x = -positionInSide;
                y = ring;
                break;
            case 2:
                x = -ring;
                y = ring - positionInSide;
                break;
            case 3:
                x = -ring + positionInSide;
                y = -positionInSide;
                break;
            case 4:
                x = positionInSide;
                y = -ring;
                break;
            case 5:
                x = ring;
                y = -ring + positionInSide;
                break;
        }

        return new Vector2Int(x, y);
    }

    private Vector3 GetPositionForHexagonFromCoordinate(Vector2Int coordinate)
    {
        float x = (coordinate.x + coordinate.y * 0.5f) * outerSize * Mathf.Sqrt(3f) * spacing;
        float z = coordinate.y * outerSize * 1.5f * spacing;
        return new Vector3(x, 0, -z);
    }
}
