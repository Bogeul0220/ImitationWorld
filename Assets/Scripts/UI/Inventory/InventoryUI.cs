using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryUI : UIBase
{
    [SerializeField]
    private Slot slotPrefab; // 슬롯 프리팹

    [SerializeField]
    private List<Slot> slotList = new List<Slot>(); // 슬롯 관리 리스트

    [SerializeField]
    private RectTransform slotParent; // 슬롯을 배치할 부모 오브젝트

    [SerializeField]
    private InventoryItemInfoPanel itemInfoPanel; // 아이템 정보 패널

    private void Awake()
    {
        // 인벤토리 패널 초기화
        InitInventoryPanel();
    }

    void OnEnable()
    {
        // 인벤토리 UI 업데이트
        InventoryManager.Instance.OnInventoryChanged += UpdateInventoryUI;
        UpdateInventoryUI();
    }

    void OnDisable()
    {
        // 인벤토리 UI 업데이트 이벤트 제거
        InventoryManager.Instance.OnInventoryChanged -= UpdateInventoryUI;
    }

    public void InitInventoryPanel()
    {
        // 슬롯 초기화
        if (slotList.Count > 0)
        {
            foreach (var slot in slotList)
            {
                Destroy(slot.gameObject);
            }
            slotList.Clear();
        }
        else
        {
            // 오브젝트 풀링으로 슬롯 생성
            ObjectPoolManager.CreatePool(slotPrefab.gameObject, 1);

            for (int i = 0; i < InventoryManager.Instance.MaxInventorySize; i++)
            {
                // 오브젝트 풀에서 슬롯을 가져와서 리스트에 추가
                Slot slot = ObjectPoolManager.Get<Slot>(slotPrefab.gameObject);
                slot.transform.SetParent(slotParent, false);
                slot.gameObject.name = $"Slot_{i}"; // 슬롯 이름 설정
                slotList.Add(slot);
                slot.InitSlot(this, i); // 슬롯 초기화
            }
        }
    }

    private void UpdateInventoryUI()
    {
        var invenData = InventoryManager.Instance.InvenItemDict;

        // 인벤토리 아이템 업데이트
        for (int i = 0; i < slotList.Count; i++)
        {
            if (invenData.ContainsKey(i))
            {
                // 슬롯에 아이템 설정
                slotList[i].CurrentItem = invenData[i];
            }
            else
            {
                // 슬롯 비우기
                slotList[i].CurrentItem = null;
            }
        }
    }

    public void ShowInfoPanel(ItemBase item, RectTransform slotRect)
    {
        if (itemInfoPanel != null)
        {
            itemInfoPanel.gameObject.SetActive(true);
            itemInfoPanel.ShowItemInfo(item);
            itemInfoPanel.SetPositionNearSlot(slotRect);
        }
    }

    public void HideInfoPanel()
    {
        if (itemInfoPanel != null)
        {
            itemInfoPanel.gameObject.SetActive(false);
        }
    }
}
