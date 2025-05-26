using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool FireClicked { get; private set; }
    public bool FireHeld { get; private set; }
    public bool ZoomInHeld { get; private set; }
    public bool PausePressed { get; private set; }

    [SerializeField] private InputActionAsset inputActions;

    private InputAction m_moveAction, m_lookAction, m_jumpAction, m_sprintAction, m_pauseActionPlayer, m_fireAction, m_zoomInAction;
    private InputAction m_pauseActionUI;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        var playerMap = inputActions.FindActionMap("Player");
        if (playerMap == null)
        {
            Debug.LogError("Player action map not found in InputActionAsset.");
            return;
        }
        var uiMap = inputActions.FindActionMap("UI");
        if (uiMap == null)
        {
            Debug.LogError("UI action map not found in InputActionAsset.");
            return;
        }

        m_moveAction = playerMap.FindAction("Move");
        m_lookAction = playerMap.FindAction("Look");
        m_jumpAction = playerMap.FindAction("Jump");
        m_sprintAction = playerMap.FindAction("Sprint");
        m_pauseActionPlayer = playerMap.FindAction("Pause");
        m_fireAction = playerMap.FindAction("Fire");
        m_pauseActionPlayer = playerMap.FindAction("Pause");
        m_zoomInAction = playerMap.FindAction("ZoomIn");

        m_pauseActionUI = uiMap.FindAction("Pause");

        playerMap.Enable();
    }

    private void Update()
    {
        MoveInput = m_moveAction.ReadValue<Vector2>();
        LookInput = m_lookAction.ReadValue<Vector2>();
        JumpPressed = m_jumpAction.WasPressedThisFrame();
        SprintHeld = m_sprintAction.IsPressed();
        PausePressed = m_pauseActionPlayer.WasPressedThisFrame();
        FireClicked = m_fireAction.WasPressedThisFrame();
        FireHeld = m_fireAction.IsPressed();
        //ZoomInHeld = m_zoomInAction.IsPressed();
        if (m_zoomInAction.WasPressedThisFrame())
            ZoomInHeld = true;
        else if (m_zoomInAction.WasReleasedThisFrame())
            ZoomInHeld = false;
    }

    private void LateUpdate()
    {
        InputPause();
    }

    private void InputPause()
    {
        if (PausePressed)
        {
            if (inputActions.FindActionMap("Player").enabled)
            {
                inputActions.FindActionMap("Player").Disable();
                inputActions.FindActionMap("UI").Enable();
            }
            else
            {
                inputActions.FindActionMap("UI").Disable();
                inputActions.FindActionMap("Player").Enable();
            }
        }
    }
}
