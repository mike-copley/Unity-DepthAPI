using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceMaker : MonoBehaviour
{
    public GameObject SurfacesContainer;
    public GameObject SurfacePointPrefab;

    public float SpaceBetweenPoints = 0.02F;
    public float PointDistanceFromSurfacePlaneForAdd = 0.05F;
    public float NormalAngleFromSurfacePlaneForAdd = 15.0F;
    
    private Surface _activeSurface;
    private List<Surface> _Surfaces = new List<Surface>();

    public Surface ActiveSurface => _activeSurface;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartNewSurface(
        Vector3 referenceOrientationUp, Vector3 referenceOrientationRight, 
        Vector3 surfacePlaneOrigin, Vector3 surfacePlaneNormal)
    {
        Debug.Log($"SurfaceMaker.StartNewSurface({surfacePlaneOrigin}, {surfacePlaneNormal})");
        var newSurfaceObject = new GameObject("Surface");
        var newSurface = newSurfaceObject.AddComponent<Surface>();
        _activeSurface = newSurface;
        _activeSurface.transform.parent = SurfacesContainer.transform;
        _activeSurface.Begin(
            referenceOrientationUp, referenceOrientationRight, 
            surfacePlaneOrigin, surfacePlaneNormal, 
            SpaceBetweenPoints);
    }

    public void AddPointToActiveSurface(Vector3 point, Vector3 normal)
    {
        Debug.Log($"SurfaceMaker.AddPointToActiveSurface({point}, {normal})");

        var d = point - _activeSurface.SurfaceOrigin;
        var w = Vector3.Dot(d, _activeSurface.SurfaceNormal);

        if (Mathf.Abs(w) > PointDistanceFromSurfacePlaneForAdd)
        {
            return;
        }

        var a = Vector3.Dot(normal, _activeSurface.SurfaceNormal);

        if (a < Mathf.Cos(Mathf.Deg2Rad * NormalAngleFromSurfacePlaneForAdd))
        {
            return;
        }
        
        _activeSurface.AddPointToSurface(SurfacePointPrefab, point, normal);
    }

    public void RemovePointFromActiveSurface(Vector3 point)
    {
        if (_activeSurface != null)
        {
            _activeSurface.RemovePointFromSurface(point);
        }
    }
    
    public void EndActiveSurface()
    {
        Debug.Log($"SurfaceMaker.EndActiveSurface()");
        _Surfaces.Add(_activeSurface);
        _activeSurface.End();
        _activeSurface = null;
    }

    public void RemoveLastSurface()
    {
        if (_activeSurface != null)
        {
            // TODO: remove the active surface
        }
        else if (_Surfaces.Count > 0)
        {
            // TODO: remove the end of the surfaces list
        }
    }
}
