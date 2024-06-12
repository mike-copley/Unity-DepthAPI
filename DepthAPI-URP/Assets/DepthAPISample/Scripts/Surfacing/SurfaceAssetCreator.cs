using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceAssetCreator : MonoBehaviour
{
    public GameObject SurfaceAsset;
    public SurfaceDataListener SurfaceDataListener;
    
    // Start is called before the first frame update
    void Start()
    {
        SurfaceDataListener.SurfaceDataReceived += HandleSurfaceDataReceived;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HandleSurfaceDataReceived(byte[] surfaceData)
    {
        Instantiate(SurfaceAsset);
        
        // TODO: replace the mesh in the instantiated asset with the
        // surface data that is provided, and then triangulated
    }
}
