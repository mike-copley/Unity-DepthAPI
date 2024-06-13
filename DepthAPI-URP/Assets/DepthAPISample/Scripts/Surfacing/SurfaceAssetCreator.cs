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
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleSurfaceDataReceived()
    {
        var surfaceData = SurfaceDataListener.SurfaceDataReceivedEventData;
        var createdGameObject = Instantiate(SurfaceAsset, Vector3.zero, Quaternion.identity);
        
        // #if UNITY_EDITOR
        // PrefabUtility.UnpackPrefabInstance(createdGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        // #endif
        
        var meshFilter = createdGameObject.GetComponent<MeshFilter>();

        // TODO: replace the mesh in the instantiated asset with the
        // surface data that is provided, and then triangulated
        var points = meshFilter.mesh.vertices;
        var normals = meshFilter.mesh.normals;
        
        var triangulatedMesh = Triangulate(points, normals);
        meshFilter.mesh = triangulatedMesh;
        meshFilter.sharedMesh = triangulatedMesh;
    }

    private Mesh Triangulate(Vector3[] points, Vector3[] normals)
    {
        List<Vertex> vertices = new List<Vertex>();
        var index = 0;
        
        Debug.LogWarning($"TRIANGULATION: input ... {points.Length}, {normals.Length}");
        foreach (var point in points)
        {
            vertices.Add(GetVertexFromPointAndNormal(point, normals[index]));
            Debug.LogWarning($"TRIANGULATION: ... point = {point}");
            Debug.LogWarning($"TRIANGULATION: ... normal = {normals[index]}");
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

    private Vertex GetVertexFromPointAndNormal(Vector3 point, Vector3 normal)
    {
        return new Vertex()
        {
            Position = new[] { (double)point.x, (double)point.y, (double)point.z },
            Normal = new[] { normal.x, normal.y, normal.z }
        };
    }

    private string VertexToString(Vertex vertex)
    {
        return
            $"{vertex.Position[0]}, {vertex.Position[1]}, {vertex.Position[2]} ... {vertex.Normal[0]}, {vertex.Normal[1]}, {vertex.Normal[2]}";
    }
}
