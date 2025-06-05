using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class InventoryItemInfoPanel : MonoBehaviour
{
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text itemNameText, itemDescriptionText, itemInfoText, itemPriceText;

    void Awake()
    {
        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
    }

    public void ShowItemInfo(ItemBase item)
    {
        var itemData = item.ItemData;
        itemIconImage.sprite = itemData.ItemIcon;
        itemIconImage.color = new Color(1f, 1f, 1f, 1f);
        itemNameText.text = itemData.ItemName;
        itemDescriptionText.text = itemData.Description;
        if (item is EquipmentItem equipmentItem)
        {
            itemInfoText.text = $"내구도 : {equipmentItem.EquipmentData.MaxDurability}";
        }
        else if (item is CountableItem countableItem)
        {
            itemInfoText.text = $"개수 : {countableItem.Amount}";
        }
        else
        {
            itemInfoText.text = "-";
        }
    }

    public void SetPositionNearSlot(RectTransform slotRect)
    {
        RectTransform infoRect = GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);

        // 슬롯 중심
        Vector3 slotCenter = (corners[0] + corners[2]) / 2f;

        // 기본 방향 : 슬롯 오른쪽, 중앙 정렬
        Vector3 preferredWorldPos = (corners[2] + corners[3]) / 2f;

        // 스크린 좌표
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, preferredWorldPos);

        // 패널 크기
        float panelWidth = infoRect.rect.width;
        float panelHeight = infoRect.rect.height;

        // 화면 크기
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 위치 조정용 오프셋
        float offsetX = 200f;
        float offsetY = 0f;

        // 수평 방향 결정 (우측 기본, 우측으로 못 갈 경우 좌측)
        bool showLeft = screenPos.x + panelHeight > screenHeight;
        Vector3 targetworldPos = showLeft
            ? (corners[0] + corners[1]) / 2f    // 왼쪽 중간
            : (corners[2] + corners[3]) / 2f;    // 오른쪽 중간

        // 수직 방향 결정 (위 기본, 위로 못 갈 경우 아래)
        bool showBelow = screenPos.y - panelHeight < 0;
        if (showBelow)
            offsetY = panelHeight / 2f;
        else if (screenPos.y + panelHeight / 2f > screenHeight)
            offsetY = -panelHeight / 2f;

        // 화면 위치 조정
        Vector2 adjustedScreenPos = RectTransformUtility.WorldToScreenPoint(null, targetworldPos);
        adjustedScreenPos += new Vector2(showLeft ? -offsetX : offsetX, offsetY);

        // 로컬 UI 좌표로 변환
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            adjustedScreenPos,
            null,
            out localPos);

        infoRect.anchoredPosition = localPos;
    }
}
