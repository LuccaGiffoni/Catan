using NUnit.Framework;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
//[RequireComponent(typeof(Rigidbody), typeof(MeshCollider))]
public class HexagonMeshBuilder : MonoBehaviour
{
    public Material HexagonMaterial { get; private set; }
    public float InnerSize { get; private set; } = 0.0f;
    public float OuterSize { get; private set; } = 1.0f;
    public float Height { get; private set; } = 0.2f;
    public bool IsFlatTopped { get; private set; } = false;

    private List<Face> hexagonFaces;

    private Mesh hexagonMesh;
    private MeshRenderer hexagonRenderer;
    private MeshFilter hexagonFilter;
    private MeshCollider hexagonCollider;
    private Rigidbody hexagonRigidbody;

    public void ConfigureHexagon(float innerSize, float outerSize,float height, bool flat)
    {
        InnerSize = innerSize;
        OuterSize = outerSize;
        Height = height;
        IsFlatTopped = flat;
    }

    private void Awake()
    {
        hexagonFilter = GetComponent<MeshFilter>();
        hexagonRenderer = GetComponent<MeshRenderer>();
        hexagonRigidbody = GetComponent<Rigidbody>();
        hexagonCollider = GetComponent<MeshCollider>();

        hexagonMesh = new()
        {
            name = "HexagonMesh"
        };

        hexagonFilter.mesh = hexagonMesh;
        hexagonRenderer.material = HexagonMaterial;
        //hexagonRigidbody.isKinematic = true;
        //hexagonCollider.convex = true;
    }

    private void OnEnable()
    {
        GenerateMesh();
        FindMiddleVertex();
    }

    public void GenerateMesh()
    {
        GenerateFaces();
        CombineFaces();
    }

    public void SetMaterial(Material newMaterial)
    {
        hexagonRenderer.material = newMaterial;
    }   
    
    public Material GetMaterial()
    {
        return hexagonRenderer.material;
    }

    private void FindMiddleVertex()
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 pointA = GetPoint(OuterSize, Height / 2f, i);
            Vector3 pointB = GetPoint(OuterSize, Height / 2f, (i < 5) ? i + 1 : 0);
            Vector3 middlePoint = (pointA + pointB) / 2f;

            GameObject emptyObject = new($"MiddlePoint.{i}");
            emptyObject.transform.position = middlePoint;
            emptyObject.transform.SetParent(transform);
        }
    }

    private void GenerateFaces()
    {
        hexagonFaces = new();

        for (int point = 0; point < 6; point++)
        {
            hexagonFaces.Add(CreateFace(InnerSize, OuterSize, Height / 2f, Height / 2f, point));
        }

        for (int point = 0; point < 6; point++)
        {
            hexagonFaces.Add(CreateFace(InnerSize, OuterSize, -Height / 2f, -Height / 2f, point, true));
        }

        for (int point = 0; point < 6; point++)
        {
            hexagonFaces.Add(CreateFace(OuterSize, OuterSize, Height / 2f, -Height / 2f, point, true));
        }

        for (int point = 0; point < 6; point++)
        {
            hexagonFaces.Add(CreateFace(InnerSize, InnerSize, Height / 2f, -Height / 2f, point, false));
        }
    }

    private Face CreateFace(float innerRad, float outerRad, float heightA, float heightB,
        int point, bool reverse = false)
    {
        Vector3 pointA = GetPoint(innerRad, heightB, point);
        Vector3 pointB = GetPoint(innerRad, heightB, (point < 5) ? point + 1 : 0);
        Vector3 pointC = GetPoint(outerRad, heightA, (point < 5) ? point + 1 : 0);
        Vector3 pointD = GetPoint(outerRad, heightA, point);

        List<Vector3> vertices = new() { pointA, pointB, pointC, pointD };
        List<int> triangles = new() { 0, 1, 2, 2, 3, 0 };
        List<Vector2> uvs = new() { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };

        if (reverse)
        {
            vertices.Reverse();
        }
        
        return new Face(vertices, triangles, uvs);
    }

    private Vector3 GetPoint(float size, float height, int index)
    {
        float angleDegree = IsFlatTopped ? 60 * index : 60 * index - 30;
        float angleRadians = Mathf.PI / 180f * angleDegree;
        
        return new Vector3((size * Mathf.Cos(angleRadians)), height, size * Mathf.Sin(angleRadians));
    }

    private void CombineFaces()
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector2> uvs = new();

        for (int i = 0; i < hexagonFaces.Count; i++)
        {
            vertices.AddRange(hexagonFaces[i].Vertices);
            uvs.AddRange(hexagonFaces[i].UVs);

            int offset = (4 * i);
            foreach(int triangle in hexagonFaces[i].Triangles)
            {
                triangles.Add(triangle + offset);
            }
        }

        hexagonMesh.vertices = vertices.ToArray();
        hexagonMesh.triangles = triangles.ToArray();
        hexagonMesh.uv = uvs.ToArray();
        hexagonMesh.RecalculateNormals();
    }
}
