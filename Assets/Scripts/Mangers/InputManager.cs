using System;
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
    public Action PausePressed { get; set; }
    public Action InventoryPressed { get; set; }
    public Action EscapeDisplayPressed { get; set; }
    public Action InteractPressed { get; set; }

    [SerializeField] private InputActionAsset inputActions;

    private InputAction m_moveAction, m_lookAction, m_jumpAction, m_sprintAction, m_pauseActionPlayer, m_fireAction, m_zoomInAction, m_inventoryActionPlayer, m_interactAction;
    private InputAction m_pauseActionUI, m_inventoryActionUI, m_escapeActionUI;

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
        m_inventoryActionPlayer = playerMap.FindAction("Inventory");
        m_interactAction = playerMap.FindAction("Interact");

        m_pauseActionUI = uiMap.FindAction("Pause");
        m_inventoryActionUI = uiMap.FindAction("Inventory");
        m_escapeActionUI = uiMap.FindAction("EscapeDisplay");

        playerMap.Enable();
    }

    void Start()
    {
        InteractPressed += InputInteract;
        EscapeDisplayPressed += InputEscapeDisplay;
    }

    private void Update()
    {
        MoveInput = m_moveAction.ReadValue<Vector2>();
        LookInput = m_lookAction.ReadValue<Vector2>();
        JumpPressed = m_jumpAction.WasPressedThisFrame();
        SprintHeld = m_sprintAction.IsPressed();
        FireClicked = m_fireAction.WasPressedThisFrame();
        FireHeld = m_fireAction.IsPressed();
        //ZoomInHeld = m_zoomInAction.IsPressed();
        if (m_zoomInAction.WasPressedThisFrame())
            ZoomInHeld = true;
        else if (m_zoomInAction.WasReleasedThisFrame())
            ZoomInHeld = false;

        if (m_pauseActionPlayer.WasPressedThisFrame() || m_pauseActionUI.WasPressedThisFrame())
            PausePressed?.Invoke();

        if (m_inventoryActionPlayer.WasPressedThisFrame() || m_inventoryActionUI.WasPressedThisFrame())
            InventoryPressed?.Invoke();
            
        if (m_interactAction.WasPressedThisFrame())
            InteractPressed?.Invoke();

        if (m_escapeActionUI.WasPressedThisFrame())
            EscapeDisplayPressed?.Invoke();
    }

    private void LateUpdate()
    {
        SetCursorAndActionMapState();
    }

    private void SetCursorAndActionMapState()
    {
        if (UIManager.Instance.DisplayOpened)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            inputActions.FindActionMap("Player").Disable();
            inputActions.FindActionMap("UI").Enable();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            inputActions.FindActionMap("UI").Disable();
            inputActions.FindActionMap("Player").Enable();
        }
    }

    private void InputInteract()
    {
        if (PlayerManager.instance.NearInteractionObject == null)
            return;

        PlayerManager.instance.NearInteractionObject.OnInteract();
    }

    private void InputEscapeDisplay()
    {
        if (inputActions.FindActionMap("UI").enabled)
        {
            inputActions.FindActionMap("UI").Disable();
            inputActions.FindActionMap("Player").Enable();
        }
    }
}
