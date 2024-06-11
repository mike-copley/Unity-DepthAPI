using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    private List<SurfacePoint> _surfacePoints = new List<SurfacePoint>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Begin()
    {
        _surfacePoints.Clear();
    }

    public void AddPointToSurface(GameObject prefab, Vector3 point, Vector3 normal)
    {
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
        
    }
}
