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
    
    public Vector3 SurfaceOrientationUp { get; private set; }
    public Vector3 SurfaceOrientationRight { get; private set; }
    
    private float SpaceBetweenPoints { get; set; }
    private float HalfSpaceBetweenPoints { get; set; }
    private float QuatSpaceBetweenPoints { get; set; }

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

        SurfaceOrientationUp = referenceOrientationUp;
        SurfaceOrientationRight = referenceOrientationRight;
        
        SurfaceOrigin = surfacePlaneOrigin;
        SurfaceNormal = surfacePlaneNormal;
        
        SpaceBetweenPoints = spaceBetweenPoints;
        HalfSpaceBetweenPoints = 0.5F * SpaceBetweenPoints;
        QuatSpaceBetweenPoints = 0.5F * HalfSpaceBetweenPoints;

        SurfaceUp = Vector3.Cross(SurfaceOrientationRight, SurfaceNormal);
        SurfaceRight = Vector3.Cross(SurfaceNormal, SurfaceUp);
    }

    public void AddPointToSurface(GameObject prefab, Vector3 point, Vector3 normal)
    {
        Debug.Log($"Surface.AddPointToSurface({prefab.name}, {point}, {normal})");
        if (_surfacePoints.Count == 0)
        {
            var rotation = Quaternion.LookRotation(normal);
            var newPoint = Instantiate(prefab, this.transform);
            newPoint.transform.SetPositionAndRotation(point, rotation);
            _surfacePoints.Add(newPoint.AddComponent<SurfacePoint>());
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

        // TODO:
        
        
        return true;
    }

    private void CalculatePointForUVs(int u, int v, float w, out Vector3 point)
    {
        point = SurfaceOrigin;
        
        // TODO:
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
    }
    
    private bool IsValidPoint(Vector3 point)
    {
        return false;
    }
}
