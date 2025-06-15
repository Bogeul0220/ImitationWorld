using System;
using UnityEngine;

public abstract class InteractionObjectBase : MonoBehaviour
{
    public string InteractionObjectName;

    void Start()
    {
        // 플레이어 매니저에 상호작용 오브젝트를 추가
        PlayerManager.Instance.AddInteractionObject(this);
    }

    void OnDisable()
    {
        PlayerManager.Instance.RemoveInteractionObject(this);
    }

    public abstract void OnInteract();
}
