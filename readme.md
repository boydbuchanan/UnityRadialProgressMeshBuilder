# Radial Progress Bar

Creates a Circle or Square mesh that can be used as a progress indicator. Such as a timer or cooldown.

## Usage

### Generate Mesh
```csharp
mesh = new RadialMesh(NumSteps, FlipNormals);
mesh.Width = Width;
mesh.Height = Height;
mesh.BorderSize = BorderSize;
mesh.AsBar = AsBar;
mesh.AsSquare = AsSquare;
mesh.IsDirty = true;
mesh.UpdateMesh();
```
### Get Vertices
```csharp
IEnumerable<Vector3> vertices = mesh.GetVertices(InvertProgress)
```
### Get Indices
```csharp
ushort[] testIndices = mesh.GetIndices(Progress, InvertProgress)
```

# Online Resources
Online Algorithms: https://rextester.com/CYCBJ87813

# Credits
http://www.code-spot.co.za/2020/11/04/generating-meshes-procedurally-in-unity/

Thanks for showing me how to debug vertices.

# Other Resources
https://docs.unity3d.com/Manual/UIE-radial-progress.html

Used as original framework

# Known Issues

After generating the mesh, the UIElement renders the mesh upside down

:::image type="content" source="Images/RadialFlipped.png" alt-text="Issue":::