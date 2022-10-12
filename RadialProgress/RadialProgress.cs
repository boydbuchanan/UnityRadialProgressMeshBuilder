using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kitchen.UIElements
{
    public partial class UITemplates : ScriptableObject
    {
        public StyleSheet RadialProgressStyleSheet;
    }
    /// <summary>
    /// An element that displays progress inside a partially filled circle
    /// </summary>
    public class RadialProgress : VisualElement
    {
        // A Factory class is needed to expose this control to UXML.
        public new class UxmlFactory : UxmlFactory<RadialProgress, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlFloatAttributeDescription ProgressAttribute = new UxmlFloatAttributeDescription(){name = "progress"};
            UxmlIntAttributeDescription BarSize = new() { name = "bar-size", defaultValue = 10 };
            UxmlBoolAttributeDescription AsBar = new() {name = "as-bar"};
            UxmlBoolAttributeDescription AsSquare = new() {name = "as-square"};
            UxmlBoolAttributeDescription InvertProgress = new() {name = "invert-progress"};
            
            public override void Init( VisualElement visualElement, IUxmlAttributes attributes, CreationContext creationContext )
            {
                base.Init( visualElement, attributes, creationContext );
                var element = visualElement as RadialProgress;
                if (element != null)
                {
                    element.Progress = ProgressAttribute.GetValueFromBag(attributes, creationContext);
                    element.BarSize = BarSize.GetValueFromBag(attributes, creationContext);
                    element.InvertProgress = InvertProgress.GetValueFromBag(attributes, creationContext);
                    element.AsBar = AsBar.GetValueFromBag(attributes, creationContext);
                    element.AsSquare = AsSquare.GetValueFromBag(attributes, creationContext);
                }
            }
        }

        private bool _invertprogress;
        public bool InvertProgress { get => _invertprogress; set { _invertprogress = value; MarkDirtyRepaint(); }}
        private bool _asSquare;
        public bool AsSquare { get => _asSquare; set { _asSquare = value; MarkDirtyRepaint(); }}
        private bool _asBar;
        public bool AsBar { get => _asBar; set { _asBar = value; MarkDirtyRepaint(); }}
        private int _barSize;
        public int BarSize { get => _barSize; set { _barSize = value; MarkDirtyRepaint(); }}

        /// <summary>
        /// A value between 0 and 100
        /// </summary>
        public float Progress
        {
            // The progress property is exposed in C#.
            get => progressValue;
            set
            {
                // Whenever the progress property changes, MarkDirtyRepaint() is named. This causes a call to the
                // generateVisualContents callback.
                progressValue = value;
                MarkDirtyRepaint();
            }
        }

        // This is the number that the Label displays as a percentage.
        private float progressValue;

        private const string editorFolder = "Assets/Sync/UIElements/RadialProgress";
        // These are USS class names for the control overall and the label.
        public static readonly string ussClassName = "radial-progress";

        // These objects allow C# code to access custom USS properties.
        static CustomStyleProperty<Color> s_TrackColor = new CustomStyleProperty<Color>("--track-color");
        static CustomStyleProperty<Color> s_ProgressColor = new CustomStyleProperty<Color>("--progress-color");

        // These are the meshes this control uses.
        RadialMesh trackMesh;
        RadialMesh progressMesh;

        // This is the number of outer vertices to generate the circle.
        const int k_NumSteps = 100;

        // This default constructor is RadialProgress's only constructor.
        public RadialProgress()
        {
            #if UNITY_EDITOR
            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>($"{editorFolder}/RadialProgress.uss");
            this.styleSheets.Add(stylesheet);
            #endif

            // Create meshes for the track and the progress.
            progressMesh = new RadialMesh(k_NumSteps);
            trackMesh = new RadialMesh(k_NumSteps);

            // Add the USS class name for the overall control.
            AddToClassList(ussClassName);

            // Register a callback after custom style resolution.
            RegisterCallback<CustomStyleResolvedEvent>(evt => CustomStylesResolved(evt));

            // Register a callback to generate the visual content of the control.
            generateVisualContent += context => GenerateVisualContent(context);

            Progress = 0.0f;
        }

        static void CustomStylesResolved(CustomStyleResolvedEvent evt)
        {
            RadialProgress element = (RadialProgress)evt.currentTarget;
            element.UpdateCustomStyles();
        }

        // After the custom colors are resolved, this method uses them to color the meshes and (if necessary) repaint
        // the control.
        void UpdateCustomStyles()
        {
            if (customStyle.TryGetValue(s_ProgressColor, out var progressColor))
            {
                progressMesh.Color = progressColor;
            }

            if (customStyle.TryGetValue(s_TrackColor, out var trackColor))
            {
                trackMesh.Color = trackColor;
            }

            if (progressMesh.IsDirty || trackMesh.IsDirty)
                MarkDirtyRepaint();
        }

        // The GenerateVisualContent() callback method calls DrawMeshes().
        static void GenerateVisualContent(MeshGenerationContext context)
        {
            RadialProgress element = (RadialProgress)context.visualElement;
            element.DrawMeshes(context);
        }

        // DrawMeshes() uses the EllipseMesh utility class to generate an array of vertices and indices, for both the
        // "track" ring (in grey) and the progress ring (in green). It then passes the geometry to the MeshWriteData
        // object, as returned by the MeshGenerationContext.Allocate() method. For the "progress" __mesh__The main graphics primitive of Unity. Meshes make up a large part of your 3D worlds. Unity supports triangulated or Quadrangulated polygon meshes. Nurbs, Nurms, Subdiv surfaces must be converted to polygons. [More info](comp-MeshGroup.html)<span class="tooltipGlossaryLink">See in [Glossary](Glossary.html#Mesh)</span>, only a slice of
        // the index arrays is used to progressively reveal parts of the mesh.
        void DrawMeshes(MeshGenerationContext context)
        {
            //Debug.Log($"Content Height {contentRect.height} - Content Width {contentRect.width}");
            
            float halfWidth = contentRect.width * 0.5f;
            float halfHeight = contentRect.height * 0.5f;

            if (halfWidth < 2.0f || halfHeight < 2.0f)
                return;
            
            float clampedBarSize = Mathf.Clamp(BarSize, 0.0f, Mathf.Min(halfWidth, halfHeight));

            progressMesh.Width = halfWidth;
            progressMesh.Height = halfHeight;
            progressMesh.BorderSize = clampedBarSize;
            progressMesh.AsBar = AsBar;
            progressMesh.AsSquare = AsSquare;
            progressMesh.UpdateMesh();

            trackMesh.Width = halfWidth;
            trackMesh.Height = halfHeight;
            trackMesh.BorderSize = clampedBarSize;
            trackMesh.AsBar = AsBar;
            trackMesh.AsSquare = AsSquare;
            trackMesh.UpdateMesh();

            // Draw track mesh first
            var trackIndices = trackMesh.GetIndices();
            var trackVertexes = trackMesh.GetVertexes();
            var trackMeshWriteData = context.Allocate(trackVertexes.Count(), trackIndices.Length);
            trackMeshWriteData.SetAllVertices(trackVertexes.ToArray());
            trackMeshWriteData.SetAllIndices(trackIndices);

            var progressVertexes = progressMesh.GetVertexes();
            var progressIndices = progressMesh.GetIndices(progressValue, InvertProgress);

            var progressMeshWriteData = context.Allocate(progressVertexes.Count(), progressIndices.Length);
            progressMeshWriteData.SetAllVertices(progressVertexes.ToArray());
            progressMeshWriteData.SetAllIndices(progressIndices);
        }
    }
}