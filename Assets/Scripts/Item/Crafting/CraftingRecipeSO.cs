using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Crafting/CraftingRecipe", order = 1)]
public class CraftingRecipeSO : ScriptableObject
{
    public string RecipeName => recipeName;     // 제작 레시피 이름
    public string Description => description;   // 제작 레시피 설명
    public List<CraftingMaterialItem> RequiredMaterials => requiredMaterials;   // 필요한 재료 목록
    public ItemData ResultItem => resultItem;   // 제작 결과 아이템

    [SerializeField]
    private string recipeName;
    [SerializeField]
    private string description;
    [SerializeField]
    private List<CraftingMaterialItem> requiredMaterials;
    [SerializeField]
    private ItemData resultItem;

    public bool CanCraft(CraftingMaterialItem[] availableMaterials)
    {
        // 재료가 충분한지 확인하는 로직
        foreach (var requiredMaterial in RequiredMaterials)
        {
            bool found = false;
            foreach (var availableMaterial in availableMaterials)
            {
                if (availableMaterial.ItemData == requiredMaterial.ItemData && availableMaterial.Amount >= requiredMaterial.Amount)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                return false; // 필요한 재료가 부족함
            }
        }
        return true; // 모든 재료가 충분함
    }
}
