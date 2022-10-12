using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Kitchen.UIElements{
    /// <summary>
    /// http://www.code-spot.co.za/2020/11/04/generating-meshes-procedurally-in-unity/
    /// </summary>
    public class RadialMeshBuilder : MeshBuilder
    {
        [Header("Mesh Options")]
        [SerializeField]
        private bool FlipNormals;
        
        [SerializeField]
        private bool AsBar;

        [SerializeField]
        private bool AsSquare;
        
        [SerializeField]
        [Min(0)]
        private int NumSteps = 32;
        
        [SerializeField]
        [Min(0)]
        private float Height = 20;

        [SerializeField]
        [Min(0)]
        private float Width = 20;

        [SerializeField]
        [Min(0)]
        private int BorderSize = 5;

        [Header("Radial Options")]
        [SerializeField]
        [RangeAttribute(0, 100)]
        private int Percent;
        
        [SerializeField]
        private bool InvertPercent;

        private RadialMesh mesh;

        override protected List<Vector3> CalculateVertices()
        {
            mesh = new RadialMesh(NumSteps, FlipNormals);
            mesh.Width = Width;
            mesh.Height = Height;
            mesh.BorderSize = BorderSize;
            mesh.AsBar = AsBar;
            mesh.AsSquare = AsSquare;
            mesh.IsDirty = true;
            mesh.UpdateMesh();

            IEnumerable<Vector3> vertices = mesh.GetVertices(InvertPercent);
            
            return vertices.ToList();
        }

        override protected List<int> CalculateTriangles()
        {
            IEnumerable<int> testIndices = mesh.GetIndices(Percent, InvertPercent).Select(x => (int)x);
            
            return testIndices.ToList();
        }
        
        private int GetTriangleCount()
        {
            return NumSteps;
        }
    }
}