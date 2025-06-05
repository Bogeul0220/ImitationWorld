using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] protected InventoryUI InventoryPanel; // 슬롯이 속한 인벤토리 UI
    [SerializeField] protected Image itemIcon;
    [SerializeField] private TMP_Text itemCountText;

    protected static Slot draggedSlot; // 드래그 중인 슬롯의 인덱스
    public int SlotIndex { get; protected set; } // 슬롯의 인덱스
    protected static GameObject draggedIcon; // 드래그 중인 아이콘 오브젝트

    // 슬롯에 저장될 아이템
    public virtual ItemBase CurrentItem
    {
        get => currentItem;
        set
        {
            currentItem = value;

            if (currentItem != null)
            {
                itemIcon.sprite = currentItem.ItemData.ItemIcon;
                itemIcon.color = new Color(1f, 1f, 1f, 1f);
                if (currentItem is CountableItem countableItem)
                {
                    itemCountText.text = countableItem.Amount.ToString();
                }
                else
                {
                    itemCountText.text = string.Empty;
                }
            }
            else
            {
                itemIcon.sprite = null;
                itemIcon.color = new Color(1f, 1f, 1f, 0f);
                itemCountText.text = string.Empty;
            }
        }
    }

    [SerializeField] protected ItemBase currentItem;

    public bool isEmpty => CurrentItem == null; // 슬롯이 비어있는지 여부

    public void InitSlot(InventoryUI inventoryUI, int slotIndex)
    {
        InventoryPanel = inventoryUI;
        SlotIndex = slotIndex;
        CurrentItem = null; // 슬롯 초기화
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentItem != null)
        {
            // 인벤토리 아이템 정보 패널에 아이템 정보 표시
            InventoryPanel.ShowInfoPanel(CurrentItem, GetComponent<RectTransform>());
        }
        else
        {
            // 아이템이 없을 경우, 아이템 정보 패널을 비움
            InventoryPanel.HideInfoPanel();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryPanel.HideInfoPanel();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isEmpty) return;

        draggedSlot = this; // 현재 드래그 중인 슬롯 설정

        draggedIcon = new GameObject("DraggedIcon");
        draggedIcon.transform.SetParent(transform.root);    // 캔버스 최상단에 붙이기
        var iconImage = draggedIcon.AddComponent<Image>();
        iconImage.sprite = itemIcon.sprite;
        iconImage.raycastTarget = false; // 아이콘이 다른 UI 요소를 가리지 않도록 설정

        // 크기와 모양 설정
        RectTransform rt = draggedIcon.GetComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0.5f); // 중앙 정렬

        itemIcon.enabled = false; // 원본 아이콘 비활성화
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            // 드래그 중인 아이콘 위치 업데이트
            draggedIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggedIcon != null)
        {
            Destroy(draggedIcon); // 드래그 아이콘 제거
        }

        if (draggedSlot != null)
        {
            draggedSlot.itemIcon.enabled = true;
        }

        draggedIcon = null; // 드래그 아이콘 초기화
        draggedSlot = null; // 드래그 중인 슬롯 초기화
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this || draggedSlot.isEmpty)
            return; // 드래그 중인 슬롯이 없으면 무시

        InventoryManager.Instance.SwapItems(draggedSlot.SlotIndex, this.SlotIndex); // 인벤토리 변경 알림
    }
}
