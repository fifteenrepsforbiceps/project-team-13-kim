using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using JUTPSActions;
using JUTPS.JUInputSystem;

public class JUDashSkill : JUTPSAnimatedAction
{
    [Header("Animation Settings")]
    [SerializeField] private string DashStateName = "Dash";
    public int AnimatorLayerIndex = 9;


    [Header("Dash Settings")]
    public float DashSpeed;
    public AnimationCurve ForwardMovementCurve, SidesMovementCurve, UpMovementCurve;
    public bool UseGravity = true;
    //Double Shift Dash
    public bool UseDoubleRunButtonToDash = true;
    public bool RotateToDesiredDirection = true;

    //Input
    public JUTPS.InputEvents.MultipleActionEvent CustomInputs;
    //Dash Timer
    public float TimeToUseDashAgain = 1;
    [Header("Unity Events")]
    public UnityEvent OnDash;
    public UnityEvent OnEndDash;

    [Header("IK Disabling")]
    public bool DisableIK = true;
    public float TimeToEnableIK = 0.4f;

    [Header("State")]
    public bool CanDash = true;

    private void OnEnable()
    {
        if(CustomInputs.Actions.Count > 0)
        {
            CustomInputs.Enable();
            CustomInputs.OnButtonsDown.AddListener(StartDash);
        }
    }
    private void OnDisable()
    {
        if (CustomInputs.Actions.Count > 0)
        {
            CustomInputs.Disable();
            CustomInputs.OnButtonsDown.RemoveListener(StartDash);
        }
    }

    private void Start()
    {
        SwitchAnimationLayer(AnimatorLayerIndex);
    }



    public override void ActionCondition()
    {
        // DoubleShiftToDash();
    }
    public override void OnActionStarted()
    {
        if (RotateToDesiredDirection) transform.rotation = TPSCharacter.DirectionTransform.rotation;
        DisableCharacterMovement();
        PlayAnimation(DashStateName);
        if (DisableIK) { TPSCharacter.InverseKinematics = false; CancelInvoke(nameof(enableIK)); }

        //Disable Dash
        if (TimeToUseDashAgain > 0) { CancelInvoke(nameof(enableDash)); disableDash(); }

        //On Start Dash Event Call
        OnDash.Invoke();
    }
    public override void OnActionEnded()
    {
        TPSCharacter.enableMove();
        if(DisableIK)Invoke(nameof(enableIK), TimeToEnableIK);

        //Enable Dash
        if (TimeToUseDashAgain > 0) { Invoke(nameof(enableDash), TimeToUseDashAgain); }

        //On Stop Dash Event Call
        OnEndDash.Invoke();
    }



    public void StartDash()
    {
        if (CanDash == false) return;
        StartAction();
    }
    private void enableIK() { TPSCharacter.InverseKinematics = true; }
    private void enableDash() => CanDash = true;
    private void disableDash() => CanDash = false;



    public override void OnActionIsPlaying()
    {
        float ActionCurrentDuration = ActionCurrentTime / ActionDuration;
        if (UseGravity)
        {
            Vector3 forwardMovement = transform.forward * ForwardMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            Vector3 sidesMovement = transform.right * SidesMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            Vector3 gravityMovement = Vector3.up * rb.velocity.y;
            rb.velocity = forwardMovement + sidesMovement + gravityMovement;
        }
        else
        {
            Vector3 forwardMovement = transform.forward * ForwardMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            Vector3 sidesMovement = transform.right * SidesMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            Vector3 upMovement = transform.up * UpMovementCurve.Evaluate(ActionCurrentDuration) * DashSpeed;
            rb.velocity = forwardMovement + sidesMovement + upMovement;
        }        
    }

}
