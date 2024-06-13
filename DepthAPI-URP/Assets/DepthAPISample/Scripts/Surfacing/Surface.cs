using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MIConvexHull;
using UnityEngine;

public class Surface : MonoBehaviour
{
    public class SurfacesSerializedData
    {
        public class SurfaceData
        {
            public struct SurfaceVertexData
            {
                public int u;
                public int v;
                public float px;
                public float py;
                public float pz;
                public float nx;
                public float ny;
                public float nz;
            }

            public List<SurfaceVertexData> vertices;
        }

        public List<SurfaceData> surfaces;

        public static byte[] Serialize(SurfacesSerializedData surfacesData)
        {
            byte[] serializedData = null;
            using var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms))
            {
                Debug.Log($"Writing num s = {surfacesData.surfaces.Count()}");
                bw.Write(surfacesData.surfaces.Count());
                foreach (var surfaceData in surfacesData.surfaces)
                {
                    bw.Write(surfaceData.vertices.Count());
                    Debug.Log($"Writing num v = {surfaceData.vertices.Count}");
                    foreach (var vertexData in surfaceData.vertices)
                    {
                        Debug.Log($"Writing p, n = [{vertexData.px},{vertexData.py},{vertexData.pz}], [{vertexData.nx},{vertexData.ny},{vertexData.nz}]");
                        bw.Write(vertexData.u);
                        bw.Write(vertexData.v);
                        bw.Write(vertexData.px);
                        bw.Write(vertexData.py);
                        bw.Write(vertexData.pz);
                        bw.Write(vertexData.nx);
                        bw.Write(vertexData.ny);
                        bw.Write(vertexData.nz);
                    }
                }
            }

