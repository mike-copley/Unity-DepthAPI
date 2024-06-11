using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SurfaceEditControls : MonoBehaviour
{
    // Grip trigger thresholds for picking up objects, with some hysteresis.
    public float grabBegin = 0.55f;
    public float grabEnd = 0.35f;
    public float pointDistanceChangeMovementThresh = 0.01F;

    public OVRInput.Controller LeftController;
    public OVRInput.Controller RightController;

    public RaycastVisualizer LeftRaycaster;
    public RaycastVisualizer RightRaycaster;
    public RaycastVisualizer HeadRaycaster;
    
    public UnityEvent OnLeftStart;
    public UnityEvent OnLeftMovedWhenDown;
    public UnityEvent OnLeftFinish;

    public UnityEvent OnRightStart;
    public UnityEvent OnRightMovedWhenDown;
    public UnityEvent OnRightFinish;
    
    private float _currLeftFlex = 0.0F;
    private float _currRightFlex = 0.0F;

    private bool _isLeftDown = false;
    private bool _isRightDown = false;
    
    // Start is called before the first frame update
    void Start()
    {
        _isLeftDown = false;
        _isRightDown = false;

        _currLeftFlex = 0.0F;
        _currRightFlex = 0.0F;
        
        OnLeftStart.RemoveAllListeners();
        OnLeftFinish.RemoveAllListeners();
        
        OnRightStart.RemoveAllListeners();
        OnRightFinish.RemoveAllListeners();
    }

    // Update is called once per frame
    void Update()
    {
        var prevFlexLeft = _currLeftFlex;
        var prevFlexRight = _currRightFlex;

        _currLeftFlex = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, LeftController);
        _currRightFlex = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, RightController);

        CheckForGrabOrReleaseLeft(_currLeftFlex, prevFlexLeft);
        CheckForGrabOrReleaseRight(_currRightFlex, prevFlexRight);

        if (_isLeftDown && DidLeftControllerMove())
        {
            OnLeftMovedWhenDown.Invoke();
        }

        if (_isRightDown && DidRightControllerMove())
        {
            OnRightMovedWhenDown.Invoke();
        }
    }

    private void CheckForGrabOrReleaseLeft(float currFlex, float prevFlex)
    {
        if ((currFlex >= grabBegin) && (prevFlex < grabBegin))
        {
            HandleGrabBeginLeft();
        }
        else if ((currFlex <= grabEnd) && (prevFlex > grabEnd))
        {
            HandleGrabEndLeft();
        }
    }
    
    private void CheckForGrabOrReleaseRight(float currFlex, float prevFlex)
    {
        if ((currFlex >= grabBegin) && (prevFlex < grabBegin))
        {
            HandleGrabBeginRight();
        }
        else if ((currFlex <= grabEnd) && (prevFlex > grabEnd))
        {
            HandleGrabEndRight();
        }
    }

    private bool DidLeftControllerMove()
    {
        return LeftRaycaster.LastKnownPointChange.magnitude >= pointDistanceChangeMovementThresh;
    }

    private bool DidRightControllerMove()
    {
        return RightRaycaster.LastKnownPointChange.magnitude >= pointDistanceChangeMovementThresh;
    }
    
    private void HandleGrabBeginLeft()
    {
        if (_isRightDown || _isLeftDown)
            return;

        _isLeftDown = true;
        OnLeftStart.Invoke();
    }

    private void HandleGrabEndLeft()
    {
        if (!_isLeftDown)
            return;

        OnLeftFinish.Invoke();
        _isLeftDown = false;
    }

    private void HandleGrabBeginRight()
    {
        if (_isLeftDown || _isRightDown)
            return;

        _isRightDown = true;
        OnRightStart.Invoke();
    }

    private void HandleGrabEndRight()
    {
        if (!_isRightDown)
            return;

        OnRightFinish.Invoke();
        _isRightDown = false;
    }
}
