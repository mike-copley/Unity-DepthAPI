using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    private List<SurfacePoint> _surfacePoints = new List<SurfacePoint>();
    
    public Vector3 SurfaceOrigin { get; private set; }
    public Vector3 SurfaceNormal { get; private set; }
    public Vector3 SurfaceUp { get; private set; }
    public Vector3 SurfaceRight { get; private set; }
    
    private float SpaceBetweenPoints { get; set; }

    private Dictionary<int, Dictionary<int, SurfacePoint>> _pointsByUVs = new();
    
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
        
        SurfaceOrigin = surfacePlaneOrigin;
        SurfaceNormal = surfacePlaneNormal;
        
        SpaceBetweenPoints = spaceBetweenPoints;

        SurfaceUp = Vector3.Cross(referenceOrientationRight, SurfaceNormal);
        SurfaceRight = Vector3.Cross(SurfaceNormal, SurfaceUp);
    }

    public void AddPointToSurface(GameObject prefab, Vector3 point, Vector3 normal)
    {
        Debug.Log($"Surface.AddPointToSurface({prefab.name}, {point}, {normal})");
        // if (_surfacePoints.Count == 0)
        if (CalculatePointUVs(point, out var u, out var v, out var w))
        {
            if (!DoesPointExist(u, v))
            {
                CalculatePointForUVs(u, v, w, out var position);
                
                var rotation = Quaternion.LookRotation(normal);
                var newPoint = Instantiate(prefab, this.transform);
                newPoint.transform.SetPositionAndRotation(position, rotation);

                var newSurfacePoint = newPoint.AddComponent<SurfacePoint>();
                newSurfacePoint.point = position;
                newSurfacePoint.normal = normal;
                
                AddPointForUVs(u, v, newSurfacePoint);
            }
        }
    }
    
    public void End()
    {
        Debug.Log($"Surface.End()");
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

    private void CalculatePointForUVs(int u, int v, float w, out Vector3 point)
    {
        point = SurfaceOrigin + 
                (u * SpaceBetweenPoints * SurfaceRight) + 
                (v * SpaceBetweenPoints * SurfaceUp) +
                (w * SurfaceNormal);
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