            serializedData = ms.ToArray();
            return serializedData;
        }
        
        public static SurfacesSerializedData Deserialize(byte[] serializedData)
        {
            var surfacesData = new SurfacesSerializedData();
            using var ms = new MemoryStream(serializedData);
            using (var br = new BinaryReader(ms))
            {
                var numSurfaces = br.ReadInt32();
                Debug.Log($"Reading num s = {numSurfaces}");
                surfacesData.surfaces = new List<SurfaceData>();
                for (var surfIndex = 0; surfIndex < numSurfaces; surfIndex++)
                {
                    var surfData = new SurfaceData();
                    var numVertices = br.ReadInt32();
                    Debug.Log($"Reading num v = {numVertices}");
                    surfData.vertices = new List<SurfaceData.SurfaceVertexData>();
                    for (var vertIndex = 0; vertIndex < numVertices; vertIndex++)
                    {
                        var vertData = new SurfaceData.SurfaceVertexData();
                        vertData.u = br.ReadInt32();
                        vertData.v = br.ReadInt32();
                        vertData.px = br.ReadSingle();
                        vertData.py = br.ReadSingle();
                        vertData.pz = br.ReadSingle();
                        vertData.nx = br.ReadSingle();
                        vertData.ny = br.ReadSingle();
                        vertData.nz = br.ReadSingle();
                        Debug.Log($"Reading p, n = [{vertData.px},{vertData.py},{vertData.pz}], [{vertData.nx},{vertData.ny},{vertData.nz}]");
                        surfData.vertices.Add(vertData);
                    }
                    surfacesData.surfaces.Add(surfData);
                }
            }

            return surfacesData;
        }

        public static SurfacesSerializedData CreateFromMesh(Vector3[] points, Vector3[] normals, int[] Us, int[] Vs)
        {
            var surfacesData = new SurfacesSerializedData();
            surfacesData.surfaces = new List<SurfaceData>();
            {
                var surfaceData = new SurfaceData();
                surfaceData.vertices = new List<SurfaceData.SurfaceVertexData>();
                var pindex = 0;
                foreach (var point in points)
                {
                    var vertexData = new SurfaceData.SurfaceVertexData();
                    Debug.Log($"CreateFromMesh() source p = {point}, n = {normals[pindex]}");
                    vertexData.u = Us[pindex];
                    vertexData.v = Vs[pindex];
                    vertexData.px = point.x;
                    vertexData.py = point.y;
                    vertexData.pz = point.z;
                    vertexData.nx = normals[pindex].x;
                    vertexData.ny = normals[pindex].y;
                    vertexData.nz = normals[pindex].z;
                    Debug.Log($"CreateFromMesh() vertex p = [{vertexData.px},{vertexData.py},{vertexData.pz}], n = [{vertexData.nx},{vertexData.ny},{vertexData.nz}]");
                    surfaceData.vertices.Add(vertexData);
                    pindex++;
                }
                surfacesData.surfaces.Add(surfaceData);
            }
            return surfacesData;
        }
        
        public static SurfacesSerializedData CreateFromSurfaces(IList<Surface> surfaces)
        {
            var surfacesData = new SurfacesSerializedData();
            surfacesData.surfaces = new List<SurfaceData>();
            foreach (var surface in surfaces)
            {
                var surfaceData = new SurfaceData();
                surfaceData.vertices = new List<SurfaceData.SurfaceVertexData>();
                foreach (var surfacePoint in surface._surfacePoints)
                {
                    var vertexData = new SurfaceData.SurfaceVertexData();
                    vertexData.u = surfacePoint.U;
                    vertexData.v = surfacePoint.V;
                    vertexData.px = surfacePoint.point.x;
                    vertexData.py = surfacePoint.point.y;
                    vertexData.pz = surfacePoint.point.z;
                    vertexData.nx = surfacePoint.normal.x;
                    vertexData.ny = surfacePoint.normal.y;
                    vertexData.nz = surfacePoint.normal.z;
                    surfaceData.vertices.Add(vertexData);
                }
                surfacesData.surfaces.Add(surfaceData);
            }
            return surfacesData;
        }
    }
    
    public class Vertex : IVertex2D
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
    
    private List<SurfacePoint> _surfacePoints = new List<SurfacePoint>();
    
    public Vector3 SurfaceOrigin { get; private set; }
    public Vector3 SurfaceNormal { get; private set; }
    public Vector3 SurfaceUp { get; private set; }
    public Vector3 SurfaceRight { get; private set; }
    
    private float SpaceBetweenPoints { get; set; }

    private Dictionary<int, Dictionary<int, SurfacePoint>> _pointsByUVs = new();
    private List<Vector3> _bakedPoints = new List<Vector3>();
    private List<Vector3> _bakedNormals = new List<Vector3>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Begin(
        Vector3 referenceOrientationUp, Vector3 referenceOrientationRight,
        Vector3 surfacePlaneOrigin, Vector3 surfacePlaneNormal, 
        float spaceBetweenPoints)
    {
        Debug.Log($"Surface.Begin({surfacePlaneOrigin}, {surfacePlaneNormal})");
        
        _surfacePoints.Clear();
        _pointsByUVs.Clear();
        _bakedPoints.Clear();
        _bakedNormals.Clear();
        
        SurfaceOrigin = surfacePlaneOrigin;
        SurfaceNormal = surfacePlaneNormal;
        
        SpaceBetweenPoints = spaceBetweenPoints;

        SurfaceUp = Vector3.Cross(referenceOrientationRight, SurfaceNormal);
        SurfaceRight = Vector3.Cross(SurfaceNormal, SurfaceUp);

        var lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    public void AddPointToSurface(GameObject prefab, Vector3 point, Vector3 normal)
    {
        // Debug.Log($"Surface.AddPointToSurface({prefab.name}, {point}, {normal})");
        if (CalculatePointUVs(point, out var u, out var v, out var w))
        {
            if (!DoesPointExist(u, v))
            {
                var position = CalculatePointForUVs(u, v, w);
                
                var rotation = Quaternion.LookRotation(normal);
                var newPoint = Instantiate(prefab, this.transform);
                newPoint.transform.SetPositionAndRotation(position, rotation);

                var newSurfacePoint = newPoint.GetComponent<SurfacePoint>();
                newSurfacePoint.point = position;
                newSurfacePoint.normal = normal;
                newSurfacePoint.U = u;
                newSurfacePoint.V = v;
                
                AddPointForUVs(u, v, newSurfacePoint);
            }
        }
    }

    public void RemovePointFromSurface(Vector3 point)
    {
        Debug.Log($"PAINTING: Surface.RemovePointFromSurface()");
        if (CalculatePointUVs(point, out var u, out var v, out var w))
        {
            Debug.Log($"PAINTING: ... we have UVs for the point: {u}, {v}");
            if (DoesPointExist(u, v))
            {
                Debug.Log($"PAINTING: ... point does exist for UVs.");
                _pointsByUVs[u].TryGetValue(v, out var surfacePoint);
                _pointsByUVs[u].Remove(v);
                _surfacePoints.Remove(surfacePoint);
                Debug.Log($"PAINTING: ... destroying surface point.");
                Destroy(surfacePoint.gameObject);
            }
        }
    }
    
    public void End()
    {
        Debug.Log($"Surface.End()");
        // TODO: add line renderer for convex hull of our points+normals, and remove point game objects
        // TODO: turn off the point mesh renderers, but keep the normal renderers on

        if (_surfacePoints.Count == 0)
            return;
        
        foreach (var surfacePoint in _surfacePoints)
        {
            surfacePoint.pointRenderer.enabled = false;
        }

        var boundaryPoints = GetBoundaryPoints();
        
        var lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.enabled = true;
        lineRenderer.positionCount = boundaryPoints.Length;
        lineRenderer.SetPositions(boundaryPoints);
    }

    private Vector3[] GetBoundaryPoints()
    {
        if (_surfacePoints.Count == 0)
            return null;

        List<Vertex> vertices = new List<Vertex>();
        
        foreach (var surfacePoint in _surfacePoints)
        {
            var v = new Vertex { X = surfacePoint.U, Y = surfacePoint.V };
            vertices.Add(v);
        }
            
        // TODO: get 2d convex hull of all point UVs, and use convex hull's result
        // UVs to convert back to points
        
        var result = ConvexHull.Create2D(vertices);

        var surfaceBoundaryPts = new Vector3[result.Result.Count + 1];
        var index = 0;
        
        foreach (var hullPt in result.Result)
        {
            var u = (int)hullPt.X;
            var v = (int)hullPt.Y;
            Debug.Log($"PAINTING: Convex Hull point {index}: {u}, {v}");
            surfaceBoundaryPts[index] = CalculatePointForUVs(u, v, 0F);
            index++;
        }
        Debug.Log($"PAINTING: Convex Hull point {index}: {(int)result.Result[0].X}, {(int)result.Result[0].Y}");
        surfaceBoundaryPts[index] = CalculatePointForUVs((int)result.Result[0].X, (int)result.Result[0].Y, 0F);

        Debug.Log($"PAINTING: Convex Hull points total: {surfaceBoundaryPts.Length}");

        GetMinAndMaxUVValues(out var minU, out var maxU, out var minV, out var maxV);
        Debug.Log($"PAINTING: UV rect: {CalculatePointForUVs(minU, minV, 0F)}, {CalculatePointForUVs(maxU, minV, 0F)}, {CalculatePointForUVs(maxU, maxV, 0F)}, {CalculatePointForUVs(minU, maxV, 0F)}");

        // NOTE: code below will get the bounding rectangle 
        // surfaceBoundaryPts = new Vector3[]
        // {
        //     CalculatePointForUVs(minU, minV, 0F),
        //     CalculatePointForUVs(maxU, minV, 0F),
        //     CalculatePointForUVs(maxU, maxV, 0F),
        //     CalculatePointForUVs(minU, maxV, 0F),
        //     CalculatePointForUVs(minU, minV, 0F),
        // };
        
        return surfaceBoundaryPts;
    }

    private void GetMinAndMaxUVValues(out int minU, out int maxU, out int minV, out int maxV)
    {
        minU = 1000000;
        maxU = -1000000;
        minV = 1000000;
        maxV = -1000000;
        foreach (var surfacePoint in _surfacePoints)
        {
            if (surfacePoint.U > maxU) maxU = surfacePoint.U;
            if (surfacePoint.U < minU) minU = surfacePoint.U;
            if (surfacePoint.V > maxV) maxV = surfacePoint.V;
            if (surfacePoint.V < minV) minV = surfacePoint.V;
        }
    }
    
    private bool CalculatePointUVs(Vector3 point, out int u, out int v, out float w)
    {
        u = 0;
        v = 0;
        w = 0.0F;

        if (!IsValidPoint(point))
            return false;

        var d = point - SurfaceOrigin;
        
        var s = Vector3.Dot(d, SurfaceRight) / SpaceBetweenPoints;
        var t = Vector3.Dot(d, SurfaceUp) / SpaceBetweenPoints;

        u = Mathf.RoundToInt(s);
        v = Mathf.RoundToInt(t);

        w = Vector3.Dot(d, SurfaceNormal);
        
        return true;
    }

    private SurfacePoint GetSurfacePointForUVs(int u, int v)
    {
        if (_pointsByUVs.ContainsKey(u))
        {
            if (_pointsByUVs[u].ContainsKey(v))
            {
                _pointsByUVs[u].TryGetValue(v, out var surfacePoint);
                return surfacePoint;
            }
        }

        return null;
    }
    
    private Vector3 CalculatePointForUVs(int u, int v, float w)
    {
        var point = SurfaceOrigin + 
                (u * SpaceBetweenPoints * SurfaceRight) + 
                (v * SpaceBetweenPoints * SurfaceUp) +
                (w * SurfaceNormal);
        return point;
    }

    private bool DoesPointExist(int u, int v)
    {
        if (!_pointsByUVs.ContainsKey(u))
            return false;

        if (!_pointsByUVs[u].ContainsKey(v))
            return false;

        return true;
    }

    private void AddPointForUVs(int u, int v, SurfacePoint surfacePoint)
    {
        if (!_pointsByUVs.ContainsKey(u))
        {
            _pointsByUVs[u] = new Dictionary<int, SurfacePoint>();
        }

        if (!_pointsByUVs[u].ContainsKey(v))
        {
            _pointsByUVs[u].Add(v, surfacePoint);
        }

        _surfacePoints.Add(surfacePoint);
    }
    
    private bool IsValidPoint(Vector3 point)
    {
        // var d = point - SurfaceOrigin;
        //
        // var s = Vector3.Dot(d, SurfaceRight) / SpaceBetweenPoints;
        // var t = Vector3.Dot(d, SurfaceUp) / SpaceBetweenPoints;
        //
        // var x = Mathf.Abs(s - Mathf.Floor(s));
        // var y = Mathf.Abs(t - Mathf.Floor(t));
        //
        // if (x is > 0.25F and < 0.75F)
        //     return false;
        //
        // if (y is > 0.25F and < 0.75F)
        //     return false;
        //
        return true;
    }
}
