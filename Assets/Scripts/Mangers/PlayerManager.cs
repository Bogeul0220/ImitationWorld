using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance; // 싱글톤 인스턴스

    [Header("플레이어 설정")]
    public GameObject Player; // 플레이어 오브젝트
    
    public InteractionObjectBase NearInteractionObject; // 가장 가까운 상호작용 오브젝트
    public List<InteractionObjectBase> InteractionObjectLists = new List<InteractionObjectBase>(); // 상호작용 가능한 오브젝트 목록
    [SerializeField]
    private float interactionRange; // 상호작용 가능한 거리

    void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (instance == null)
            instance = this;
    }

    void Update()
    {
        InteractWithClosestObject();
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
}
