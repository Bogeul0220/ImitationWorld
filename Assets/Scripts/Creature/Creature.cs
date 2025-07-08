using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum CreatureState   // 상태
{
    Idle,       // 대기
    Patrol,     // 순찰
    Escape,     // 도망
    StandOff,   // 대치
    Battle,     // 전투
    TakeHit,    // 타격 (큰 공격에 맞았을 시만 실행)
    Died,       // 사망 상태
}

public enum Belligerent // 호전성(싸움을 적극적으로 하는가에 대한 분류)
{
    Peaceful,       // 비전투
    NonAggressive,  // 후공(먼저 공격을 받으면 전투 시작)
    Aggressive,     // 선공(시야에 들어올 시 바로 전투 시작)
}

public class Creature : MonoBehaviour
{
    [Header("크리쳐 기본 정보")]
    public string CreatureName;
    public int Level;
    public Sprite CreatureImage;
    public CreatureState currentState;
    public bool AllyEnemyConversion;
    public bool BattleBegin;
    public Belligerent CurrentBelligerent;
    public int CreatureIndex = -1;

    [Header("크리쳐 이동 세팅")]
    public float MoveSpeed; // 이동 속도
    public NavMeshAgent navMeshAgent; // 네비메시 에이전트
    public float DistanceToTarget;  // 타겟과의 거리
    public float DetectionRange = 10f; // 타겟 탐지 범위(적군과 아군 모두 적용)
    private Vector3? escapePoint; // 도망 시 타겟의 반대 방향으로 이동할 위치

    [Header("크리쳐 스탯 세팅")]
    public UnitStats CreatureStat;
    public CreatureInfoUI CreatureHeathUI;
    public float AttackRange; // 공격 범위
    public float AttackSpeed = 1f; // 공격 속도
    public float AttackBaseDamage = 1f; // 공격력
    public float BaseDefecse = 1f; // 방어력
    public UnitStats Target; // 전투 시 타겟 오브젝트

    [Header("크리쳐 프리팹 세팅")]
    [SerializeField] private GameObject creaturePrefab;
    public Animator animator;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    public List<Collider> HitColliderList = new List<Collider>();

    [Header("크리쳐 전투 세팅")]
    public List<SkillBaseSO> SkillList = new List<SkillBaseSO>();
    private Dictionary<SkillBaseSO, float> SkillsCooldownDict = new Dictionary<SkillBaseSO, float>();
    // public Dictionary<string, SkillBaseSO> SkillDict = new Dictionary<string, SkillBaseSO>();
    private float attackCooldownTimer = 0f; // 공격 쿨타임 타이머
    public bool IsAttacking = false; // 공격 중 여부
    public bool IsUsingSkill = false; // 스킬 캐스팅 중 여부
    public IEnumerator SkillCastCoroutine; // 스킬 캐스팅 코루틴
    private bool BeingCaptured = false;

    // 크리쳐 대기 / 순찰
    private float idlePatrolSwitchTimer = 0f;
    private float switchInterval = 3f;
    private float patrolDuration = 5f;
    private Vector3 patrolTarget;
    private float patrolTimer = 0f;

    // 크리쳐 초기화 메소드 (Belligerent는 기본적으로 NonAggressive로 설정)
    public virtual void InitCreature(bool isAlly)
    {
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        // NavMeshAgent가 활성화되어 있고 NavMesh 위에 있는지 확인
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            if (navMeshAgent.isStopped)
                navMeshAgent.isStopped = false;
        }

        if (CreatureStat == null)
            CreatureStat = GetComponent<UnitStats>();

        CreatureStat?.Init();

        if (creaturePrefab != null)
        {
            animator = creaturePrefab.GetComponent<Animator>();
            skinnedMeshRenderer = creaturePrefab.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (HitColliderList.Count > 0)
        {
            HitColliderList.Clear();
        }

        // 실제 게임 오브젝트의 콜라이더에 Damageable 추가
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            if (col != null)
            {
                if (!col.GetComponent<Damageable>())
                    col.AddComponent<Damageable>().InitDamageable(CreatureStat);
                else
                    col.GetComponent<Damageable>()?.InitDamageable(CreatureStat);

                if (!col.GetComponent<Catchable>())
                    col.AddComponent<Catchable>().InitCatchable(this);
                else
                    col.GetComponent<Catchable>()?.InitCatchable(this);

                HitColliderList.Add(col);
            }
        }

