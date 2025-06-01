using UnityEngine;

public abstract class InteractionObjectBase : MonoBehaviour
{
    public string InteractionObjectName;

    public GameObject interactionObject;

    void Start()
    {
        // 플레이어 매니저에 상호작용 오브젝트를 추가
        PlayerManager.instance.AddInteractionObject(this);
        this.InitInteractObject();
    }

    public abstract void InitInteractObject();
}
