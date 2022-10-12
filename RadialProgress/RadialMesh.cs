using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kitchen.UIElements{
    /// <summary>
    /// A radial mesh has a vertice in the center and triangles are created from it like a pie chart.
    /// If the radial is rendered as a border, there is no center and quads are created instead.
    /// https://rextester.com/BYFBT42714
    /// https://rextester.com/CYCBJ87813
    /// </summary>
    public class RadialMesh
    {
        private Vertex[] vertexes { get; set; }
        private ushort[] indices { get; set; }
        
        private List<Vector3> vertices {get; set;}
        private List<int> triangles {get; set;}

        public bool IsDirty { get => isDirty; set => isDirty = value; }
        public float BorderSize { get => borderSize; set => CompareAndWrite(ref borderSize, value); }
        public float Height { get => height; set => CompareAndWrite(ref height, value); }
        public float Width { get => width; set => CompareAndWrite(ref width, value); }
        public bool AsBar
        {
            get => renderInner;
            set { isDirty = value != renderInner; renderInner = value; }
        }
        public bool AsSquare
        {
            get => asSquare;
            set { isDirty = value != asSquare; asSquare = value; }
        }
        public int StepsPerQuarter
        {
            get => quarterSteps;
            set { isDirty = value != quarterSteps; quarterSteps = value; }
        }
        public Color Color { get => color; set { isDirty = value != color; color = value; } }

        private int quarterSteps;
        private int totalSteps => quarterSteps * 4;
        private bool isDirty;
        private float borderSize;
        private float height;
        private float width;
        private bool renderInner;
        private bool asSquare;
        private Color color;

        private int[] QuadIndiceMap;
        private int[] TriIndiceMap;
        private int[] FlipQuadIndiceMap;
        private int[] FlipTriIndiceMap;

        public RadialMesh(int numSteps, bool flipNormals = false)
        {
            StepsPerQuarter = numSteps;

            QuadIndiceMap = flipNormals ? ReverseSquareIndices : NormalSquareIndices;
            TriIndiceMap = flipNormals ? ReverseTriIndices : NormalTriIndices;
            
            FlipQuadIndiceMap = flipNormals ? NormalSquareIndices : ReverseSquareIndices;
            FlipTriIndiceMap = flipNormals ? NormalTriIndices : ReverseTriIndices;
        }

        public void UpdateMesh()
        {
            if (!isDirty)
            {
                Debug.Log("Not Dirty");
                return;
            }
            isDirty = false;

            vertices = new List<Vector3>();
            triangles = new List<int>();

            CalculateVertices();

            triangles =  AsBar ? CreateIndices(totalSteps, QuadIndiceMap) : CreateRadialIndices(totalSteps, TriIndiceMap);

            if (vertexes == null || vertexes.Length != vertices.Count)
                vertexes = new Vertex[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
                vertexes[i] = CreateVertex(vertices[i]);
            
            indices = triangles.Select(x => (ushort)x).ToArray();
        }
        private Vertex CreateVertex(Vector3 vect)
        {
            Vertex vertex = new Vertex();
            vertex.position = new Vector3(vect.x + Width, vect.y + Height, Vertex.nearZ);
            vertex.tint = Color;
            return vertex;
        }

        public IEnumerable<Vertex> GetVertexes(){
            return vertexes;
        }

        public IEnumerable<Vector3> GetVertices(bool reverse = false){
            IEnumerable<Vector3> returnVertices = vertices;
            if(reverse)
            {
                // Keep first two vertices so mesh starts at same location
                int startVertices = 2;
                returnVertices = returnVertices.Take(startVertices);
                if (AsBar) // rendering quads, reverse first vertices
                    returnVertices = returnVertices.Reverse();
                // if we have a center vertice it stays as the first element
                returnVertices = returnVertices.Concat(vertices.Skip(startVertices).Reverse());
            }
            return vertices;
        }

        protected List<Vector3> CalculateVertices()
        {
            vertices = new List<Vector3>();

            if (!AsBar)
                vertices.Add(Vector2.zero);

            if (AsSquare)
                SquareMesh();
            else
                CircleMesh();

            return vertices;
        }

        public ushort[] GetIndices(float percent = 100f, bool invertPercent = false)
        {
            float clampedProgress = Mathf.Clamp(percent, 0.0f, 100.0f);
            int steps = Mathf.FloorToInt(invertPercent ? totalSteps * (1 - (clampedProgress / 100.0f)) : (totalSteps * clampedProgress) / 100.0f);
            
            int indicesPerStep = AsBar ? 6 : 3;

            indicesPerStep *= steps;
            IEnumerable<ushort> returnIndices = indices;
            // With a center vertice we need to flip indices when we invert the vertices
            if(invertPercent && !AsBar){
                returnIndices = CreateRadialIndices(totalSteps, FlipTriIndiceMap).Select(x => (ushort)x);
            }

            return returnIndices.Take(indicesPerStep).ToArray();
        }
        
        public static int[] NormalSquareIndices = new [] { 0, 3, 1, 0, 2, 3 };
        public static int[] ReverseSquareIndices = new [] { 0, 1, 3, 0, 3, 2 };
        public static int[] NormalTriIndices = new [] { 0, 2, 1 };
        public static int[] ReverseTriIndices = new [] { 0, 1, 2 };
        private static List<int> CreateIndices(int steps, int[] indicesMap){
            int triPerMap = indicesMap.Length / 3;
            var indices = new List<int>();
            var total = steps * triPerMap;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Creating {total} Indices: Steps {steps}");
            for (int i = 0; i < total; i+=triPerMap)
            {
                for (int idx = 0; idx < indicesMap.Length; idx++)
                {
                    indices.Add((i + indicesMap[idx]) % total);
                    sb.Append($"{(i + indicesMap[idx]) % total} \t");
                }
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
            return indices;
        }
        /// <summary>
        /// Every triangle shares the vertice at index 0, which is at the center
        /// </summary>
        private static List<int> CreateRadialIndices(int triangles, int[] indicesMap){
            int triPerMap = indicesMap.Length / 3;
            var indices = new List<int>();
            var total = triangles * triPerMap;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < total; i+=triPerMap)
            {
                indices.Add(0);
                sb.Append($"0 \t");
                for (int idx = 1; idx < indicesMap.Length; idx++)
                {
                    int value = (i + indicesMap[idx]) % total;
                    // zero is reserved for center vertice
                    if(value == 0){
                        value = i + indicesMap[idx];
                    }
                    indices.Add(value);
                    sb.Append($"{value} \t");
                }
                sb.AppendLine($"");
            }
            Debug.Log(sb.ToString());
            return indices;
        }
        public void CircleMesh()
        {
            float stepSize = 360.0f / (float)totalSteps;
            float angle = 0.0f;

            for (int i = 0; i < totalSteps; ++i)
            {
                float radians = Mathf.Deg2Rad * angle;

                float outerPosX = Mathf.Sin(radians) * Width;
                float outerPosY = Mathf.Cos(radians) * Height;

                float innerPosX = Mathf.Sin(radians) * (Width - BorderSize);
                float innerPosY = Mathf.Cos(radians) * (Height - BorderSize);

                if (AsBar)
                {
                    vertices.Add(new Vector3(innerPosX, innerPosY, 0));
                }
                vertices.Add(new Vector3(outerPosX, outerPosY, 0));
                
                angle += stepSize;
            }
        }
        public void SquareMesh()
        {
            int index = 0;
            
            //float stepsPerSide = totalSteps / 4;
            float stepsPerSide = StepsPerQuarter;
            float posMax = (stepsPerSide / 2);
            float negMax = (-1 * posMax);

            float outerStep = (Height / posMax);
            float innerStep = ((Height - BorderSize) / posMax);

            float outerPosX, outerPosY = 0;
            float innerPosX, innerPosY = 0;

            // Top Middle -> Top Right
            //x 0, y max, move X to max
            outerPosY = Height;
            innerPosY = outerPosY - BorderSize;
            for (float x = 0, y = posMax; x < posMax; index++, x++)
            {
                outerPosX = x * outerStep;
                innerPosX = x * innerStep;

                if (AsBar)
                    vertices.Add(new Vector3(innerPosX, innerPosY, 0));

                vertices.Add(new Vector3(outerPosX, outerPosY, 0));
            }

            // Top Right -> Bottom Right
            // x max, y max, move y to -max
            outerPosX = Width;
            innerPosX = outerPosX - BorderSize;
            for (float x = posMax, y = posMax; y > negMax; index++, y--)
            {
                outerPosY = y * outerStep;
                innerPosY = y * innerStep;

                if (AsBar)
                    vertices.Add(new Vector3(innerPosX, innerPosY, 0));

                vertices.Add(new Vector3(outerPosX, outerPosY, 0));
            }
            // Bottom Right -> Bottom Left
            // x max, y -max, move x to -max
            outerPosY = (-1 * Height);
            innerPosY = outerPosY + BorderSize;
            for (float x = posMax, y = negMax; x > negMax; index++, x--)
            {
                outerPosX = x * outerStep;
                innerPosX = x * innerStep;

                if (AsBar)
                    vertices.Add(new Vector3(innerPosX, innerPosY, 0));

                vertices.Add(new Vector3(outerPosX, outerPosY, 0));
            }
            // Bottom left -> Top Left
            // x -max, y -max, move y to max
            outerPosX = (-1 * Width);
            innerPosX = outerPosX + BorderSize;
            for (float x = negMax, y = negMax; y < posMax; index++, y++)
            {
                outerPosY = y * outerStep;
                innerPosY = y * innerStep;

                if (AsBar)
                    vertices.Add(new Vector3(innerPosX, innerPosY, 0));

                vertices.Add(new Vector3(outerPosX, outerPosY, 0));
            }
            // Top left -> Top Middle
            // x -max, y max, move x to 0
            outerPosY = Height;
            innerPosY = outerPosY - BorderSize;
            for (float x = negMax, y = posMax; x < 0; index++, x++)
            {
                outerPosX = x * outerStep;
                innerPosX = x * innerStep;

                if (AsBar)
                    vertices.Add(new Vector3(innerPosX, innerPosY, 0));

                vertices.Add(new Vector3(outerPosX, outerPosY, 0));
            }
        }
        void CompareAndWrite(ref float field, float newValue)
        {
            if (Mathf.Abs(field - newValue) > float.Epsilon)
            {
                isDirty = true;
                field = newValue;
            }
        }

    }
}