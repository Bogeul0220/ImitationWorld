using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class P_MovementController : MonoBehaviour
{
    private Vector2 m_moveAmt;
    private Vector2 m_lookAmt;

    private Animator m_animator;
    private float _animationBlend;
    private CharacterController m_characterController;

    [Header("Jump & Grounded")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public float JumpTimeout = 0.50f;
    public float FallTimeout = 0.15f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    [Header("Cinemachine")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;

    public float WalkSpeed = 2;
    public float SprintSpeed = 6;
    public float SpeedChangeRate = 10.0f;
    public float RotateSpeed = 5;
    public float JumpSpeed = 5;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }

    private void Update()
    {
        m_moveAmt = InputManager.Instance.MoveInput;
        m_lookAmt = InputManager.Instance.LookInput;

        JumpAndGravity();
        GroundedCheck();
        Walking();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void Walking()
    {
        float targetSpeed = InputManager.Instance.SprintHeld ? SprintSpeed : WalkSpeed;

        if (m_moveAmt == Vector2.zero) targetSpeed = 0.0f;

        Vector3 forward = CinemachineCameraTarget.transform.forward;
        Vector3 right = CinemachineCameraTarget.transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * m_moveAmt.y + right * m_moveAmt.x;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, RotateSpeed * Time.deltaTime);
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        m_characterController.Move(moveDirection.normalized * (targetSpeed * Time.deltaTime) +
                                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        m_animator.SetFloat("Speed", _animationBlend);
        m_animator.SetFloat("MotionSpeed", 1f);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (m_animator != null)
            {
                m_animator.SetBool("Jump", false);
                m_animator.SetBool("FreeFall", false);
            }

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (InputManager.Instance.JumpPressed && _jumpTimeoutDelta <= 0.0f)
            {
                // 원하는 높이 H에 도달하기 위해 필요한 속도는 √(H × -2 × G)이다.
                // H : 도달하려는 원하는 높이 (높이 값, 예: 미터 단위)
                // G : 중력 가속도 (보통 음수 값, 예: -9.81 m/s²)
                // velocity : 물체가 그 높이까지 올라가기 위해 처음에 필요한 속도
                // 따라서 식은 velocity = √(H × 19.62) 처럼 양수 값의 제곱근을 구하는 공식
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                if (m_animator != null)
                    m_animator.SetBool("Jump", true);
            }

            if (_jumpTimeoutDelta >= 0.0f)
                _jumpTimeoutDelta -= Time.deltaTime;
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (m_animator != null)
                    m_animator.SetBool("FreeFall", false);
            }
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void GroundedCheck()
    {
        // 바닥에 닿아 있는지 확인하기 위해 CheckSphere를 사용하여 구를 생성합니다.
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        if (m_animator != null)
        {
            m_animator.SetBool("Grounded", Grounded);
        }
    }

    private void CameraRotation()
    {
        if (m_lookAmt.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            _cinemachineTargetYaw += m_lookAmt.x * 1.0f;
            _cinemachineTargetPitch += m_lookAmt.y * 1.0f;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void OnFootstep()
    {

    }

    private void OnLand(AnimationEvent animationEvent)
    {
        /*
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
        */
    }
}
