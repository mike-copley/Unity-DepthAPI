using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfacesEditor : MonoBehaviour
{
    public SurfaceMaker SurfaceMaker;
    public SurfaceEditControls SurfaceEditControls;
    public SurfaceDataSender SurfaceDataSender;

    public void HandleStartButtonPressed()
    {
        // SurfaceDataSender.SendTestData = true;
        var surfaces = SurfaceMaker.BakedSurfaces;
        var surfacesData = Surface.SurfacesSerializedData.CreateFromSurfaces(surfaces);
        var serializedData = Surface.SurfacesSerializedData.Serialize(surfacesData);
        SurfaceDataSender.SendDataToListener(serializedData);
    }
    
    public void HandleLeftTriggerDown()
    {
    }

    public void HandleLeftControlledMovedWhenDown()
    {
        Debug.Log($"PAINTING: SurfacesEditor.HandleLeftControlledMovedWhenDown()");
        SurfaceMaker.RemovePointFromActiveSurface(
            SurfaceEditControls.LeftRaycaster.LastKnownPoint);
    }
    
    public void HandleLeftTriggerUp()
    {
        
    }

    public void HandleLeftSqueeze()
    {
        SurfaceMaker.RemoveLastSurface();
    }

    public void HandleRightTriggerDown()
    {
        Debug.Log($"PAINTING: HandleRightTriggerDown()");
        if (SurfaceMaker.ActiveSurface == null)
        {
            Debug.Log($"PAINTING: ... StartNewSurface()");
            SurfaceMaker.StartNewSurface(
                SurfaceEditControls.HeadRaycaster.ReferenceOrientationUp,
                SurfaceEditControls.HeadRaycaster.ReferenceOrientationRight,
                SurfaceEditControls.HeadRaycaster.LastKnownPoint,
                SurfaceEditControls.HeadRaycaster.LastKnownNormal);
        }
        
        Debug.Log($"PAINTING: ... AddPointToActiveSurface()");
        SurfaceMaker.AddPointToActiveSurface(
            SurfaceEditControls.RightRaycaster.LastKnownPoint,
            SurfaceEditControls.RightRaycaster.LastKnownNormal);
    }

    public void HandleRightControllerMovedWhenDown()
    {
        // Debug.Log($"PAINTING: HandleRightControllerMovedWhenDown()");
        SurfaceMaker.AddPointToActiveSurface(
            SurfaceEditControls.RightRaycaster.LastKnownPoint,
            SurfaceEditControls.RightRaycaster.LastKnownNormal);
    }
    
    public void HandleRightTriggerUp()
    {
        Debug.Log($"PAINTING: HandleRightTriggerUp()");
    }

    public void HandleRightSqueeze()
    {
        Debug.Log($"PAINTING: HandleRightSqueeze()");
        SurfaceMaker.EndActiveSurface();
    }
}
