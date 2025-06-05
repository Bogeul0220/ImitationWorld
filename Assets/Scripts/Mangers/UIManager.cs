using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField]
    private TMP_Text interactionText;

    [Header("Sub Canvas")]
    [SerializeField]
    private Canvas subCanvas;
    public GameObject craftingPanelDisplay;

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
        InputManager.Instance.EscapeDisplayPressed += EscapeDisplay;
    }

    void LateUpdate()
    {
        SetInteractionText();
    }

    public void SetInteractionText()
    {
        if (PlayerManager.instance.NearInteractionObject != null && DisplayOpened == false)
        {
            interactionText.text = $"[E] {PlayerManager.instance.NearInteractionObject.InteractionObjectName}";
        }
        else
        {
            interactionText.text = "";
        }
    }

    private void DisplayPause()
    {
        pauseDisplay.SetActive(!pauseDisplay.activeSelf);
    }

    private void DisplayInventory()
    {
        inventoryDisplay.SetActive(!inventoryDisplay.activeSelf);
    }

    public void DisplayInteractCraft()
    {
        if (PlayerManager.instance.NearInteractionObject == null)
            return;

        Debug.Log("DisplayInteract called");
        craftingPanelDisplay.SetActive(!craftingPanelDisplay.activeSelf);
    }

    private void EscapeDisplay()
    {
        pauseDisplay.SetActive(false);
        inventoryDisplay.SetActive(false);
        craftingPanelDisplay.SetActive(false);
    }
}
