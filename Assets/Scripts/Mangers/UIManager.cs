using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    [SerializeField]
    private GameObject pauseDisplay;
    [SerializeField]
    private GameObject inventoryDisplay;

    [Header("Sub Canvas")]
    [SerializeField]
    private Canvas subCanvas;
    [SerializeField]
    private GameObject craftingPanelDisplay;

    public bool DisplayOpened
    {
        get
        {
            return pauseDisplay.activeSelf || inventoryDisplay.activeSelf || craftingPanelDisplay.activeSelf;
        }
    }

    void Start()
    {
        if (pauseDisplay == null || inventoryDisplay == null)
        {
            Debug.LogError("Pause or Inventory display is not assigned in UIManager.");
            return;
        }
        pauseDisplay.SetActive(false);
        inventoryDisplay.SetActive(false);

        InputManager.Instance.PausePressed += DisplayPause;
        InputManager.Instance.InventoryPressed += DisplayInventory;
        InputManager.Instance.CraftingPressed += DisplayCrafting;
        InputManager.Instance.EscapeDisplayPressed += EscapeDisplay;
    }

    private void DisplayPause()
    {
        pauseDisplay.SetActive(!pauseDisplay.activeSelf);
    }

    private void DisplayInventory()
    {
        inventoryDisplay.SetActive(!inventoryDisplay.activeSelf);
    }

    private void DisplayCrafting()
    {
        craftingPanelDisplay.SetActive(!craftingPanelDisplay.activeSelf);
    }

    private void EscapeDisplay()
    {
        pauseDisplay.SetActive(false);
        inventoryDisplay.SetActive(false);
    }
}
