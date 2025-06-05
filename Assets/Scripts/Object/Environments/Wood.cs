using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : BreakableObjecs
{
    protected override void OnBreak()
    {
        _ = StartCoroutine(DelayDestroy());
        base.OnBreak();
    }

    IEnumerator DelayDestroy()
    {
        float duration = 3f;

        while (duration > 0)
        {
            duration -= Time.deltaTime;
        }

        yield return null;
    }
}