        AllyEnemyConversion = isAlly;
        if (AllyEnemyConversion == true)
        {
            ChangeTagInChildren(this.transform, "Ally");
        }
        else
        {
            ChangeTagInChildren(this.transform, "Enemy");
        }

        BattleBegin = false;
        if (CreatureStat.isDead)
            CreatureStat.isDead = false;
        currentState = CreatureState.Idle;
        CreatureStat.CurrentBattleTarget = null;
        CreatureStat.DamagedTargetDict.Clear();
        if (isAlly)
            CurrentBelligerent = CreatureManager.Instance.CurrentAllyBelligerent;

        IsAttacking = false;
        IsUsingSkill = false;
        if (SkillCastCoroutine != null)
        {
            StopCoroutine(SkillCastCoroutine);
            SkillCastCoroutine = null;
        }


        if (CreatureHeathUI == null)
            CreatureHeathUI = transform.GetComponentInChildren<CreatureInfoUI>();
        CreatureHeathUI.SetCreature(this);
        CreatureHeathUI.GetComponent<RectTransform>().position = this.transform.position + new Vector3(0f, navMeshAgent.height <= 1f ? 1.5f : navMeshAgent.height, 0f);
        CreatureHeathUI.UpdateHealthSlider();
        AddedSkillInList();

        CreatureStat.OnDamaged -= ConversionBattleBegin;
        CreatureStat.OnDamaged += ConversionBattleBegin;

        // InitCreature가 호출될 때마다 새로운 Index 생성
        // System.Random을 사용하여 더 강력한 랜덤 생성
        if (CreatureIndex == -1)
        {
            System.Random random = new System.Random();
            CreatureIndex = random.Next(100000000, 999999999); // 9자리 숫자로 안전하게 생성
        }

