using System.Collections.Generic;
using UnityEngine;
namespace Kitchen.UIElements{
public class CircleMeshBuilder : MeshBuilder
{
    [Header("Mesh Options")]
    [SerializeField]
    [Min(0)]
    private int trianglesPerRad = 5;

    override protected List<Vector3> CalculateVertices()
    {
        var triangleCount = GetTriangleCount();
        var vertices = new List<Vector3>();

        vertices.Add(Vector2.zero);

        for (int i = 0; i < triangleCount; i++)
        {
            float theta = i / (float)triangleCount * 2 * Mathf.PI;
            var vertex = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0);
            vertices.Add(vertex);
        }

        return vertices;
    }

    override protected List<int> CalculateTriangles()
    {
        var triangleCount = GetTriangleCount(); 
        var triangles = new List<int>();
        
        for (int i = 0; i < triangleCount; i++)
        {
            int index0 = 0;
            int index1 = i + 1;
            int index2 = i + 2;

            if(i == triangleCount - 1)
            {
                index2 = 1; //second vertex of last triangle is vertex1
            }

            triangles.Add(index0);
            triangles.Add(index2);
            triangles.Add(index1);
        }

        return triangles;
    }

    private int GetTriangleCount()
    {
        return Mathf.CeilToInt(2 * Mathf.PI * trianglesPerRad);
    }
}
}