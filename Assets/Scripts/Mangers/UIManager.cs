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

    [SerializeField] private GameObject pauseDisplay;
    [SerializeField] private GameObject inventoryDisplay;
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private PlayerStatusUI playerStatusUI;
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
            interactionText.text = $"[F] {PlayerManager.Instance.NearInteractionObject.InteractionObjectName}";
        }
        else
        {
            interactionText.text = "";
        }
    }

    private void DisplayPause()
    {
        if(pauseDisplay.activeSelf)
            SoundManager.Instance.PlaySFX("CloseUI");
        else
            SoundManager.Instance.PlaySFX("OpenUI");

        pauseDisplay.SetActive(!pauseDisplay.activeSelf);
        playerStatusUI.gameObject.SetActive(!playerStatusUI.gameObject.activeSelf);
    }

    private void DisplayInventory()
    {
        if(inventoryDisplay.activeSelf)
            SoundManager.Instance.PlaySFX("CloseUI");
        else
            SoundManager.Instance.PlaySFX("OpenUI");

        inventoryDisplay.SetActive(!inventoryDisplay.activeSelf);
        playerStatusUI.gameObject.SetActive(!playerStatusUI.gameObject.activeSelf);
    }

    public void DisplayInteractCraft()
    {
        if (PlayerManager.Instance.NearInteractionObject == null)
            return;

        if(craftingPanelDisplay.activeSelf)
            SoundManager.Instance.PlaySFX("CloseUI");
        else
            SoundManager.Instance.PlaySFX("OpenUI");

        craftingPanelDisplay.SetActive(!craftingPanelDisplay.activeSelf);
        playerStatusUI.gameObject.SetActive(!playerStatusUI.gameObject.activeSelf);
    }

    private void EscapeDisplay()
    {
        SoundManager.Instance.PlaySFX("CloseUI");

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
        if (CreatureManager.Instance.TamedCreatures.Count <= 0)
        {
            creatureImage.gameObject.SetActive(false);
            creatureImageBorder.gameObject.SetActive(false);
            prevCreatureImage.gameObject.SetActive(false);
            nextCreatureImage.gameObject.SetActive(false);
            creatureNameText.text = "";
            return;
        }

        var currentKey = CreatureManager.Instance.TamedCreatureKey[InputManager.Instance.SelectedAllyCreature];
        var currentCreature = CreatureManager.Instance.TamedCreatures[currentKey];

        if (currentCreature != null)
        {
            creatureImage.gameObject.SetActive(true);
            creatureNameText.text = currentCreature.CreatureName;
            creatureImage.sprite = currentCreature.CreatureImage;

            if (CreatureManager.Instance.RetireAllyReviveProgress.ContainsKey(currentKey))
            {
                creatureImageBorder.color = Color.red;
                creatureImage.fillAmount = CreatureManager.Instance.RetireAllyReviveProgress[currentKey];
            }
            else
            {
                creatureImageBorder.color = Color.white;
                creatureImage.fillAmount = 1f;
            }


            if (CreatureManager.Instance.CurrentTakeOutCreature != null && CreatureManager.Instance.CurrentTakeOutCreature.CreatureIndex == currentCreature.CreatureIndex)
                creatureImageBorder.gameObject.SetActive(true);
            else
                creatureImageBorder.gameObject.SetActive(false);

            if (CreatureManager.Instance.TamedCreatureKey.Count > 1)
            {
                int totalCount = CreatureManager.Instance.TamedCreatureKey.Count;
                int currentIndex = InputManager.Instance.SelectedAllyCreature;

                // 이전 크리쳐 인덱스
                int prevIndex = (currentIndex - 1 + totalCount) % totalCount;
                var prevKey = CreatureManager.Instance.TamedCreatureKey[prevIndex];
                var prevCreature = CreatureManager.Instance.TamedCreatures[prevKey];

                // 다음 크리쳐 인덱스
                int nextIndex = (currentIndex + 1) % totalCount;
                var nextKey = CreatureManager.Instance.TamedCreatureKey[nextIndex];
                var nextCreature = CreatureManager.Instance.TamedCreatures[nextKey];

                if (totalCount == 2)
                {
                    if (currentIndex == 0)
                    {
                        prevCreatureImage.gameObject.SetActive(false);
                        nextCreatureImage.gameObject.SetActive(true);
                        nextCreatureImage.sprite = nextCreature.CreatureImage;
                        if (CreatureManager.Instance.RetireAllyReviveProgress.ContainsKey(nextKey))
                        {
                            nextCreatureImage.color = Color.red;
                            nextCreatureImage.fillAmount = CreatureManager.Instance.RetireAllyReviveProgress[nextKey];
                        }
                        else
                            nextCreatureImage.color = Color.white;
                    }
                    else
                    {
                        prevCreatureImage.gameObject.SetActive(true);
                        prevCreatureImage.sprite = prevCreature.CreatureImage;
                        nextCreatureImage.gameObject.SetActive(false);
                        if (CreatureManager.Instance.RetireAllyReviveProgress.ContainsKey(prevKey))
                        {
                            prevCreatureImage.color = Color.red;
                            prevCreatureImage.fillAmount = CreatureManager.Instance.RetireAllyReviveProgress[prevKey];
                        }
                        else
                            prevCreatureImage.color = Color.white;
                    }
                }
                else
                {
                    prevCreatureImage.gameObject.SetActive(true);
                    prevCreatureImage.sprite = prevCreature.CreatureImage;
                    if (CreatureManager.Instance.RetireAllyReviveProgress.ContainsKey(prevKey))
                    {
                        prevCreatureImage.color = Color.red;
                        prevCreatureImage.fillAmount = CreatureManager.Instance.RetireAllyReviveProgress[prevKey];
                    }
                    else
                    {
                        prevCreatureImage.color = Color.white;
                        prevCreatureImage.fillAmount = 1f;
                    }

                    nextCreatureImage.gameObject.SetActive(true);
                    nextCreatureImage.sprite = nextCreature.CreatureImage;
                    if (CreatureManager.Instance.RetireAllyReviveProgress.ContainsKey(nextKey))
                    {
                        nextCreatureImage.color = Color.red;
                        nextCreatureImage.fillAmount = CreatureManager.Instance.RetireAllyReviveProgress[nextKey];
                    }
                    else
                    {
                        nextCreatureImage.color = Color.white;
                        nextCreatureImage.fillAmount = 1f;
                    }
                }
            }
            else
            {
                prevCreatureImage.gameObject.SetActive(false);
                nextCreatureImage.gameObject.SetActive(false);
            }
        }
    }
}