        Debug.Log($"{CreatureIndex} : {CreatureName} Init 완료");
    }

    public void ChangeTagInChildren(Transform parent, string newTag)
    {
        // 현재 오브젝트의 태그와 레이어 변경
        parent.gameObject.tag = newTag;
        parent.gameObject.layer = LayerMask.NameToLayer(newTag);

        // 모든 자식 오브젝트에 대해 재귀적으로 처리
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            ChangeTagInChildren(child, newTag);
        }
    }

    // 현재 스킬 리스트에 추가하기 (TODO)
    public void AddedSkillInList()
    {
        foreach (var skill in SkillList)
        {
            if (!SkillsCooldownDict.ContainsKey(skill))
                SkillsCooldownDict.Add(skill, 0f);
            else
                SkillsCooldownDict[skill] = 0f;
        }
    }

    void Update()
    {
        if (BeingCaptured) return;

        switch (currentState)
        {
            case CreatureState.Idle:
                IdleState();
                break;
            case CreatureState.Patrol:
                PatrolState();
                break;
            case CreatureState.Escape:
                EscapeState();
                break;
            case CreatureState.StandOff:
                StandOffState();
                break;
            case CreatureState.Battle:
                BattleState();
                break;
            case CreatureState.TakeHit:
                TakeHitState();
                break;
            case CreatureState.Died:
                DiedState();
                break;
        }

        UpdateCooldownDict();
    }

    protected virtual void IdleState()
    {
        if (CreatureStat.isDead)
        {
            StartCoroutine(CreatureDead(5f));
            return;
        }

        if (BattleBegin && CreatureStat.CurrentBattleTarget != null)
        {
            Target = CreatureStat.CurrentBattleTarget;
            escapePoint = null;
            currentState = CreatureState.StandOff;
            return;
        }

        switch (AllyEnemyConversion)
        {
            case true:
                // 아군일 때의 행동
                if (CurrentBelligerent == Belligerent.Peaceful)
                {
                    // 평화 상태일 때의 행동
                    // 공격하지 않음
                    FollowPlayer(); // 플레이어를 따라감
                }
                else if (CurrentBelligerent == Belligerent.NonAggressive)
                {
                    // 후공 상태일 때의 행동
                    // 플레이어가 데미지를 입거나 공격하면 StandOff 상태로 전환
                    var player = PlayerManager.Instance.Player;
                    if (player != null && player.GetComponent<P_CombatController>().InBattle)
                    {
                        Target = player.GetComponent<P_CombatController>().PlayerStat.CurrentBattleTarget;
                        currentState = CreatureState.StandOff;
                    }

                    FollowPlayer(); // 플레이어를 따라감
                }
                else if (CurrentBelligerent == Belligerent.Aggressive)
                {
                    // 선공 상태일 때의 행동 (CreatureManager를 만들고 현재 크리쳐와 적군을 관리하는 방식으로 변경 필요)
                    // 범위에 들어오면 StandOff 상태로 전환
                    var player = PlayerManager.Instance.Player;
                    if (player != null)
                    {
                        if (player.GetComponent<P_CombatController>().InBattle)
                        {
                            Target = player.GetComponent<P_CombatController>().PlayerStat.CurrentBattleTarget;
                            currentState = CreatureState.StandOff; // 플레이어가 전투 중이면 StandOff 상태로 전환
                        }
                        else if (IsDetection()) // 플레이어의 범위에 들어오면 StandOff 상태로 전환
                        {
                            Target = FindNearTarget();
                            currentState = CreatureState.StandOff;
                        }
                        else
                            FollowPlayer(); // 플레이어를 따라감
                    }
                }
                break;
            case false:
                // 적군일 때의 행동
                if (CurrentBelligerent == Belligerent.Peaceful)
                {
                    // 적군은 평화상태를 가지지 않음
                    CurrentBelligerent = Belligerent.NonAggressive;
                }
                else if (CurrentBelligerent == Belligerent.NonAggressive)
                {
                    // 후공 상태일 때의 행동

                    if (IsDetection())  // 플레이어가 범위에 들어오면 Escape 상태로 전환
                        currentState = CreatureState.Escape;
                }
                else if (CurrentBelligerent == Belligerent.Aggressive)
                {
                    // 선공 상태일 때의 행동
                    // 데미지를 입거나 플레이어가 범위에 들어오면 StandOff 상태로 전환
                    if (IsDetection() || BattleBegin)
                    {
                        Target = FindNearTarget();
                        currentState = CreatureState.StandOff;
                    }
                }

                idlePatrolSwitchTimer += Time.deltaTime;

                animator.SetFloat("MoveSpeed", navMeshAgent.velocity.magnitude);

                if (idlePatrolSwitchTimer >= switchInterval)
                {
                    // 일정 시간마다 Patrol 상태로 전환
                    idlePatrolSwitchTimer = 0f; // 타이머 초기화
                    currentState = CreatureState.Patrol;
                }
                break;
        }
    }
    protected virtual void PatrolState()
    {
        if (CreatureStat.isDead)
        {
            StartCoroutine(CreatureDead(5f));
            return;
        }

        if (!AllyEnemyConversion)
        {
            // 적군일 때의 순찰 행동
            // Patrol 상태에서 타겟을 찾으면 StandOff 상태로 전환
            float distance = Vector3.Distance(transform.position, patrolTarget);

            if (CurrentBelligerent == Belligerent.Aggressive && IsDetection())
            {
                Target = FindNearTarget();
                currentState = CreatureState.StandOff;
            }

            if (patrolTimer >= patrolDuration || distance < 0.5f)
            {
                patrolTimer = 0f; // 타이머 초기화
                currentState = CreatureState.Idle; // Idle 상태로 전환
            }
            else if (IsDetection())
            {
                if (CurrentBelligerent == Belligerent.Aggressive)
                {
                    patrolTimer = 0f; // 타이머 초기화
                    currentState = CreatureState.StandOff;  // 선공 상태일 때는 StandOff 상태로 전환
                }
                else if (CurrentBelligerent == Belligerent.NonAggressive)
                {
                    patrolTimer = 0f; // 타이머 초기화
                    currentState = CreatureState.Escape; // 후공 상태일 때는 도망
                }
            }

            patrolTimer += Time.deltaTime;
            if (patrolTimer == Time.deltaTime)
            {
                Vector2 randomCircle = Random.insideUnitCircle * 20f;
                float targetX = transform.position.x + randomCircle.x;
                float targetZ = transform.position.z + randomCircle.y;

                float heigth = FindTerrainContainingPoint(new Vector3(targetX, 0f, targetZ))?.SampleHeight(new Vector3(targetX, 0f, targetZ)) ?? 0f;

                patrolTarget = new Vector3(targetX, heigth, targetZ); // 순찰 타겟 위치 설정
            }
            if (IsNavMeshAgentValid())
            {
                animator.SetFloat("MoveSpeed", navMeshAgent.velocity.magnitude, 0.1f, Time.deltaTime);
                SafeResumeNavMeshAgent();
                SafeSetDestination(patrolTarget); // 순찰 타겟 위치로 이동
            }
        }
    }

    protected virtual void EscapeState()
    {
        if (CreatureStat.isDead)
        {
            StartCoroutine(CreatureDead(5f));
            return;
        }

        // 도망 상태
        // 타겟이 없거나 타겟이 죽었을 때 Idle 상태로 전환
        if (Target == null || (Target.GetComponent<UnitStats>()?.isDead ?? true))
        {
            CreatureStat.DamagedTargetDict.Remove(Target);
            CreatureStat.CurrentBattleTarget = null;
            BattleBegin = false;
            Target = null; // 타겟을 null로 설정
            escapePoint = null; // 도망 위치 초기화
            currentState = CreatureState.Idle;
            return;
        }

        if (BattleBegin && CreatureStat.CurrentBattleTarget != null)
        {
            Target = CreatureStat.CurrentBattleTarget;
            escapePoint = null;
            currentState = CreatureState.StandOff;
            return;
        }

        SafeResumeNavMeshAgent();

        // 타겟과의 거리 계산
        float distance = Vector3.Distance(transform.position, Target.transform.position);
        DistanceToTarget = distance;

        if (distance >= DetectionRange * 3f)
        {
            // 타겟과의 거리가 탐지 범위의 두 배 이상이면 Idle 상태로 전환
            Target = null; // 타겟을 null로 설정
            escapePoint = null; // 도망 위치 초기화
            currentState = CreatureState.Idle;
        }
        else
        {
            // 타겟과의 거리가 탐지 범위 이내이면 도망
            Vector3 escapeDirection = (transform.position - Target.transform.position).normalized; // 타겟의 반대 방향

            // 도망 위치를 계산
            escapePoint = transform.position + escapeDirection * DetectionRange * 3f; // 탐지 범위의 세 배 거리로 도망 위치 설정

            SafeResumeNavMeshAgent(); // 네비메시 에이전트 활성화
            animator.SetFloat("MoveSpeed", 1f, 0.1f, Time.deltaTime);
            SafeSetDestination(escapePoint.Value); // 도망 위치로 이동
        }
    }

    protected virtual void StandOffState()
    {
        if (CreatureStat.isDead)
        {
            StartCoroutine(CreatureDead(5f));
            return;
        }

        if (AllyEnemyConversion)
        {
            if (PlayerManager.Instance.Player.GetComponent<P_CombatController>().PlayerStat.CurrentBattleTarget != null)
            {
                Target = PlayerManager.Instance.Player.GetComponent<P_CombatController>().PlayerStat.CurrentBattleTarget;
            }
            else if (CurrentBelligerent == Belligerent.Aggressive)
            {
                Target = FindNearTarget();
            }
            else
            {
                CreatureStat.CurrentBattleTarget = null;
                BattleBegin = false;
                currentState = CreatureState.Idle;
                return;
            }
            
            if (Target != null && Target.isDead)
            {
                CreatureStat.CurrentBattleTarget = null;
                BattleBegin = false;
                currentState = CreatureState.Idle;
                return;
            }
        }
        else
        {
            if (CreatureStat.CurrentBattleTarget != null)
                Target = CreatureStat.CurrentBattleTarget;
            else if (CurrentBelligerent == Belligerent.Aggressive)
                Target = FindNearTarget();
            else
            {
                CreatureStat.CurrentBattleTarget = null;
                BattleBegin = false;
                currentState = CreatureState.Idle;
                return;
            }

            if (Target != null && Target.isDead)
            {
                // 타겟이 죽을 경우 Idle 상태로 전환
                CreatureStat.DamagedTargetDict.Remove(Target);
                Target = null;
                if (CreatureStat.DamagedTargetDict.Count > 0)
                {
                    int maxDamage = 0;
                    foreach (var target in CreatureStat.DamagedTargetDict.Keys)
                    {
                        if (CreatureStat.DamagedTargetDict[target] > maxDamage)
                        {
                            maxDamage = CreatureStat.DamagedTargetDict[target];
                            CreatureStat.CurrentBattleTarget = target;
                            Target = CreatureStat.CurrentBattleTarget;
                        }
                    }
                }
                else
                {
                    CreatureStat.CurrentBattleTarget = null;
                    BattleBegin = false;
                    Target = null;
                    currentState = CreatureState.Idle;
                    return;
                }
            }
        }

        if (Target != null)
        {
            float distance = Vector3.Distance(transform.position, Target.transform.position);
            DistanceToTarget = distance;

            // 부드러운 회전을 위해 회전 속도를 낮춤
            Vector3 lookDirection = (Target.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);

            if (distance > AttackRange)
            {
                if (IsUsingSkill)
                {
                    SafeStopNavMeshAgent();
                    animator.SetFloat("MoveSpeed", 0f);
                }
                else
                {
                    if (distance > 25f)
                    {
                        // 타겟이 멀어지면 Idle 상태로 전환
                        currentState = CreatureState.Idle;
                        Target = null;
                        return;
                    }

                    SafeResumeNavMeshAgent(); // 네비메시 에이전트 활성화

                    Vector3 directionToTarget = (Target.transform.position - transform.position).normalized; // 타겟 방향 계산
                    Vector3 destination = Target.transform.position - directionToTarget * (AttackRange - 0.2f); // 타겟에게 공격이 닿는 거리까지 이동할 위치 계산

                    if (IsNavMeshAgentValid())
                    {
                        animator.SetFloat("MoveSpeed", navMeshAgent.velocity.magnitude); // 이동 애니메이션 재생
                    }
                    SafeSetSpeed(MoveSpeed);
                    SafeSetDestination(destination); // 타겟에게 공격이 닿는 거리까지 이동
                }
            }
            else
            {
                if (attackCooldownTimer > 0f)
                {
                    // 부드러운 회전을 위해 회전 속도를 낮춤
                    Vector3 cooldownLookDirection = (Target.transform.position - transform.position).normalized;
                    Quaternion cooldownLookRotation = Quaternion.LookRotation(cooldownLookDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, cooldownLookRotation, 5f * Time.deltaTime);

                    SafeResumeNavMeshAgent(); // 네비메시 에이전트 활성화

                    animator.SetFloat("MoveSpeed", 0);
                    attackCooldownTimer -= Time.deltaTime; // 공격 쿨타임 감소
                }
                else
                {
                    currentState = CreatureState.Battle; // 공격 준비가 되면 Battle 상태로 전환
                }
            }
        }
        else
        {
            // 타겟이 없으면 Idle 상태로 전환
            currentState = CreatureState.Idle;
        }
    }

    protected virtual void BattleState()
    {
        if (CreatureStat.isDead)
        {
            StartCoroutine(CreatureDead(5f));
            return;
        }

        if (AllyEnemyConversion)
        {
            if (PlayerManager.Instance.Player.GetComponent<P_CombatController>().PlayerStat.CurrentBattleTarget != null)
            {
                Target = PlayerManager.Instance.Player.GetComponent<P_CombatController>().PlayerStat.CurrentBattleTarget;
            }
            else if (CurrentBelligerent == Belligerent.Aggressive)
            {
                Target = FindNearTarget();
            }
            else
            {
                CreatureStat.CurrentBattleTarget = null;
                BattleBegin = false;
                currentState = CreatureState.Idle;
                return;
            }

            if (Target != null && Target.isDead)
            {
                CreatureStat.CurrentBattleTarget = null;
                BattleBegin = false;
                currentState = CreatureState.Idle;
                return;
            }
        }
        else
        {
            if (CreatureStat.CurrentBattleTarget != null)
            {
                Target = CreatureStat.CurrentBattleTarget;
            }
            else if (CurrentBelligerent == Belligerent.Aggressive)
            {
                Target = FindNearTarget();
            }
            else
            {
                CreatureStat.CurrentBattleTarget = null;
                BattleBegin = false;
                currentState = CreatureState.Idle;
                return;
            }

            if (Target != null && Target.isDead)
            {
                // 타겟이 죽을 경우 Idle 상태로 전환
                CreatureStat.DamagedTargetDict.Remove(Target);
                Target = null;
                if (CreatureStat.DamagedTargetDict.Count > 0)
                {
                    int maxDamage = 0;
                    foreach (var target in CreatureStat.DamagedTargetDict.Keys)
                    {
                        if (CreatureStat.DamagedTargetDict[target] > maxDamage)
                        {
                            maxDamage = CreatureStat.DamagedTargetDict[target];
                            CreatureStat.CurrentBattleTarget = target;
                            Target = CreatureStat.CurrentBattleTarget;
                        }
                    }
                }
                else
                {
                    CreatureStat.CurrentBattleTarget = null;
                    BattleBegin = false;
                    Target = null;
                    currentState = CreatureState.Idle;
                    return;
                }
            }
        }

        if (Target != null && Target.GetComponent<UnitStats>() != null)
        {
            float distance = Vector3.Distance(transform.position, Target.transform.position);
            DistanceToTarget = distance;

            // 부드러운 회전을 위해 회전 속도를 낮춤
            Vector3 battleLookDirection = (Target.transform.position - transform.position).normalized;
            Quaternion battleLookRotation = Quaternion.LookRotation(battleLookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, battleLookRotation, 5f * Time.deltaTime);

            if (distance > AttackRange)
            {
                if (IsUsingSkill)
                {
                    if (!navMeshAgent.isStopped)
                        navMeshAgent.isStopped = true;
                    animator.SetFloat("MoveSpeed", 0f);
                }
                else
                {
                    if (distance > 25f)
                    {
                        // 타겟이 멀어지면 Idle 상태로 전환
                        currentState = CreatureState.Idle;
                        Target = null;
                        return;
                    }

                    // 타겟과의 거리가 공격 범위보다 멀면 타겟에게 이동
                    if (navMeshAgent.isStopped)
                        navMeshAgent.isStopped = false; // 네비메시 에이전트 활성화

                    Vector3 directionToTarget = (Target.transform.position - transform.position).normalized; // 타겟 방향 계산
                    Vector3 destination = Target.transform.position - directionToTarget * (AttackRange - 0.1f); // 타겟에게 공격이 닿는 거리까지 이동할 위치 계산

                    animator.SetFloat("MoveSpeed", navMeshAgent.velocity.magnitude); // 이동 애니메이션 재생
                    navMeshAgent.SetDestination(destination); // 타겟에게 공격이 닿는 거리까지 이동
                }
            }
            else
            {
                // 스킬 사용 또는 기본 공격
                // 현재 등록되어 있는 스킬이 있다면 해당 스킬 사용

                if (!IsAttacking)
                {
                    if (SkillCastCoroutine != null)
                    {
                        StopCoroutine(SkillCastCoroutine);
                        SkillCastCoroutine = null;
                    }

                    SkillBaseSO skill = SelletedSkill();

                    if (skill != null && !IsUsingSkill)
                    {
                        if (!navMeshAgent.isStopped)
                            navMeshAgent.isStopped = true;
                        IsAttacking = true;
                        SkillCastCoroutine = skill.ActivateSkill(this, Target.GetComponent<UnitStats>());
                        StartCoroutine(SkillCastCoroutine);
                        animator.SetFloat("MoveSpeed", 0f); // 이동 애니메이션 재생
                        SkillsCooldownDict[skill] = skill.setSkillCooldown;
                    }
                }
                else
                {
                    currentState = CreatureState.StandOff; // 공격 준비가 되지 않았으면 StandOff 상태로 전환
                }
            }
        }
        else
        {
            // 타겟이 없으면 Idle 상태로 전환
            currentState = CreatureState.Idle;
        }
    }
    protected virtual void TakeHitState()
    {
        if (CreatureStat.isDead)
        {
            StartCoroutine(CreatureDead(5f));
            return;
        }

        // 큰 공격을 받았을 때의 행동
        // 타격 애니메이션 재생 및 상태 전환
        if (SkillCastCoroutine != null)
        {
            StopCoroutine(SkillCastCoroutine);
            IsUsingSkill = false; // 스킬 캐스팅 중지
        }

        animator.SetTrigger("TakeHit");
    }

    protected virtual void DiedState()
    {
        // 사망 상태
        // 사망 애니메이션 재생 및 상태 전환
        if (SkillCastCoroutine != null)
        {
            StopCoroutine(SkillCastCoroutine);
            IsUsingSkill = false; // 스킬 캐스팅 중지
        }

        navMeshAgent.isStopped = true; // 네비메시 에이전트 정지
        gameObject.layer = LayerMask.NameToLayer("Dead"); // 레이어 변경
    }

    private Terrain FindTerrainContainingPoint(Vector3 point)
    {
        if (EnvironmentManager.Instance?.Terrains == null || EnvironmentManager.Instance.Terrains.Length == 0)
            return null; // Terrain이 없으면 null 반환

        foreach (var terrain in EnvironmentManager.Instance.Terrains)
        {
            Vector3 terrainPos = terrain.transform.position;
            TerrainData terrainData = terrain.terrainData;

            if (point.x >= terrainPos.x && point.x <= terrainPos.x + terrainData.size.x &&
                point.z >= terrainPos.z && point.z <= terrainPos.z + terrainData.size.z)
                return terrain;
        }

        return null; // 해당하는 Terrain이 없으면 null 반환
    }

    private void FollowPlayer()
    {
        // NavMeshAgent 안전성 체크
        if (navMeshAgent == null || !navMeshAgent.isActiveAndEnabled || !navMeshAgent.isOnNavMesh)
        {
            return;
        }

        if (navMeshAgent.isStopped)
            navMeshAgent.isStopped = false; // 네비메시 에이전트 활성화

        if (PlayerManager.Instance?.Player == null) return;

        Vector3 playerPos = PlayerManager.Instance.Player.transform.position;
        float distance = Vector3.Distance(transform.position, playerPos);

        if (PlayerManager.Instance.Player != null)
        {
            if (distance < 7f)
            {
                SafeResetPath();
                animator.SetFloat("MoveSpeed", 0f);
                return;
            }

            if (distance < 20f)
            {
                Vector3 dir = (playerPos - transform.position).normalized;
                Vector3 followTarget = playerPos - dir * 5f;

                float t = Mathf.InverseLerp(7f, 20f, distance); // t = 0 ~ 1
                float _speed = Mathf.Lerp(1f, 5f, t);    // 거리에 따라(t) 이동속도 변경

                SafeSetSpeed(MoveSpeed * _speed); // 기본 이동 속도 설정
                SafeSetDestination(followTarget); // 플레이어에게 이동
                if (IsNavMeshAgentValid())
                {
                    animator.SetFloat("MoveSpeed", navMeshAgent.velocity.magnitude);
                }
                return;
            }

            if (distance >= 20f)
            {
                Vector3 direction = (playerPos - transform.position).normalized;
                Vector3 teleportPos = playerPos - direction * 3f;   // 플레이어 뒤로 3m

                // NavMash 위에 위치하도록 조정
                NavMeshHit hit;
                if (NavMesh.SamplePosition(teleportPos, out hit, 5f, NavMesh.AllAreas))
                {
                    SafeWarp(hit.position);
                }
                else
                {
                    SafeWarp(playerPos);
                }

                animator.SetFloat("MoveSpeed", 0f);
                return;
            }
        }
    }

    private bool IsDetection()
    {
        if (AllyEnemyConversion)
        {
            // 아군일 때의 탐지 로직
            foreach (var enemy in CreatureManager.Instance.SpawnedWildCreatures)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance <= DetectionRange)
                    {
                        Target = enemy.CreatureStat;
                        return true;
                    }
                }
            }
            return false;
        }
        else
        {
            // 적군일 때의 탐지 로직
            if (PlayerManager.Instance.Player != null)
            {
                float distance = Vector3.Distance(transform.position, PlayerManager.Instance.Player.transform.position);
                if (CreatureManager.Instance.CurrentTakeOutCreature != null)
                {
                    float distanceToTakeOutCreature = Vector3.Distance(transform.position, CreatureManager.Instance.CurrentTakeOutCreature.transform.position);
                    if (distanceToTakeOutCreature <= DetectionRange)
                    {
                        if (distanceToTakeOutCreature < distance)
                            Target = CreatureManager.Instance.CurrentTakeOutCreature.CreatureStat;
                        else
                            Target = PlayerManager.Instance.Player.GetComponent<UnitStats>();
                        return true;
                    }
                }
                else if (distance <= DetectionRange)
                {
                    Target = PlayerManager.Instance.Player.GetComponent<UnitStats>();
                    return true;
                }
            }
            return false;
        }
    }

    private UnitStats FindNearTarget()
    {
        if (AllyEnemyConversion)
        {
            // 아군일 때의 탐지 로직
            foreach (var enemy in CreatureManager.Instance.SpawnedWildCreatures)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance <= DetectionRange)
                    {
                        return enemy.CreatureStat;
                    }
                }
            }
        }
        else
        {
            // 적군일 때의 탐지 로직
            if (PlayerManager.Instance.Player != null)
            {
                float distance = Vector3.Distance(transform.position, PlayerManager.Instance.Player.transform.position);
                if (CreatureManager.Instance.CurrentTakeOutCreature != null)
                {
                    float distanceToTakeOutCreature = Vector3.Distance(transform.position, CreatureManager.Instance.CurrentTakeOutCreature.transform.position);
                    if (distanceToTakeOutCreature <= DetectionRange)
                    {
                        if (distanceToTakeOutCreature < distance)
                            Target = CreatureManager.Instance.CurrentTakeOutCreature.CreatureStat;
                        else
                            Target = PlayerManager.Instance.Player.GetComponent<UnitStats>();
                        return Target;
                    }
                }
                else if (distance <= DetectionRange)
                {
                    Target = PlayerManager.Instance.Player.GetComponent<UnitStats>();
                    return PlayerManager.Instance.Player.GetComponent<UnitStats>();
                }
            }
        }

        return null;
    }

    private SkillBaseSO SelletedSkill()
    {
        // 현재 스킬 리스트에서 사용 가능한 스킬을 랜덤으로 선택하는 메소드
        List<SkillBaseSO> usableSkills = new List<SkillBaseSO>();

        // 현재 선택된 스킬을 반환하는 메소드
        if (SkillList.Count <= 0)
            return null;

        // 현재 스킬 리스트에서 사용 가능한 스킬을 찾음
        foreach (var skill in SkillList)
            if (SkillsCooldownDict[skill] <= 0f)
                usableSkills.Add(skill);

        // 사용 가능한 스킬이 있다면 랜덤으로 선택
        if (usableSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, usableSkills.Count);
            return usableSkills[randomIndex];
        }
        else
        {
            return null; // 사용 가능한 스킬이 없으면 null 반환
        }
    }

    public void UpdateCooldownDict()
    {
        foreach (var skill in SkillsCooldownDict.ToList()) // 복사본 순회
        {
            if (skill.Value > 0f)
            {
                SkillsCooldownDict[skill.Key] = skill.Value - Time.deltaTime;
            }
        }
    }

    public IEnumerator CreatureSizeDown()
    {
        // NavMeshAgent가 활성화되어 있고 NavMesh 위에 있는지 확인
        SafeStopNavMeshAgent();

        float duration = 1f;
        float elapsed = 0f;
        BeingCaptured = true;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.zero;
    }

    public IEnumerator CreatrueSizeUp(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;

        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;

        BeingCaptured = false;

        // NavMeshAgent가 활성화되어 있고 NavMesh 위에 있는지 확인
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
        }
    }

    public void AttackIsDone()
    {
        IsAttacking = false;
        IsUsingSkill = false;

        // NavMeshAgent 안전성 체크
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh && navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = false;
        }

        SoundManager.Instance.PlaySFX($"{CreatureName}Roar");
        animator.SetTrigger("BaseAttack");
    }

    private IEnumerator CreatureDead(float delay = 0f)
    {
        currentState = CreatureState.Died;
        animator.SetTrigger("IsDead");

        if (AllyEnemyConversion)
        {
            CreatureManager.Instance.CallInAllyCreature();
            CreatureManager.Instance.AllyCreatureDead(CreatureIndex);
        }

        if (gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(delay);

            if (CreatureManager.Instance.SpawnedWildCreatures.Contains(this))
                CreatureManager.Instance.SpawnedWildCreatures.Remove(this);
            ObjectPoolManager.Return(gameObject);
        }
    }

    private void ConversionBattleBegin() => BattleBegin = true;

    // NavMeshAgent 안전성 체크 헬퍼 메서드
    private bool IsNavMeshAgentValid()
    {
        return navMeshAgent != null && navMeshAgent.isActiveAndEnabled && navMeshAgent.isOnNavMesh;
    }

    // NavMeshAgent 안전하게 정지
    private void SafeStopNavMeshAgent()
    {
        if (IsNavMeshAgentValid() && !navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = true;
        }
    }

    // NavMeshAgent 안전하게 재시작
    private void SafeResumeNavMeshAgent()
    {
        if (IsNavMeshAgentValid() && navMeshAgent.isStopped)
        {
            navMeshAgent.isStopped = false;
        }
    }

    // NavMeshAgent 안전하게 목적지 설정
    private void SafeSetDestination(Vector3 destination)
    {
        if (IsNavMeshAgentValid())
        {
            navMeshAgent.SetDestination(destination);
        }
    }

    // NavMeshAgent 안전하게 속도 설정
    private void SafeSetSpeed(float speed)
    {
        if (IsNavMeshAgentValid())
        {
            navMeshAgent.speed = speed;
        }
    }

    // NavMeshAgent 안전하게 경로 초기화
    private void SafeResetPath()
    {
        if (IsNavMeshAgentValid())
        {
            navMeshAgent.ResetPath();
        }
    }

    // NavMeshAgent 안전하게 워프
    private void SafeWarp(Vector3 position)
    {
        if (IsNavMeshAgentValid())
        {
            navMeshAgent.Warp(position);
        }
    }
}