using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfacesEditor : MonoBehaviour
{
    public SurfaceMaker SurfaceMaker;
    public SurfaceEditControls SurfaceEditControls;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleLeftTriggerDown()
    {
        SurfaceMaker.RemoveLastSurface();
    }

    public void HandleLeftControlledMovedWhenDown()
    {
        
    }
    
    public void HandleLeftTriggerUp()
    {
        
    }

    public void HandleRightTriggerDown()
    {
        SurfaceMaker.StartNewSurface(
            SurfaceEditControls.HeadRaycaster.LastKnownPoint,
            SurfaceEditControls.HeadRaycaster.LastKnownNormal);
    }

    public void HandleRightControllerMovedWhenDown()
    {
        SurfaceMaker.AddPointToActiveSurface(
            SurfaceEditControls.RightRaycaster.LastKnownPoint,
            SurfaceEditControls.RightRaycaster.LastKnownNormal);
    }
    
    public void HandleRightTriggerUp()
    {
        SurfaceMaker.EndActiveSurface();
    }
}
