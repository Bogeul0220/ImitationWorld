using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDragon : Creature
{
    protected override void BattleState()
    {
        throw new System.NotImplementedException();
    }

    protected override void DiedState()
    {
        throw new System.NotImplementedException();
    }

    protected override void EscapeState()
    {
        throw new System.NotImplementedException();
    }

    protected override void IdleState()
    {
        base.IdleState();
        // RedDragon 전용 Idle 상태 로직
    }

    protected override void PatrolState()
    {
        throw new System.NotImplementedException();
    }

    protected override void StandOffState()
    {
        throw new System.NotImplementedException();
    }

    protected override void TakeHitState()
    {
        throw new System.NotImplementedException();
    }
}
