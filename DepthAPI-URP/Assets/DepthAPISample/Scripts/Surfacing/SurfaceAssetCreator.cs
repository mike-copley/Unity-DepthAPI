using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MIConvexHull;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class SurfaceAssetCreator : MonoBehaviour
{
    public struct Vertex : IVertex
    {
        public double[] Position { get; set; }
        public float[] Normal { get; set; }
    }
    
    public GameObject SurfaceAsset;
    public SurfaceDataListener SurfaceDataListener;

    public void HandleSurfaceDataReceived()
    {
        var surfaceSerializedData = SurfaceDataListener.SurfaceDataReceivedEventData;
        var surfacesData = Surface.SurfacesSerializedData.Deserialize(surfaceSerializedData);
        
        var createdGameObject = Instantiate(SurfaceAsset, Vector3.zero, Quaternion.identity);
        
        // #if UNITY_EDITOR
        // PrefabUtility.UnpackPrefabInstance(createdGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        // #endif
        
        var meshFilter = createdGameObject.GetComponent<MeshFilter>();

        // TODO: replace the mesh in the instantiated asset with the
        // surface data that is provided, and then triangulated
        // var points = meshFilter.mesh.vertices;
        // var normals = meshFilter.mesh.normals;
        
        var triangulatedMesh = Triangulate(surfacesData.surfaces[0].vertices);
        meshFilter.mesh = triangulatedMesh;
        meshFilter.sharedMesh = triangulatedMesh;
    }

    private Mesh Triangulate(List<Surface.SurfacesSerializedData.SurfaceData.SurfaceVertexData> surfaceVertices)
    {
        List<Vertex> vertices = new List<Vertex>();
        var index = 0;
        
        Debug.LogWarning($"TRIANGULATION: input ... num vertices = {surfaceVertices.Count()}");
        foreach (var vertex in surfaceVertices)
        {
            Debug.LogWarning($"TRIANGULATION: ... source point = [{vertex.px},{vertex.py},{vertex.pz}]");
            Debug.LogWarning($"TRIANGULATION: ... source normal = [{vertex.nx},{vertex.ny},{vertex.nz}]");
            vertices.Add(GetVertexFromPointAndNormal(vertex.px, vertex.py, vertex.pz, vertex.nx, vertex.ny, vertex.nz));
            Debug.LogWarning($"TRIANGULATION: ... vertex p = [{vertices[index].Position[0]},{vertices[index].Position[1]},{vertices[index].Position[2]}]");
            Debug.LogWarning($"TRIANGULATION: ... vertex n = [{vertices[index].Normal[0]},{vertices[index].Normal[1]},{vertices[index].Normal[2]}]");
            index++;
        }

        List<Vector3> meshVertices = new List<Vector3>();
        List<Vector3> meshNormals = new List<Vector3>();
        List<int> meshIndices = new List<int>();
        
        var result = MIConvexHull.Triangulation.CreateDelaunay(vertices);
        Debug.LogWarning($"TRIANGULATION: output ... {result.Cells.Count()}");
        
        foreach (var cell in result.Cells)
        {
            Debug.LogWarning($"TRIANGULATION: ... face vertex count = {cell.Vertices.Count()}");
            Debug.LogWarning($"TRIANGULATION: ... face normal component count = {cell.Normal.Length}");
            
            Debug.LogWarning($"TRIANGULATION: ... vertex = {VertexToString(cell.Vertices[0])}");
            GetPointAndNormalFromVertex(cell.Vertices[0], out var point0, out var normal0);
            meshVertices.Add(point0);
            meshNormals.Add(normal0);
            meshIndices.Add(meshIndices.Count);
            
            Debug.LogWarning($"TRIANGULATION: ... vertex = {VertexToString(cell.Vertices[1])}");
            GetPointAndNormalFromVertex(cell.Vertices[1], out var point1, out var normal1);
            meshVertices.Add(point1);
            meshNormals.Add(normal1);
            meshIndices.Add(meshIndices.Count);
            
            Debug.LogWarning($"TRIANGULATION: ... vertex = {VertexToString(cell.Vertices[2])}");
            GetPointAndNormalFromVertex(cell.Vertices[2], out var point2, out var normal2);
            meshVertices.Add(point2);
            meshNormals.Add(normal2);
            meshIndices.Add(meshIndices.Count);
        }

        var newMesh = new Mesh { vertices = meshVertices.ToArray(), normals = meshNormals.ToArray() };
        newMesh.SetIndices(meshIndices.ToArray(), MeshTopology.Triangles, 0);
        
        return newMesh;
    }

    private void GetPointAndNormalFromVertex(Vertex vertex, out Vector3 point, out Vector3 normal)
    {
        point = new Vector3(
            (float)vertex.Position[0], 
            (float)vertex.Position[1], 
            (float)vertex.Position[2]);
        normal = new Vector3(vertex.Normal[0], vertex.Normal[1], vertex.Normal[2]);
    }

    private Vertex GetVertexFromPointAndNormal(float px, float py, float pz, float nx, float ny, float nz)
    {
        return new Vertex()
        {
            Position = new[] { (double)px, (double)py, (double)pz },
            Normal = new[] { nx, ny, nz }
        };
    }

    private string VertexToString(Vertex vertex)
    {
        return
            $"{vertex.Position[0]}, {vertex.Position[1]}, {vertex.Position[2]} ... {vertex.Normal[0]}, {vertex.Normal[1]}, {vertex.Normal[2]}";
    }
}
