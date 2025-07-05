using System;
using System.Net.WebSockets;
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
    public bool ThrowBallHeld { get; private set; }
    public Action PausePressed { get; set; }
    public Action InventoryPressed { get; set; }
    public Action EscapeDisplayPressed { get; set; }
    public Action InteractPressed { get; set; }
    public int SelectedWeaponNum { get; set; }
    public int SelectedBallIndex { get; set; }
    public bool SpawnedTamedCreature { get; set; }
    public int SelectedAllyCreature { get; set; }
    public bool SpawnAllyHeld { get; set; }
    public Action CallInAllyPressed { get; set; }

    [SerializeField] private InputActionAsset inputActions;

    private InputAction m_moveAction, m_lookAction, m_jumpAction, m_sprintAction, m_pauseActionPlayer,
        m_fireAction, m_zoomInAction, m_inventoryActionPlayer, m_interactAction, m_selectedWeaponInput,
        m_throwBallAction, m_changeBallAction, m_selectAlly, m_spawnAlly;

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
        m_selectedWeaponInput = playerMap.FindAction("SelectedWeapon");
        m_throwBallAction = playerMap.FindAction("ThrowBall");
        m_changeBallAction = playerMap.FindAction("ChangeBallScroll");
        m_selectAlly = playerMap.FindAction("SelectAlly");
        m_spawnAlly = playerMap.FindAction("SpawnAlly");

        m_pauseActionUI = uiMap.FindAction("Pause");
        m_inventoryActionUI = uiMap.FindAction("Inventory");
        m_escapeActionUI = uiMap.FindAction("EscapeDisplay");

        playerMap.Enable();
    }

    void Start()
    {
        InteractPressed += InputInteract;
        EscapeDisplayPressed += InputEscapeDisplay;
        SelectedAllyCreature = 0;
    }

    private void Update()
    {
        var creatureList = CreatureManager.Instance.TamedCreatureKey;

        MoveInput = m_moveAction.ReadValue<Vector2>();
        LookInput = m_lookAction.ReadValue<Vector2>();
        JumpPressed = m_jumpAction.WasPressedThisFrame();
        SprintHeld = m_sprintAction.IsPressed();
        FireClicked = m_fireAction.WasPressedThisFrame();
        FireHeld = m_fireAction.IsPressed();

        if (InventoryManager.Instance.HasEnoughBall())
            ThrowBallHeld = m_throwBallAction.IsPressed();

        if (CreatureManager.Instance.TamedCreatureKey.Count > 0 && m_selectAlly.WasPerformedThisFrame())
        {
            float allySelectValue = m_selectAlly.ReadValue<float>();

            if (CreatureManager.Instance.TamedCreatureKey.Count <= 1)
                return;

            if (allySelectValue > 0f)  // C키 입력
            {
                if (SelectedAllyCreature >= CreatureManager.Instance.TamedCreatureKey.Count - 1)
                    SelectedAllyCreature = 0;
                else
                    SelectedAllyCreature++;
                Debug.Log("C키 입력");
            }
            else if (allySelectValue < 0f) // Z키 입력
            {
                if (SelectedAllyCreature <= 0)
                    SelectedAllyCreature = CreatureManager.Instance.TamedCreatureKey.Count - 1;
                else
                    SelectedAllyCreature--;
                Debug.Log("Z키 입력");
            }
        }

        if (creatureList.Count > 0 && SelectedAllyCreature != -1)
        {
            if (CreatureManager.Instance.RetireAllyReviveProgress.ContainsKey(CreatureManager.Instance.TamedCreatureKey[SelectedAllyCreature]))
            {
                return;
            }
            
            if (m_spawnAlly.WasPressedThisFrame() && CreatureManager.Instance.CurrentTakeOutCreature != null)
            {
                CallInAllyPressed?.Invoke();
                return;
            }

            SpawnAllyHeld = m_spawnAlly.IsPressed();
        }

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

        Vector2 scrollValue = m_changeBallAction.ReadValue<Vector2>();
        if (scrollValue.y > 0f)
        {
            SelectedBallIndex = (SelectedBallIndex + 1) % 3;
            UIManager.Instance.BallTargetUpdate(SelectedBallIndex);
        }
        else if (scrollValue.y < 0f)
        {
            SelectedBallIndex = (SelectedBallIndex + 2) % 3;
            UIManager.Instance.BallTargetUpdate(SelectedBallIndex);
        }

        if (m_selectedWeaponInput.triggered)
        {
            var control = m_selectedWeaponInput.activeControl;
            int newSelectedWeaponNum = -1;

            if (control == Keyboard.current.digit1Key) newSelectedWeaponNum = 0;
            else if (control == Keyboard.current.digit2Key) newSelectedWeaponNum = 1;
            else if (control == Keyboard.current.digit3Key) newSelectedWeaponNum = 2;
            else if (control == Keyboard.current.digit4Key) newSelectedWeaponNum = 3;

            if (newSelectedWeaponNum != -1)
            {
                if (newSelectedWeaponNum != SelectedWeaponNum)
                {
                    SelectedWeaponNum = newSelectedWeaponNum;
                }
                PlayerManager.Instance.ToggleWeapon(newSelectedWeaponNum);
            }
        }

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
        if (PlayerManager.Instance.NearInteractionObject == null)
            return;

        PlayerManager.Instance.NearInteractionObject.OnInteract();
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
