using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private GameObject DamagedEffect;

    public void PlayEffect(Vector3 position)
    {
        var effect = ObjectPoolManager.Get<Effect>(DamagedEffect);
        effect.transform.position = position;
        effect.gameObject.SetActive(true);
        _ = StartCoroutine(effect.EffectCoroutine());
    }
}
