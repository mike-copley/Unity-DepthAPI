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
    
    public Vector3 LastKnownPoint { get; private set; }
    public Vector3 LastKnownPointChange { get; private set; }
    public Vector3 LastKnownNormal { get; private set; }
    public Vector3 LastKnownNormalChange { get; private set; }
    
    // Start is called before the first frame update
    void Start()
    {
        PlacedObject = Instantiate(Prefab);
    }

    // Update is called once per frame
    void Update()
    {
        var camera = Camera.allCameras[0];

        if (!UseCameraAsSource)
        {
            var direction = Source.up;
            var origin = Source.position + (direction * RayOffsetDistance);
            DepthCaster.RaycastBlocking(
                new Ray(origin, direction),
                out var result);
            PlacedObject.transform.SetPositionAndRotation(result.Position, Quaternion.LookRotation(result.Normal));
            UpdateLastKnowns(result);
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
            UpdateLastKnowns(result);
        }
    }

    private void UpdateLastKnowns(DepthCastResult result)
    {
        LastKnownPoint = result.Position;
        LastKnownNormal = result.Normal;

        LastKnownPointChange = result.Position - LastKnownPoint;
        LastKnownNormalChange = result.Normal - LastKnownNormal;
    }
}
