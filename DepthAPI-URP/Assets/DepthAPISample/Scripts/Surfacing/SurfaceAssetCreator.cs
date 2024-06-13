using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;
using MIConvexHull;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class SurfaceAssetCreator : MonoBehaviour
{
    // public class Vertex : IVertex
    // {
    //     public double[] Position { get; set; }
    //     public float[] Normal { get; set; }
    // }
    
    public class DelPoint : IPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        
        public Vector3 Point { get; set; }
        public Vector3 Normal { get; set; }
        public int Index { get; set; }
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

        List<Mesh> meshes = new List<Mesh>();

        var surfacesBase = CalculateSurfacesBottomCenter(surfacesData);
        OffsetSurfacesBy(surfacesData, surfacesBase);
        
        foreach (var surface in surfacesData.surfaces)
        {
            var triangulatedMesh = Triangulate(surface.vertices);
            meshes.Add(triangulatedMesh);
        }

        var combinedMesh = CombineMeshes(meshes);
        
        meshFilter.mesh = combinedMesh;
        meshFilter.sharedMesh = combinedMesh;
    }

    private Vector3 CalculateSurfacesBottomCenter(Surface.SurfacesSerializedData surfacesData)
    {
        var cx = 0F;
        var cy = 1000000F;
        var cz = 0F;
        var count = 0;
        
        foreach (var surface in surfacesData.surfaces)
        {
            foreach (var vertex in surface.vertices)
            {
                cx += vertex.px;
                cy = cy > vertex.py ? vertex.py : cy;
                cz += vertex.pz;
                count++;
            }
        }

        cx /= (count > 0 ? count : 1);
        cz /= (count > 0 ? count : 1);
        
        return new Vector3(cx, cy, cz);
    }

    private void OffsetSurfacesBy(Surface.SurfacesSerializedData surfacesData, Vector3 offset)
    {
        foreach (var surface in surfacesData.surfaces)
        {
            foreach (var vertex in surface.vertices)
            {
                vertex.px -= offset.x;
                vertex.py -= offset.y;
                vertex.pz -= offset.z;
            }
        }
    }
    
    private Mesh CombineMeshes(List<Mesh> meshes)
    {
        var meshVertices = new List<Vector3>();
        var meshNormals = new List<Vector3>();
        var meshIndices = new List<int>();

        foreach (var mesh in meshes)
        {
            var vertices = mesh.vertices;
            var normals = mesh.normals;

            for (var index = 0; index < vertices.Length; index++)
            {
                // Debug.Log($"final pt: {vertices[index]}");
                meshVertices.Add(vertices[index]);
                meshNormals.Add(normals[index]);
                // Debug.Log($"final id: {meshIndices.Count}");
                meshIndices.Add(meshIndices.Count);
            }
        }
        
        var newMesh = new Mesh { vertices = meshVertices.ToArray(), normals = meshNormals.ToArray() };
        newMesh.SetIndices(meshIndices.ToArray(), MeshTopology.Triangles, 0);

        return newMesh;
    }
    
    private Mesh Triangulate(List<Surface.SurfacesSerializedData.SurfaceData.SurfaceVertexData> surfaceVertices)
    {
        List<Vector3> meshVertices = new List<Vector3>();
        List<Vector3> meshNormals = new List<Vector3>();
        List<int> meshIndices = new List<int>();

        var delpointsList = new List<DelPoint>();
        var index = 0;
        
        // Debug.LogWarning($"TRIANGULATION: input ... num vertices = {surfaceVertices.Count()}");
        foreach (var vertex in surfaceVertices)
        {
            // Debug.LogWarning($"TRIANGULATION: ... source point = [{vertex.px},{vertex.py},{vertex.pz}]");
            // Debug.LogWarning($"TRIANGULATION: ... source normal = [{vertex.nx},{vertex.ny},{vertex.nz}]");
            delpointsList.Add(GetDelPointFromPointAndNormal(vertex.u, vertex.v, vertex.px, vertex.py, vertex.pz, vertex.nx, vertex.ny, vertex.nz));
            // Debug.LogWarning($"TRIANGULATION: ... vertex = [{DelPointToString(delpointsList[index])}]");
            index++;
        }
        
        var delpoints = new IPoint[delpointsList.Count];

        index = 0;
        foreach (var dp in delpointsList)
        {
            delpoints[index] = dp;
            index++;
        }
        
        var delaunator = new Delaunator(delpoints);
        // Debug.LogWarning($"TRIANGULATION: output triangle count ... {delaunator.GetTriangles().Count()}");
        
        foreach (var triangle in delaunator.GetTriangles())
        {
            var vertices = new IPoint[3];
            index = 0;
            foreach (var dp in triangle.Points)
            {
                vertices[index] = dp;
                index++;
            }

            var dp0 = vertices[0] as DelPoint;
            var dp1 = vertices[1] as DelPoint;
            var dp2 = vertices[2] as DelPoint;
            
            // Debug.LogWarning($"TRIANGULATION: ... vertex = {DelPointToString(dp0)}");
            GetPointAndNormalFromDelPoint(dp0, out var point0, out var normal0);
            meshVertices.Add(point0);
            meshNormals.Add(normal0);
            meshIndices.Add(meshIndices.Count);
            
            // Debug.LogWarning($"TRIANGULATION: ... vertex = {DelPointToString(dp1)}");
            GetPointAndNormalFromDelPoint(dp1, out var point1, out var normal1);
            meshVertices.Add(point1);
            meshNormals.Add(normal1);
            meshIndices.Add(meshIndices.Count);
            
            // Debug.LogWarning($"TRIANGULATION: ... vertex = {DelPointToString(dp2)}");
            GetPointAndNormalFromDelPoint(dp2, out var point2, out var normal2);
            meshVertices.Add(point2);
            meshNormals.Add(normal2);
            meshIndices.Add(meshIndices.Count);
        }

        // List<Vertex> vertices = new List<Vertex>();
        // var index = 0;
        //
        // Debug.LogWarning($"TRIANGULATION: input ... num vertices = {surfaceVertices.Count()}");
        // foreach (var vertex in surfaceVertices)
        // {
        //     Debug.LogWarning($"TRIANGULATION: ... source point = [{vertex.px},{vertex.py},{vertex.pz}]");
        //     Debug.LogWarning($"TRIANGULATION: ... source normal = [{vertex.nx},{vertex.ny},{vertex.nz}]");
        //     vertices.Add(GetVertexFromPointAndNormal(vertex.px, vertex.py, vertex.pz, vertex.nx, vertex.ny, vertex.nz));
        //     Debug.LogWarning($"TRIANGULATION: ... vertex = [{VertexToString(vertices[index])}]");
        //     index++;
        // }
        //
        // var result = MIConvexHull.Triangulation.CreateDelaunay(vertices);
        // Debug.LogWarning($"TRIANGULATION: output ... {result.Cells.Count()}");
        //
        // foreach (var cell in result.Cells)
        // {
        //     Debug.LogWarning($"TRIANGULATION: ... face vertex count = {cell.Vertices.Count()}");
        //     Debug.LogWarning($"TRIANGULATION: ... face normal component count = {cell.Normal.Length}");
        //     
        //     Debug.LogWarning($"TRIANGULATION: ... vertex = {VertexToString(cell.Vertices[0])}");
        //     GetPointAndNormalFromVertex(cell.Vertices[0], out var point0, out var normal0);
        //     meshVertices.Add(point0);
        //     meshNormals.Add(normal0);
        //     meshIndices.Add(meshIndices.Count);
        //     
        //     Debug.LogWarning($"TRIANGULATION: ... vertex = {VertexToString(cell.Vertices[1])}");
        //     GetPointAndNormalFromVertex(cell.Vertices[1], out var point1, out var normal1);
        //     meshVertices.Add(point1);
        //     meshNormals.Add(normal1);
        //     meshIndices.Add(meshIndices.Count);
        //     
        //     Debug.LogWarning($"TRIANGULATION: ... vertex = {VertexToString(cell.Vertices[2])}");
        //     GetPointAndNormalFromVertex(cell.Vertices[2], out var point2, out var normal2);
        //     meshVertices.Add(point2);
        //     meshNormals.Add(normal2);
        //     meshIndices.Add(meshIndices.Count);
        // }

        var newMesh = new Mesh { vertices = meshVertices.ToArray(), normals = meshNormals.ToArray() };
        newMesh.SetIndices(meshIndices.ToArray(), MeshTopology.Triangles, 0);
        return newMesh;
    }

    private void GetPointAndNormalFromDelPoint(DelPoint vertex, out Vector3 point, out Vector3 normal)
    {
        point = vertex.Point;
        normal = vertex.Normal;
    }

    private DelPoint GetDelPointFromPointAndNormal(int u, int v, float px, float py, float pz, float nx, float ny, float nz)
    {
        return new DelPoint()
        {
            X = (double)u, 
            Y = (double)v,
            Point = new Vector3(px, py, pz),
            Normal = new Vector3(nx, ny, nz )
        };
    }

    private string DelPointToString(DelPoint vertex)
    {
        return
            $"{vertex.X}, ..., {vertex.Y} ... {vertex.Normal[0]}, {vertex.Normal[1]}, {vertex.Normal[2]}";
    }

    // private void GetPointAndNormalFromVertex(Vertex vertex, out Vector3 point, out Vector3 normal)
    // {
    //     point = new Vector3(
    //         (float)vertex.Position[0], 
    //         (float)vertex.Position[1], 
    //         (float)vertex.Position[^1]);
    //     normal = new Vector3(vertex.Normal[0], vertex.Normal[1], vertex.Normal[2]);
    // }
    //
    // private Vertex GetVertexFromPointAndNormal(float px, float py, float pz, float nx, float ny, float nz)
    // {
    //     return new Vertex()
    //     {
    //         Position = new[] { (double)px, (double)py, (double)pz },
    //         Normal = new[] { nx, ny, nz }
    //     };
    // }
    //
    // private string VertexToString(Vertex vertex)
    // {
    //     return
    //         $"{vertex.Position[0]}, ..., {vertex.Position[^1]} ... {vertex.Normal[0]}, {vertex.Normal[1]}, {vertex.Normal[2]}";
    // }
}
