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
    [SerializeField] private TMP_Text creatureNameText;
    [SerializeField] private Image creatureImageBorder;
    [SerializeField] private Image prevCreatureImage;
    [SerializeField] private Image nextCreatureImage;


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
                ballQuickSlots[i].gameObject.SetActive(true);
                ballQuickSlots[i].GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                ballQuickSlots[i].gameObject.SetActive(false);
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
        if (CreatureManager.Instance.SpawnedTamedCreatures.Count <= 0)
        {
            creatureImage.gameObject.SetActive(false);
            creatureImageBorder.gameObject.SetActive(false);
            prevCreatureImage.gameObject.SetActive(false);
            nextCreatureImage.gameObject.SetActive(false);
            creatureNameText.text = "";
            return;
        }

        var currentKey = CreatureManager.Instance.SpawnedTamedKey[InputManager.Instance.SelectedAllyCreature];
        var currentCreature = CreatureManager.Instance.SpawnedTamedCreatures[currentKey];

        if (currentCreature != null)
        {
            creatureImage.gameObject.SetActive(true);
            creatureNameText.text = currentCreature.CreatureName;
            creatureImage.sprite = currentCreature.CreatureImage;
            if (CreatureManager.Instance.CurrentTakeOutCreature != null && CreatureManager.Instance.CurrentTakeOutCreature.CreatureIndex == currentCreature.CreatureIndex)
                creatureImageBorder.gameObject.SetActive(true);
            else
                creatureImageBorder.gameObject.SetActive(false);

            if (CreatureManager.Instance.SpawnedTamedKey.Count > 1)
            {
                int totalCount = CreatureManager.Instance.SpawnedTamedKey.Count;
                int currentIndex = InputManager.Instance.SelectedAllyCreature;

                // 이전 크리쳐 인덱스
                int prevIndex = (currentIndex - 1 + totalCount) % totalCount;
                var prevKey = CreatureManager.Instance.SpawnedTamedKey[prevIndex];
                var prevCreature = CreatureManager.Instance.SpawnedTamedCreatures[prevKey];

                // 다음 크리쳐 인덱스
                int nextIndex = (currentIndex + 1) % totalCount;
                var nextKey = CreatureManager.Instance.SpawnedTamedKey[nextIndex];
                var nextCreature = CreatureManager.Instance.SpawnedTamedCreatures[nextKey];

                if(totalCount == 2)
                {
                    if(currentIndex == 0)
                    {
                        prevCreatureImage.gameObject.SetActive(false);
                        nextCreatureImage.gameObject.SetActive(true);
                        nextCreatureImage.sprite = nextCreature.CreatureImage;
                    }
                    else
                    {
                        prevCreatureImage.gameObject.SetActive(true);
                        prevCreatureImage.sprite = prevCreature.CreatureImage;
                        nextCreatureImage.gameObject.SetActive(false);
                    }
                }
                else
                {
                    prevCreatureImage.gameObject.SetActive(true);
                    prevCreatureImage.sprite = prevCreature.CreatureImage;
                    nextCreatureImage.gameObject.SetActive(true);
                    nextCreatureImage.sprite = nextCreature.CreatureImage;
                }
            }
            else
            {
                prevCreatureImage.gameObject.SetActive(false);
                nextCreatureImage.gameObject.SetActive(false);
            }

            // if(CreatureManager.Instance.CurrentTakeOutCreature != null && CreatureManager.Instance.CurrentTakeOutCreature.CreatureIndex == targetCreature.CreatureIndex)
            // {
            //     creatureImage.gameObject.SetActive(true);
            //     creatureImage.sprite = targetCreature.CreatureImage;
            //     creatureImageBorder.gameObject.SetActive(true);
            //     if(CreatureManager.Instance.SpawnedTamedKey.Count > 1)
            //     {
            //         var prevKey = CreatureManager.Instance.SpawnedTamedKey[InputManager.Instance.SelectedAllyCreature - 1];
            //         var prevCreature = CreatureManager.Instance.SpawnedTamedCreatures[prevKey];
            //         if(prevCreature != null)
            //         {
            //             prevCreatureImage.gameObject.SetActive(true);
            //             prevCreatureImage.sprite = prevCreature.CreatureImage;
            //         }
            //         else
            //         {
            //             prevCreatureImage.gameObject.SetActive(false);
            //         }

            //         var nextKey = CreatureManager.Instance.SpawnedTamedKey[InputManager.Instance.SelectedAllyCreature + 1];
            //         var nextCreature = CreatureManager.Instance.SpawnedTamedCreatures[nextKey];
            //         if(nextCreature != null)
            //         {
            //             nextCreatureImage.gameObject.SetActive(true);
            //             nextCreatureImage.sprite = nextCreature.CreatureImage;
            //         }
            //         else
            //         {
            //             nextCreatureImage.gameObject.SetActive(false);
            //         }
            //     }
            // }
            // else
            // {
            //     creatureImage.gameObject.SetActive(true);
            //     creatureImage.sprite = targetCreature.CreatureImage;
            //     creatureImageBorder.gameObject.SetActive(false);
            // }
        }
    }
}
