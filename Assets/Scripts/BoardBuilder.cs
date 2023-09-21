using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.TerrainTools;
using UnityEngine;

public class BoardBuilder : MonoBehaviour
{
    [Header("Lands Materials")]
    [SerializeField] private Material sea;
    [SerializeField] private Material desert;

    [Header("Resources Materials")]
    [SerializeField] private Material manna;
    [SerializeField] private Material fish;
    [SerializeField] private Material rock;
    [SerializeField] private Material oak;
    [SerializeField] private Material clay;

    [Header("Hexagon Settings")]
    [SerializeField] public Material hexagonMaterial;
    [SerializeField] public float innerSize = 0f;
    [SerializeField] public float outerSize = 1f;
    [SerializeField] public float height = 0.1f;
    [SerializeField] public bool isFlatTopped = true;
    [SerializeField] public float spacing;

    [Header("Roads")]
    [SerializeField] private Transform roads;
    [SerializeField] private GameObject roadGameObject;

    private void OnEnable()
    {
        LayoutGrid();
    }

    private Material[] CreateMaterialList()
    {
        return new Material[]
        {
            oak, oak, oak, oak,
            fish, fish, fish, fish,
            manna, manna, manna, manna,
            rock, rock, rock,
            clay, clay, clay
        };
    }

    private void ShuffleMaterials(Material[]  materials)
    {
        for (int i = materials.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (materials[randomIndex], materials[i]) = (materials[i], materials[randomIndex]);
        }
    }

    private void ClearGameObject()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < roads.childCount; i++)
        {
            DestroyImmediate(roads.GetChild(i).gameObject);
        }
    }

    private void LayoutGrid()
    {
        ClearGameObject();

        Material[] materials = CreateMaterialList();
        ShuffleMaterials(materials);

        Dictionary<Material, int> materialLimits = new Dictionary<Material, int>();
        foreach (Material material in materials)
        {
            if (!materialLimits.ContainsKey(material))
            {
                materialLimits[material] = 1;
            }
            else
            {
                materialLimits[material]++;
            }
        }

        Dictionary<Material, int> materialCounts = new Dictionary<Material, int>(materialLimits.Keys.Count);
        foreach (Material key in materialLimits.Keys)
        {
            materialCounts[key] = 0;
        }

        int[] ringCounts = { 1, 6, 12, 18 };
        int hexagonIndex = 0;

        Material previousMaterial = null;

        for (int ring = 0; ring < 4; ring++)
        {
            int count = ringCounts[ring];

            for (int i = 0; i < count; i++)
            {
                Vector2Int coordinate = GetHexagonCoordinate(ring, i);
                GameObject land = new($"Land {hexagonIndex}", typeof(HexagonMeshBuilder));
                land.transform.position = GetPositionForHexagonFromCoordinate(coordinate);

                HexagonMeshBuilder builder = land.GetComponent<HexagonMeshBuilder>();
                builder.ConfigureHexagon(innerSize, outerSize, height, isFlatTopped);

                if (ring == 0)
                {
                    builder.SetMaterial(desert);
                }
                else if (ring == 1 || ring == 2)
                {
                    Material selectedMaterial;
                    do
                    {
                        selectedMaterial = materials[Random.Range(0, materials.Length)];
                    } while (!IsMaterialValidForCoordinate(selectedMaterial, GetHexagonCoordinate(ring, i)) || 
                        materialCounts[selectedMaterial] >= materialLimits[selectedMaterial] || 
                        selectedMaterial == previousMaterial);

                    builder.SetMaterial(selectedMaterial);
                    materialCounts[selectedMaterial]++;
                    previousMaterial = selectedMaterial;
                }
                else if (ring == 3)
                {
                    builder.SetMaterial(sea);
                }

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
                            CreateRoadPointsBetweenVertices(coordA, coordC);
                        }
                        else
                        {
                            Vector2Int coordC = GetHexagonCoordinate(ring - 1, (i / ring) * (ring - 1) - 1);
                            CreateRoadPointsBetweenVertices(coordA, coordC);
                        }
                    }
                    else
                    {
                        CreateRoadPointsBetweenVertices(coordA, coordB);
                    }
                }

                land.transform.SetParent(transform, true);
                hexagonIndex++;
            }
        }
    }

    private void CreateRoadPointsBetweenVertices(Vector2Int coordA, Vector2Int coordB)
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

    private bool IsMaterialValidForCoordinate(Material material, Vector2Int coordinate)
    {
        Vector2Int[] neighbors = new Vector2Int[]
        {
        coordinate + new Vector2Int(1, 0),
        coordinate + new Vector2Int(-1, 0),
        coordinate + new Vector2Int(0, 1),
        coordinate + new Vector2Int(0, -1),
        coordinate + new Vector2Int(1, -1),
        coordinate + new Vector2Int(-1, 1)
        };

        foreach (Vector2Int neighbor in neighbors)
        {
            GameObject neighborObject = FindHexagonByCoordinate(neighbor);
            if (neighborObject != null)
            {
                HexagonMeshBuilder neighborBuilder = neighborObject.GetComponent<HexagonMeshBuilder>();
                if (neighborBuilder.GetMaterial() == material)
                {
                    return false;
                }

                // Check if the neighbor's neighbors have the same material
                Vector2Int[] neighborNeighbors = new Vector2Int[]
                {
                neighbor + new Vector2Int(1, 0),
                neighbor + new Vector2Int(-1, 0),
                neighbor + new Vector2Int(0, 1),
                neighbor + new Vector2Int(0, -1),
                neighbor + new Vector2Int(1, -1),
                neighbor + new Vector2Int(-1, 1)
                };

                foreach (Vector2Int neighborNeighbor in neighborNeighbors)
                {
                    GameObject neighborNeighborObject = FindHexagonByCoordinate(neighborNeighbor);
                    if (neighborNeighborObject != null)
                    {
                        HexagonMeshBuilder neighborNeighborBuilder = neighborNeighborObject.GetComponent<HexagonMeshBuilder>();
                        if (neighborNeighborBuilder.GetMaterial() == material)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }


    private GameObject FindHexagonByCoordinate(Vector2Int coordinate)
    {
        foreach (Transform child in transform)
        {
            if (GetPositionForHexagonFromCoordinate(coordinate) == child.position)
            {
                return child.gameObject;
            }
        }

        return null;
    }
}
