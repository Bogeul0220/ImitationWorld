using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class BreakableObjecs : MonoBehaviour
{
    [SerializeField] private UnitStats _status;

    void Start()
    {
        OnInit();
    }

    public virtual void OnInit()
    {
        if (_status == null)
            _status = GetComponent<UnitStats>();

        _status.Init();

        _status.OnDied += OnBreak;
    }

    protected virtual void OnBreak()
    {
        ObejctPoolManager.Return(this.gameObject);
    }
}
