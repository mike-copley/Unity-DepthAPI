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
        
    }

    public void AddPointToActiveSurface(Vector3 point, Vector3 normal)
    {
        
    }
    
    public void EndActiveSurface()
    {
        
    }

    public void RemoveLastSurface()
    {
        
    }
}
