using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catchable : MonoBehaviour
{
    public Creature CatchCreture;

    public void InitCatchable(Creature creatrue)
    {
        this.CatchCreture = creatrue;
    }
}
