using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceMaker : MonoBehaviour
{
    public GameObject SurfacesContainer;
    public GameObject SurfacePointPrefab;

    public float SpaceBetweenPoints = 0.05F;
    public float PointDistanceFromSurfacePlaneForAdd = 0.05F;
    public float NormalAngleFromSurfacePlaneForAdd = 15.0F;
    
    private Surface _activeSurface;
    private List<Surface> _Surfaces = new List<Surface>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartNewSurface(Vector3 surfacePlaneOrigin, Vector3 surfacePlaneNormal)
    {
        var newSurfaceObject = new GameObject("Surface");
        var newSurface = newSurfaceObject.AddComponent<Surface>();
        _activeSurface = newSurface;
        _activeSurface.transform.parent = SurfacesContainer.transform;
        _activeSurface.Begin();
    }

    public void AddPointToActiveSurface(Vector3 point, Vector3 normal)
    {
        _activeSurface.AddPointToSurface(SurfacePointPrefab, point, normal);
    }
    
    public void EndActiveSurface()
    {
        _Surfaces.Add(_activeSurface);
        _activeSurface.End();
        _activeSurface = null;
    }

    public void RemoveLastSurface()
    {
        
    }
}
