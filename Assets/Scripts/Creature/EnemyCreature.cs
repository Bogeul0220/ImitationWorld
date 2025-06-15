using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCreature : Creature
{
    void Start()
    {
        InitCreatrue(false, Belligerent.NonAggressive);
    }

    protected override void BattleState()
    {
        base.BattleState();
    }

    protected override void DiedState()
    {
        base.DiedState();
    }

    protected override void EscapeState()
    {
        base.EscapeState();
    }

    protected override void IdleState()
    {
        base.IdleState();
    }

    protected override void PatrolState()
    {
        base.PatrolState();
    }

    protected override void StandOffState()
    {
        base.StandOffState();
    }

    protected override void TakeHitState()
    {
        base.TakeHitState();
    }
}
