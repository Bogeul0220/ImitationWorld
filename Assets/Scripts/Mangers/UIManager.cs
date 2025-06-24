using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

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

    public Sprite currentBallBackground;
    public Sprite otherBallBackground;

    public BallQuickSlot[] ballQuickSlots = new BallQuickSlot[3];

    [SerializeField]
    private GameObject pauseDisplay;
    [SerializeField]
    private GameObject inventoryDisplay;
    [SerializeField]
    private TMP_Text interactionText;
    [SerializeField]
    private PlayerStatusUI playerStatusUI;
    [SerializeField] private Image creatureImage;
    [SerializeField] private Image creatureImageBorder;


    [Header("Sub Canvas")]
    [SerializeField]
    private Canvas subCanvas;
    public GameObject craftingPanelDisplay;

    [Header("Field UI")]
    [SerializeField] private GameObject damageFloatingPrefab;

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
        BallTargetUpdate(InputManager.Instance.SelectedBallIndex);

        InputManager.Instance.PausePressed += DisplayPause;
        InputManager.Instance.InventoryPressed += DisplayInventory;
        InputManager.Instance.EscapeDisplayPressed += EscapeDisplay;
    }

    void LateUpdate()
    {
        SetInteractionText();
        DisplayCreatureImage();
    }

    public void SetInteractionText()
    {
        if (PlayerManager.Instance.NearInteractionObject != null && DisplayOpened == false)
        {
            interactionText.text = $"[E] {PlayerManager.Instance.NearInteractionObject.InteractionObjectName}";
        }
        else
        {
            interactionText.text = "";
        }
    }

    private void DisplayPause()
    {
        pauseDisplay.SetActive(!pauseDisplay.activeSelf);
        playerStatusUI.gameObject.SetActive(!playerStatusUI.gameObject.activeSelf);
    }

    private void DisplayInventory()
    {
        inventoryDisplay.SetActive(!inventoryDisplay.activeSelf);
        playerStatusUI.gameObject.SetActive(!playerStatusUI.gameObject.activeSelf);
    }

    public void DisplayInteractCraft()
    {
        if (PlayerManager.Instance.NearInteractionObject == null)
            return;

        Debug.Log("DisplayInteract called");
        craftingPanelDisplay.SetActive(!craftingPanelDisplay.activeSelf);
        playerStatusUI.gameObject.SetActive(!playerStatusUI.gameObject.activeSelf);
    }

    private void EscapeDisplay()
    {
        pauseDisplay.SetActive(false);
        inventoryDisplay.SetActive(false);
        craftingPanelDisplay.SetActive(false);
        
        playerStatusUI.gameObject.SetActive(true);
    }

    public void BallTargetUpdate(int currentNum)
    {
        for (int i = 0; i < ballQuickSlots.Length; i++)
        {
            if (i == currentNum)
            {
                ballQuickSlots[i].Border.gameObject.SetActive(true);
                ballQuickSlots[i].BackgroundImage.sprite = currentBallBackground;
                ballQuickSlots[i].GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                ballQuickSlots[i].Border.gameObject.SetActive(false);
                ballQuickSlots[i].BackgroundImage.sprite = otherBallBackground;
                ballQuickSlots[i].GetComponent<RectTransform>().localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }
    }
    
    public void DisplayDamageFloating(int damage, Vector3 position)
    {
        var floating = ObjectPoolManager.Get<DamageFloating>(damageFloatingPrefab);
        floating.transform.position = position;
        floating.SetDamage(damage);
    }

    public void DisplayCreatureImage()
    {
        if(CreatureManager.Instance.SpawnedTamedCreatures.Count <= 0)
        {
            creatureImage.gameObject.SetActive(false);
            creatureImageBorder.gameObject.SetActive(false);
            return;
        }
        if(InputManager.Instance.SelectedAllyCreature == -1)
        {
            creatureImage.gameObject.SetActive(false);
            creatureImageBorder.gameObject.SetActive(false);
            return;
        }

        var key = CreatureManager.Instance.SpawnedTamedKey[InputManager.Instance.SelectedAllyCreature];

        var targetCreature = CreatureManager.Instance.SpawnedTamedCreatures[key];

        if(CreatureManager.Instance.CurrentTakeOutCreature != null && CreatureManager.Instance.CurrentTakeOutCreature.CreatureIndex == targetCreature.CreatureIndex)
        {
            creatureImage.gameObject.SetActive(true);
            creatureImage.sprite = targetCreature.CreatureImage;
            creatureImageBorder.gameObject.SetActive(true);
        }
        else
        {
            creatureImage.gameObject.SetActive(true);
            creatureImage.sprite = targetCreature.CreatureImage;
            creatureImageBorder.gameObject.SetActive(false);
        }
    }
}
