using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance; // 싱글톤 인스턴스

    [Header("플레이어 설정", order = 1)]
    public GameObject Player; // 플레이어 오브젝트
    public List<MeleeWeapon> MeleeWeaponPrefabs;
    public P_CombatController PlayerCombatController;
    public P_GunController PlayerGunController;
    public bool WeaponEquiped;

    public UnitStats PlayerStat;

    public InteractionObjectBase NearInteractionObject; // 가장 가까운 상호작용 오브젝트
    public List<InteractionObjectBase> InteractionObjectLists = new List<InteractionObjectBase>(); // 상호작용 가능한 오브젝트 목록
    [SerializeField]
    private float interactionRange; // 상호작용 가능한 거리

    void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        PlayerCombatController = Player.GetComponent<P_CombatController>();
        PlayerGunController = Player.GetComponent<P_GunController>();

        InventoryManager.Instance.OnInventoryChanged += ChangeCurrentWeaponPrefab;
    }

    void Update()
    {
        InteractWithClosestObject();
    }

    void LateUpdate()
    {
        PlayerHealthUpdate();
    }

    private void PlayerHealthUpdate()
    {
        if (Player != null)
        {
            PlayerStat = Player.GetComponent<UnitStats>();
        }
    }

    private void InteractWithClosestObject()
    {
        // 플레이어와 가장 가까운 상호작용 가능한 오브젝트를 찾는 로직
        if (InteractionObjectLists.Count == 0)
        {
            NearInteractionObject = null;
            return;
        }

        float closestDistance = float.MaxValue;
        InteractionObjectBase closestObject = null;

        foreach (var interactionObject in InteractionObjectLists)
        {
            float distance = Vector3.Distance(Player.transform.position, interactionObject.transform.position);
            if (distance < closestDistance && distance <= interactionRange)
            {
                closestDistance = distance;
                closestObject = interactionObject;
            }
        }

        NearInteractionObject = closestObject;
    }

    public void AddInteractionObject(InteractionObjectBase interactionObject)
    {
        if (!InteractionObjectLists.Contains(interactionObject))
        {
            InteractionObjectLists.Add(interactionObject);
        }
    }

    public void RemoveInteractionObject(InteractionObjectBase interactionObject)
    {
        if (InteractionObjectLists.Contains(interactionObject))
        {
            InteractionObjectLists.Remove(interactionObject);
        }
    }

    private void ChangeCurrentWeaponPrefab()
    {
        if (InventoryManager.Instance.CurrentWeapon == null || InventoryManager.Instance.CurrentWeapon.ItemData is WeaponItemData == false)
        {
            DisableAllWeapons();
            return;
        }

        var meleeType = (InventoryManager.Instance.CurrentWeapon.ItemData as WeaponItemData).meleeWeaponType;

        foreach (var prefab in MeleeWeaponPrefabs)
        {
            PlayerCombatController.CurrentWeapon = null;
            prefab.gameObject.SetActive(false);

            if (WeaponEquiped == false)
                break;

            if (prefab.meleeWeaponType == meleeType)
            {
                PlayerCombatController.CurrentWeapon = prefab;
                prefab.gameObject.SetActive(true);
            }
        }
    }

    public void DisableAllWeapons()
    {
        foreach (var prefab in MeleeWeaponPrefabs)
        {
            prefab.gameObject.SetActive(false);
        }
        PlayerCombatController.CurrentWeapon = null;
        WeaponEquiped = false;
    }

    public void ToggleWeapon(int slotIndex)
    {
        if (InventoryManager.Instance.WeaponEquipDict.ContainsKey(slotIndex))
        {
            var weaponInSlot = InventoryManager.Instance.WeaponEquipDict[slotIndex];
            
            // 현재 장착된 무기와 선택한 슬롯의 무기가 같고, 무기가 활성화된 상태라면
            if (WeaponEquiped && InventoryManager.Instance.CurrentWeapon == weaponInSlot)
            {
                // 무기 비활성화
                WeaponEquiped = false;
                InventoryManager.Instance.CurrentWeapon = null;
                ChangeCurrentWeaponPrefab();
            }
            else
            {
                // 무기 활성화
                WeaponEquiped = true;
                InventoryManager.Instance.SelectedCurrentWeapon(slotIndex);
                ChangeCurrentWeaponPrefab();
            }
        }
        else
        {
            // 장착되지 않은 슬롯이면 모든 무기 비활성화
            DisableAllWeapons();
        }
    }
}
