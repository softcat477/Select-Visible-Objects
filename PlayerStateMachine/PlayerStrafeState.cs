using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Cinemachine.Utility;

public class PlayerStrafeState : IState
{
    Vector2 moveVector;
    public PlayerStrafeState(PlayerStateMachine psm) : base(psm) {

    }
    public override void EnterState() {
        Debug.Log("Enter Strafe state");
        _psm.animator.CrossFade("Strafe", 0.2f);

        _psm.cameraFreeLook.SetActive(false);
        _psm.cameraFocus.SetActive(true);
    }
    public override void UpdateState() {
        moveVector = _psm.ReadMoveInput();
        float mag = moveVector.magnitude;

        _psm.animator.SetFloat("velocity.x", moveVector.x);
        _psm.animator.SetFloat("velocity.y", moveVector.y);
        // transform to camera space
        Vector2 moveVectorCameraSpace = toCameraSpace(moveVector);
        Vector3 dSpeed = new Vector3(moveVectorCameraSpace.x, 0, moveVectorCameraSpace.y);
        dSpeed = dSpeed * _psm.strafeSpeed;
        _psm.charV.x = dSpeed.x;
        _psm.charV.z = dSpeed.z;

        Quaternion targetRotation;
        if (_psm.select == null) {
            targetRotation = Quaternion.LookRotation(new Vector3(_psm.followCamera.transform.forward.x, 
                0, 
                _psm.followCamera.transform.forward.z));
        }
        else {
            Vector3 directionToSubject = _psm.select.transform.position - _psm.transform.position;
            targetRotation = Quaternion.LookRotation(new Vector3(directionToSubject.x,
                0,
                directionToSubject.z));
        }
        Quaternion currentRotation = _psm.transform.rotation;
        _psm.transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _psm.rotatePerFrame * Time.deltaTime);
        CheckSwitchStates();
    }
    public override void ExitState() {
        _psm.animator.CrossFade("Idle", 0.2f);
        _psm.cameraFreeLook.SetActive(true);
        _psm.cameraFocus.SetActive(false);

        bool recenter = false;
 
        if (recenter)
        {
            // Disable the transposers while recentering
            // for (int i = 0; i < 3; ++i)
            //     orbital[i].enabled = !recenter;
            CinemachineFreeLook vcam = _psm.cameraFreeLook.GetComponent<CinemachineFreeLook>();
            vcam.m_YAxis.Value = 0.5f;
            // tmp.m_XAxis.Value = 0.0f;
            // Debug.Log("m_XAxis.Value: " + tmp.m_XAxis.Value);

            CinemachineOrbitalTransposer[] orbital = new CinemachineOrbitalTransposer[3];
            CinemachineVirtualCamera[] rigs = new CinemachineVirtualCamera[3];

            for (int i = 0; vcam != null && i < 3; ++i)
            {
                rigs[i] = vcam.GetRig(i);
                orbital[i] = rigs[i].GetCinemachineComponent<CinemachineOrbitalTransposer>();
                string s = $"rigs[{i}].transform.position: {rigs[i].transform.position}";
                Debug.Log(s);
            }

            // How far away from centered are we?
            Transform target = vcam.Follow;
            Vector3 up = vcam.State.ReferenceUp;
            Vector3 back = vcam.transform.position - target.position;
            // float angle = UnityVectorExtensions.SignedAngle(
            //     back.ProjectOntoPlane(up), -target.forward.ProjectOntoPlane(up), up);
            float angle = UnityVectorExtensions.SignedAngle(
                target.transform.forward.ProjectOntoPlane(up), Vector3.forward, Vector3.up);
            if (Mathf.Abs(angle) < UnityVectorExtensions.Epsilon)
                recenter = false; // done!
 
            Debug.Log("angle: " + angle);
            vcam.m_XAxis.Value = -angle;
            // Do the recentering on all 3 rigs
            // angle = Damper.Damp(angle, 1.0f, Time.deltaTime);
            // for (int i = 0; recenter && i < 3; ++i)
            // {
            //     Vector3 pos = rigs[i].transform.position - target.position;
            //     pos = Quaternion.AngleAxis(angle, up) * pos;
            //     string s = $"Set rigs[{i}].transform.position from {rigs[i].transform.position} to {pos + target.position}";
            //     Debug.Log(s);
            //     rigs[i].transform.position = pos + target.position;
            //     Debug.Log(rigs[i].transform.position);
            // }
        }
    }
    public override void CheckSwitchStates() {
        if (!_psm.isStrafing) {
            IState nextState = PlayerIdleState.Create(_psm);
            SwitchState(nextState);
        }
    }

    static public PlayerStrafeState Create(PlayerStateMachine psm) {
        return new PlayerStrafeState(psm);
    }

    private Vector2 toCameraSpace(Vector2 vec) {
        Vector3 flat_world_forward = new Vector3(Vector3.forward.x, 0, Vector3.forward.z);
        Vector3 flat_camera_forward = new Vector3(_psm.followCamera.transform.forward.x, 0, _psm.followCamera.transform.forward.z);

        Quaternion rot = Quaternion.FromToRotation(flat_world_forward, flat_camera_forward);
        Vector3 before = new Vector3(vec.x, 0, vec.y);
        Vector3 after = rot * before;
        return new Vector2(after.x, after.z);
    }
}
