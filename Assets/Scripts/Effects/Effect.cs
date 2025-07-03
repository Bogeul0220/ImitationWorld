using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    [SerializeField] private ParticleSystem EffectParticle;

    public IEnumerator EffectCoroutine()
    {
        yield return new WaitForSeconds(EffectParticle.main.duration);
        ObjectPoolManager.Return(this.gameObject);
    }
}
