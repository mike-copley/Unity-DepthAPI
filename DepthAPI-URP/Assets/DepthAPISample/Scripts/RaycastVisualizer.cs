using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastVisualizer : MonoBehaviour
{
    public Transform Source;
    public bool UseCameraAsSource;
    public float CameraTiltAngle = 10.0F;
    public float RayOffsetDistance = 0.1F;
    public DepthCast DepthCaster;
    public GameObject Prefab;

    private GameObject PlacedObject;
    
    private int frame = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        PlacedObject = Instantiate(Prefab);
    }

    // Update is called once per frame
    void Update()
    {
        var camera = Camera.allCameras[0];
    
        // Debug.LogWarning($"Camera: {camera.name}, {camera.transform.position}, {camera.transform.forward}");
        // Debug.LogWarning($"Camera: {camera.name}, {Source.position}, {Source.up}");

        if (!UseCameraAsSource)
        {
            var direction = Source.up;
            var origin = Source.position + (direction * RayOffsetDistance);
            DepthCaster.RaycastBlocking(
                new Ray(origin, direction),
                out var result);
            PlacedObject.transform.SetPositionAndRotation(result.Position, Quaternion.LookRotation(result.Normal));
        }

        else
        {
            var tilt = Quaternion.AngleAxis(CameraTiltAngle, camera.transform.right);
            var direction = tilt * camera.transform.forward;
            var origin = camera.transform.position + (direction * RayOffsetDistance);
            DepthCaster.RaycastBlocking(
                new Ray(origin, direction),
                out var result);
            PlacedObject.transform.SetPositionAndRotation(result.Position, Quaternion.LookRotation(result.Normal));
        }
    }
}
