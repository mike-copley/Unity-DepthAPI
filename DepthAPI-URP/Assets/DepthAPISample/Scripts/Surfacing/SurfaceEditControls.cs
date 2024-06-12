using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SurfaceEditControls : MonoBehaviour
{
    // Grip trigger thresholds for picking up objects, with some hysteresis.
    public float grabBegin = 0.65F;
    public float grabEnd = 0.35F;
    public float pointDistanceChangeMovementThresh = 0.01F;

    public OVRInput.Controller LeftController;
    public OVRInput.Controller RightController;

    public RaycastVisualizer LeftRaycaster;
    public RaycastVisualizer RightRaycaster;
    public RaycastVisualizer HeadRaycaster;
    
    public UnityEvent OnLeftStart;
    public UnityEvent OnLeftMovedWhenDown;
    public UnityEvent OnLeftFinish;
    public UnityEvent OnLeftSqueeze;

    public UnityEvent OnRightStart;
    public UnityEvent OnRightMovedWhenDown;
    public UnityEvent OnRightFinish;
    public UnityEvent OnRightSqueeze;
    
    private float _currLeftFlex = 0.0F;
    private float _currLeftSqueezeFlex = 0.0F;

    private float _currRightFlex = 0.0F;
    private float _currRightSqueezeFlex = 0.0F;
    
    private bool _isLeftDown = false;
    private bool _isRightDown = false;
    
    // Start is called before the first frame update
    void Start()
    {
        _isLeftDown = false;
        _isRightDown = false;

        _currLeftFlex = 0.0F;
        _currLeftSqueezeFlex = 0.0F;
        
        _currRightFlex = 0.0F;
        _currRightSqueezeFlex = 0.0F;
        
        OnLeftStart.RemoveAllListeners();
        OnLeftMovedWhenDown.RemoveAllListeners();
        OnLeftFinish.RemoveAllListeners();
        OnLeftSqueeze.RemoveAllListeners();
        
        OnRightStart.RemoveAllListeners();
        OnRightMovedWhenDown.RemoveAllListeners();
        OnRightFinish.RemoveAllListeners();
        OnRightSqueeze.RemoveAllListeners();
    }

    // Update is called once per frame
    void Update()
    {
        var prevFlexLeft = _currLeftFlex;
        var prevSqueezeFlexLeft = _currLeftSqueezeFlex;
        
        var prevFlexRight = _currRightFlex;
        var prevSqueezeFlexRight = _currRightSqueezeFlex;
        
        _currLeftFlex = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, LeftController);
        _currLeftSqueezeFlex = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, LeftController);

        _currRightFlex = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, RightController);
        _currRightSqueezeFlex = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, RightController);

        // Debug.Log($"PAINTING: crf: {_currRightFlex}, crs: {_currRightSqueezeFlex}");
        
        CheckForGrabOrReleaseLeft(_currLeftFlex, prevFlexLeft);
        CheckForGrabOrReleaseRight(_currRightFlex, prevFlexRight);

        if (CheckForLeftSqueeze(_currLeftSqueezeFlex, prevSqueezeFlexLeft))
        {
            OnLeftSqueeze.Invoke();
        }

        if (CheckForRightSqueeze(_currRightSqueezeFlex, prevSqueezeFlexRight))
        {
            Debug.Log($"PAINTING: calling OnRightSqueeze.Invoke()");
            OnRightSqueeze.Invoke();
        }
        
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
        if ((currFlex >= grabBegin) && (prevFlex < grabEnd))
        {
            HandleGrabBeginLeft();
        }
        else if ((currFlex <= grabEnd) && (prevFlex > grabEnd))
        {
            HandleGrabEndLeft();
        }
    }

    private bool CheckForLeftSqueeze(float currFlex, float prevFlex)
    {
        if ((currFlex >= grabBegin) && (prevFlex < grabEnd))
        {
            return true;
        }
        // else if ((currFlex <= grabEnd) && (prevFlex > grabEnd))
        // {
        //     HandleGrabEndLeft();
        // }
        return false;
    }
    
    private void CheckForGrabOrReleaseRight(float currFlex, float prevFlex)
    {
        if ((currFlex >= grabBegin) && (prevFlex < grabEnd))
        {
            HandleGrabBeginRight();
        }
        else if ((currFlex <= grabEnd) && (prevFlex > grabEnd))
        {
            HandleGrabEndRight();
        }
    }
    
    private bool CheckForRightSqueeze(float currFlex, float prevFlex)
    {
        if ((currFlex >= grabBegin) && (prevFlex < grabEnd))
        {
            Debug.Log($"PAINTING: CheckForRightSqueeze() ... cf: {currFlex}, pf: {prevFlex}");
            return true;
        }
        // else if ((currFlex <= grabEnd) && (prevFlex > grabEnd))
        // {
        //     HandleGrabEndRight();
        // }
        return false;
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
        if (_isLeftDown)
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
        if (_isRightDown)
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
