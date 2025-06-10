using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum CreatureState   // 상태
{
    Idle,       // 대기
    Patrol,     // 순찰
    Chase,      // 추격
    Escape,     // 도망
    StandOff,   // 대치
    Attack,     // 일반 공격
    TakeHit,    // 타격 (큰 공격에 맞았을 시만 실행)
    Died,       // 사망 상태
    UseSkill    // 스킬 사용
}

public enum Belligerent // 호전성(싸움을 적극적으로 하는가에 대한 분류)
{
    Peaceful,       // 비전투
    NonAggressive,  // 후공(먼저 공격을 받으면 전투 시작)
    Aggressive,     // 선공(시야에 들어올 시 바로 전투 시작)
}

public abstract class Creature : MonoBehaviour
{
    [Header("크리쳐 정보")]
    public string CreatureName;
    public int Level;
    public CreatureState currentState;
    public bool AllyEnemyConversion;
    public bool BattleBegin;
    public Belligerent belligerent;
    [SerializeField] UnitStats unitstat;

    // 현재 등록한 스킬 리스트
    public List<SkillSOBase> CurrentSKillList = new List<SkillSOBase>(4);
    public List<float> CurrentSKillCooldownList = new List<float>(4);
    // 스킬을 기억할 저장소
    public Dictionary<string, SkillSOBase> skillDict = new Dictionary<string, SkillSOBase>();


    public virtual void InitCreatrue(bool isAlly)
    {
        currentState = CreatureState.Idle;
        AllyEnemyConversion = isAlly;
        BattleBegin = false;
    }

    // 현재 스킬 리스트에 추가하기 (TODO)
    public void InitSkill(string skillName)
    {

    }

    protected abstract void IdleState();
    protected abstract void PatrolState();
    protected abstract void ChaseState();
    protected abstract void EscapeState();
    protected abstract void StandOffState();
    protected abstract void AttackState();
    protected abstract void TakeHitState();
    protected abstract void DiedState();
    protected abstract void UseSkillState();
}