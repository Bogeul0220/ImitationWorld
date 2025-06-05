using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : UIBase
{
    [Header("Slot Settings")]
    [SerializeField]
    private Transform slotParent; // 슬롯을 담을 부모 오브젝트
    [SerializeField]
    private CraftingSlot slotPrefab; // 슬롯 프리팹
    [SerializeField]
    private List<CraftingSlot> slotList = new(); // 슬롯 관리 리스트

    [Header("Crafting Display")]
    [SerializeField]
    private List<CraftingRecipeSO> craftingRecipes; // 제작 레시피 목록
    [SerializeField]
    private Image resultItemIcon; // 제작 결과 아이템 아이콘
    [SerializeField]
    private GameObject[] materialItems; // 재료 아이템 배열

    private ItemData targetItem; // 현재 선택된 아이템

    private void Awake()
    {
        // 슬롯 초기화
        InitCraftingSlots();
    }

    void OnEnable()
    {
        if (resultItemIcon.gameObject.activeSelf)
            resultItemIcon.gameObject.SetActive(false); // 결과 아이템 아이콘 비활성화

        foreach (var materialItem in materialItems)
            materialItem.SetActive(false); // 재료 아이템 비활성화

        targetItem = null; // 선택된 아이템 초기화
    }

    private void InitCraftingSlots()
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

        // 오브젝트 풀링으로 슬롯 생성
        ObejctPoolManager.CreatePool(slotPrefab.gameObject, 1);

        for (int i = 0; i < craftingRecipes.Count; i++)
        {
            // 오브젝트 풀에서 슬롯을 가져와서 리스트에 추가
            CraftingSlot slot = ObejctPoolManager.Get<CraftingSlot>(slotPrefab.gameObject);
            slot.transform.SetParent(slotParent, false);
            slot.gameObject.name = $"CraftingSlot_{i}"; // 슬롯 이름 설정
            slot.InitSlot(this); // 슬롯 초기화
            slot.CurrentItemData = craftingRecipes[i].ResultItem; // 슬롯에 아이템 데이터 설정
            Debug.Log($"Initialized Crafting Slot: {slot.gameObject.name} with Item: {slot.CurrentItemData.ItemName}");
            Debug.Log($"Result Item : {craftingRecipes[i].ResultItem.ItemName}");
            slotList.Add(slot);
        }
    }

    public void OnDisplayRecipe(ItemData sellectItem)
    {
        targetItem = sellectItem; // 선택한 아이템 저장

        // 선택한 아이템에 해당하는 제작 레시피를 찾음
        foreach (var recipe in craftingRecipes)
        {
            if (recipe.ResultItem == targetItem)
            {
                // 재료 아이템 표시
                for (int i = 0; i < materialItems.Length; i++)
                {
                    if (i < recipe.RequiredMaterials.Count)
                    {
                        materialItems[i].SetActive(true);
                        materialItems[i].transform.Find("Icon").GetComponent<Image>().sprite = recipe.RequiredMaterials[i].ItemData.ItemIcon;
                        materialItems[i].GetComponentInChildren<TMP_Text>().text = $" x {recipe.RequiredMaterials[i].NeedCraftCount}";
                    }
                    else
                    {
                        materialItems[i].SetActive(false);
                    }
                }

                if (resultItemIcon.gameObject.activeSelf == false)
                    resultItemIcon.gameObject.SetActive(true); // 결과 아이템 아이콘 활성화

                // 제작 결과 아이템 아이콘 표시
                resultItemIcon.sprite = recipe.ResultItem.ItemIcon;
                return;
            }
        }

        // 해당 아이템에 대한 레시피가 없을 경우
        Debug.LogWarning("해당 아이템에 대한 제작 레시피가 없습니다: " + targetItem.ItemName);
    }

    public void OnCraftItem()
    {
        if (targetItem == null)
            return;

        // 선택한 아이템에 대한 제작 레시피를 찾음
        foreach (var recipe in craftingRecipes)
        {
            if (recipe.ResultItem == targetItem)
            {
                foreach (var material in recipe.RequiredMaterials)
                {
                    // 재료 아이템이 충분한지 확인
                    if (!InventoryManager.Instance.HasEnoughMaterials(material))
                    {
                        Debug.LogWarning($"제작에 필요한 재료가 부족합니다: {material.ItemData.ItemName}");
                        return; // 재료가 부족하면 제작 중단
                    }
                }

                // 모든 재료 아이템이 충분하다면 제작 진행
                foreach (var material in recipe.RequiredMaterials)
                {
                    // 재료 아이템 제거
                    InventoryManager.Instance.RemoveItem(material, material.NeedCraftCount);
                }

                // 제작 완료 후 결과 아이템 추가
                var craftedItem = targetItem.CreateItem();
                if(craftedItem == null)
                {
                    Debug.LogError("제작된 아이템이 null입니다. 아이템 데이터가 올바른지 확인하세요.");
                    return; // 제작된 아이템이 null인 경우 중단
                }
                InventoryManager.Instance.AddItem(craftedItem);

                return; // 제작 완료 후 종료
            }
        }
    }
}
