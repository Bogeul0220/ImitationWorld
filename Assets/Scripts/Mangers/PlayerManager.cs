using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance; // 싱글톤 인스턴스

    [Header("플레이어 설정", order = 1)]
    public GameObject Player; // 플레이어 오브젝트
    public List<MeleeWeapon> MeleeWeaponPrefabs;
    public P_CombatController PlayerCombatController;
    public P_GunController PlayerGunController;

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
            foreach (var prefab in MeleeWeaponPrefabs)
            {
                if (prefab.gameObject.activeInHierarchy)
                    prefab.gameObject.SetActive(false);
            }
            return;
        }

        var itemData = InventoryManager.Instance.CurrentWeapon.ItemData as WeaponItemData;
        var meleeType = itemData.meleeWeaponType;
        foreach (var prefab in MeleeWeaponPrefabs)
        {
            if (prefab.meleeWeaponType == meleeType)
            {
                Player.GetComponent<P_CombatController>().CurrentWeapon = prefab;
                prefab.gameObject.SetActive(true);
            }
            else
            {
                Player.GetComponent<P_CombatController>().CurrentWeapon = null;
                prefab.gameObject.SetActive(false);
            }
        }
    }
}
