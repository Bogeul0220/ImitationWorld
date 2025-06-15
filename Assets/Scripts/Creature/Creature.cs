using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Unity.VisualScripting;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using UnityEngine.Rendering;

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

public abstract class Creature : MonoBehaviour
{
    [Header("크리쳐 기본 정보")]
    public string CreatureName;
    public int Level;
    public CreatureState currentState;
    public bool AllyEnemyConversion;
    public bool BattleBegin;
    public Belligerent Belligerent;

    [Header("크리쳐 이동 세팅")]
    public float MoveSpeed; // 이동 속도
    public float ObstacleAvoidanceDistance; // 장애물 회피 거리
    public float AvoidStrength; // 장애물 회피 강도
    public LayerMask ObstacleLayer; // 장애물 레이어
    public CharacterController characterController; // 캐릭터 컨트롤러
    private float gravity = -9.81f; // 중력 값
    public float DistanceToTarget;  // 타겟과의 거리
    public float DetectionRange = 10f; // 타겟 탐지 범위(적군과 아군 모두 적용)
    private Vector3? escapePoint; // 도망 시 타겟의 반대 방향으로 이동할 위치

    [Header("크리쳐 중력 세팅")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.28f;
    public LayerMask GroundLayers;
    public float Gravity = -15.0f;
    public float FallTimeout = 0.15f;
    private float _fallTimeoutDelta;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    [Header("크리쳐 스탯 세팅")]
    [SerializeField] UnitStats unitstat;
    public float AttackRange = 1f; // 공격 범위
    public float AttackSpeed = 1f; // 공격 속도
    public float AttackBaseDamage = 1f; // 공격력
    public float BaseDefecse = 1f; // 방어력
    public GameObject Target; // 전투 / 작업 시 타겟 오브젝트

    [Header("크리쳐 프리팹 세팅")]
    [SerializeField] private GameObject creaturePrefab;
    public Animator animator;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    public List<Collider> hitColliderList = new List<Collider>();

    [Header("크리쳐 컴포넌트 세팅")]

    [Header("크리쳐 전투 세팅")]
    public List<SkillBaseSO> CurrentSkillList = new List<SkillBaseSO>(4);
    public Dictionary<string, SkillBaseSO> SkillDict = new Dictionary<string, SkillBaseSO>();
    public float AttackCooldown;
    private float attackCooldownTimer = 0f; // 공격 쿨타임 타이머
    public bool IsUsingSkill = false; // 스킬 캐스팅 중 여부
    public IEnumerator SkillCastCoroutine; // 스킬 캐스팅 코루틴
    private bool IsDead = false; // 사망 여부

    [Header("크리쳐 대기/순찰")]
    private float idlePatrolSwitchTimer = 0f;
    private float switchInterval = 3f;
    private float patrolDuration = 5f;
    private Vector3 patrolTarget;
    private float patrolTimer = 0f;

    void Start()
    {
        InitCreatrue(true); // 기본적으로 아군으로 초기화
    }

    // 크리쳐 초기화 메소드 (Belligerent는 기본적으로 NonAggressive로 설정)
    public virtual void InitCreatrue(bool isAlly, Belligerent belligerent = Belligerent.NonAggressive)
    {
        if (creaturePrefab != null)
        {
            animator = creaturePrefab.GetComponent<Animator>();
            skinnedMeshRenderer = creaturePrefab.GetComponentInChildren<SkinnedMeshRenderer>();
            foreach (Collider col in creaturePrefab.GetComponentsInChildren<Collider>())
                if (col != null)
                    hitColliderList.Add(col);
        }

        characterController = GetComponent<CharacterController>();
        if (unitstat == null)
            unitstat = GetComponent<UnitStats>();

        currentState = CreatureState.Idle;
        AllyEnemyConversion = isAlly;
        BattleBegin = false;
        IsDead = false;

        unitstat.OnDamaged += ConversionBattleBegin;
    }

    // 현재 스킬 리스트에 추가하기 (TODO)
    public void InitSkill(string skillName)
    {

    }

    void Update()
    {
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
    }

    void LateUpdate()
    {

    }
    void FixedUpdate()
    {
        // 바닥에 닿아 있는지 확인
        GroundedCheck();
        
        // 중력 적용
        ApplyGravity();
    }

    protected virtual void IdleState()
    {
        switch (AllyEnemyConversion)
        {
            case true:
                // 아군일 때의 행동
                if (Belligerent == Belligerent.Peaceful)
                {
                    // 평화 상태일 때의 행동
                    // 공격하지 않음
                    var player = PlayerManager.Instance.Player;
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    DistanceToTarget = distance;
                    if (player != null)
                    {
                        if (distance < 7f)
                            animator.SetFloat("MoveSpeed", 0f);
                        else
                        {
                            MoveToPoint(player.transform.position, distance);
                        }
                    }
                }
                else if (Belligerent == Belligerent.NonAggressive)
                {
                    // 후공 상태일 때의 행동
                    // 플레이어가 데미지를 입거나 공격하면 StandOff 상태로 전환
                    animator.SetFloat("MoveSpeed", 0f);
                    var player = PlayerManager.Instance.Player;
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    if (player != null)
                    {
                        if (player.GetComponent<P_CombatController>().InBattle)
                            currentState = CreatureState.StandOff;

                        if (distance < 7f)
                            animator.SetFloat("MoveSpeed", 0f);
                        else
                        {
                            MoveToPoint(player.transform.position, distance);
                        }
                    }
                }
                else if (Belligerent == Belligerent.Aggressive)
                {
                    // 선공 상태일 때의 행동 (CreatureManager를 만들고 현재 크리쳐와 적군을 관리하는 방식으로 변경 필요)
                    // 범위에 들어오면 StandOff 상태로 전환
                    animator.SetFloat("MoveSpeed", 0f);
                    var player = PlayerManager.Instance.Player;
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    if (player != null)
                    {
                        if (player.GetComponent<P_CombatController>().InBattle)
                            currentState = CreatureState.StandOff;

                        if (distance < 7f)
                            animator.SetFloat("MoveSpeed", 0f);
                        else
                        {
                            MoveToPoint(player.transform.position, distance);
                        }
                    }
                }
                break;
            case false:
                // 적군일 때의 행동
                if (Belligerent == Belligerent.Peaceful)
                {
                    // 적군은 평화상태를 가지지 않음
                    Belligerent = Belligerent.NonAggressive;
                }
                else if (Belligerent == Belligerent.NonAggressive)
                {
                    // 후공 상태일 때의 행동
                    
                    if (IsDetection())  // 플레이어가 범위에 들어오면 Escape 상태로 전환
                        currentState = CreatureState.Escape;
                    else if (BattleBegin)   // 데미지를 입으면 StandOff 상태로 전환
                        currentState = CreatureState.StandOff;
                }
                else if (Belligerent == Belligerent.Aggressive)
                {
                    // 선공 상태일 때의 행동
                    // 데미지를 입거나 플레이어가 범위에 들어오면 StandOff 상태로 전환
                    if (IsDetection() || BattleBegin)
                        currentState = CreatureState.StandOff;
                }

                idlePatrolSwitchTimer += Time.deltaTime;
                animator.SetFloat("MoveSpeed", 0f);
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
        if (!AllyEnemyConversion)
        {
            // 적군일 때의 순찰 행동
            // Patrol 상태에서 타겟을 찾으면 StandOff 상태로 전환
            float distance = Vector3.Distance(transform.position, patrolTarget);

            if (patrolTimer >= patrolDuration || distance < 0.5f)
            {
                patrolTimer = 0f; // 타이머 초기화
                currentState = CreatureState.Idle; // Idle 상태로 전환
            }
            else if (IsDetection())
            {
                if (Belligerent == Belligerent.Aggressive)
                {
                    patrolTimer = 0f; // 타이머 초기화
                    currentState = CreatureState.StandOff;  // 선공 상태일 때는 StandOff 상태로 전환
                }
                else if (Belligerent == Belligerent.NonAggressive)
                {
                    patrolTimer = 0f; // 타이머 초기화
                    currentState = CreatureState.Escape; // 후공 상태일 때는 도망
                }
            }

            patrolTimer += Time.deltaTime;
            if (patrolTimer == Time.deltaTime)
            {
                Vector2 randomCircle = Random.insideUnitCircle * 5f;
                float targetX = transform.position.x + randomCircle.x;
                float targetZ = transform.position.z + randomCircle.y;

                float heigth = FindTerrainContainingPoint(new Vector3(targetX, 0f, targetZ))?.SampleHeight(new Vector3(targetX, 0f, targetZ)) ?? 0f;

                patrolTarget = new Vector3(randomCircle.x, heigth, randomCircle.y);
            }


            animator.SetFloat("MoveSpeed", Random.Range(0.5f, 1f), 0.1f, Time.deltaTime);
            MoveToPoint(patrolTarget, distance);
        }
    }

    protected virtual void EscapeState()
    {
        // 도망 상태
        // 타겟이 없거나 타겟이 죽었을 때 Idle 상태로 전환
        if (Target == null || (Target.GetComponent<UnitStats>()?.isDead ?? true))
        {
            Target = null; // 타겟을 null로 설정
            escapePoint = null; // 도망 위치 초기화
            currentState = CreatureState.Idle;
            return;
        }

        // 타겟과의 거리 계산
        float distance = Vector3.Distance(transform.position, Target.transform.position);
        DistanceToTarget = distance;

        if (distance >= DetectionRange * 5f)
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

            animator.SetFloat("MoveSpeed", 1f, 0.1f, Time.deltaTime);
            Quaternion lookRotation = Quaternion.LookRotation(escapeDirection);
            characterController.Move(escapeDirection * MoveSpeed * 3f * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    protected virtual void StandOffState()
    {
        if (Target != null)
        {
            if (Target.GetComponent<Creature>().IsDead || Target.GetComponent<P_CombatController>().IsDied)
            {
                // 타겟이 죽었을 경우 Idle 상태로 전환
                currentState = CreatureState.Idle;
                Target = null;
                return;
            }

            float distance = Vector3.Distance(transform.position, Target.transform.position);
            DistanceToTarget = distance;
            if (distance > AttackRange)
            {
                // 타겟과의 거리가 공격 범위보다 멀면 타겟에게 이동
                MoveToPoint(Target.transform.position, distance);
            }
            else
            {
                if (attackCooldownTimer > 0f)
                {
                    // 대치 상태일때 왼쪽으로 천천히 이동
                    Vector3 leftMove = -transform.right * MoveSpeed * 0.5f * Time.deltaTime;
                    characterController.Move(leftMove);
                    animator.SetFloat("MoveSpeed", 0.5f, 0.1f, Time.deltaTime);
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
        if (Target != null && Target.GetComponent<UnitStats>() != null)
        {
            if (Target.GetComponent<Creature>().IsDead || Target.GetComponent<P_CombatController>().IsDied)
            {
                // 타겟이 죽었을 경우 Idle 상태로 전환
                currentState = CreatureState.Idle;
                Target = null;
                return;
            }

            float distance = Vector3.Distance(transform.position, Target.transform.position);
            DistanceToTarget = distance;

            if (distance > AttackRange)
            {
                // 타겟과의 거리가 공격 범위보다 멀면 타겟에게 이동
                MoveToPoint(Target.transform.position, distance);
            }
            else
            {
                // 스킬 사용 또는 기본 공격
                // 현재 등록되어 있는 스킬이 있다면 해당 스킬 사용
                if (SkillCastCoroutine != null)
                    StopCoroutine(SkillCastCoroutine);

                if (SelletedSkill() != null && !IsUsingSkill)
                {
                    SkillBaseSO skill = SelletedSkill();
                    SkillCastCoroutine = skill.ActivateSkill(this, Target.GetComponent<UnitStats>());
                    StartCoroutine(SkillCastCoroutine);
                    StartCoroutine(skill.StartCoolDown());
                }
                else
                {
                    // 기본 공격 애니메이션 재생
                    animator.SetTrigger("Attack");
                    // 공격력 계산 및 타겟에게 데미지 적용
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

        animator.SetTrigger("Die");
        characterController.enabled = false; // 캐릭터 컨트롤러 비활성화
        skinnedMeshRenderer.enabled = false; // 메쉬 렌더러 비활성화
        gameObject.layer = LayerMask.NameToLayer("Dead"); // 레이어 변경
    }

    protected void MoveToPoint(Vector3 targetPos, float distance)
    {
        if (characterController == null)
            return;

        Vector3 finalDir = (targetPos - transform.position).normalized;
        float moveToTargetSpeed = (distance > 15f) ? 1f : 0.5f; // 타겟과의 거리에 따라 이동 속도 조절

        // 장애물 탐지 (앞으로 Ray 발사)
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hit, ObstacleAvoidanceDistance, ObstacleLayer))
        {
            // 회피 방향: hit.normal과 transform.right를 이용해 벗어나는 방향 계산
            Vector3 obstacleNormal = hit.normal;
            Vector3 avoidDir = Vector3.Cross(obstacleNormal, Vector3.up).normalized;

            // 회피 강도 적용
            finalDir += avoidDir * AvoidStrength;
        }

        animator.SetFloat("MoveSpeed", moveToTargetSpeed, 0.1f, Time.deltaTime);

        // 회전
        if (finalDir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(finalDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        Vector3 horizontalVelocity = distance > 15f ? finalDir.normalized * MoveSpeed * moveToTargetSpeed : finalDir.normalized * MoveSpeed * 3f * moveToTargetSpeed;
        Vector3 move = horizontalVelocity * Time.deltaTime;

        characterController.Move(move);
    }

    private void ApplyGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }
        }
        else
        {
            _verticalVelocity += Gravity * Time.deltaTime;

            if (_verticalVelocity < -_terminalVelocity)
                _verticalVelocity = -_terminalVelocity; // 최대 낙하 속도 제한
        }

        characterController.Move(new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void GroundedCheck()
    {
        // 바닥에 닿아 있는지 확인하기 위해 CheckSphere를 사용하여 구를 생성합니다.
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);
    }

    private Terrain FindTerrainContainingPoint(Vector3 point)
    {
        if (EnvironmentManager.Instance?.terrains == null || EnvironmentManager.Instance.terrains.Length == 0)
            return null; // Terrain이 없으면 null 반환

        foreach (var terrain in EnvironmentManager.Instance.terrains)
        {
            Vector3 terrainPos = terrain.transform.position;
            TerrainData terrainData = terrain.terrainData;

            if (point.x >= terrainPos.x && point.x <= terrainPos.x + terrainData.size.x &&
                point.z >= terrainPos.z && point.z <= terrainPos.z + terrainData.size.z)
                return terrain;
        }

        return null; // 해당하는 Terrain이 없으면 null 반환
    }

    private bool IsDetection()
    {
        if (AllyEnemyConversion)
        {
            // 아군일 때의 탐지 로직
            foreach (var enemy in CreatureManager.Instance.spawnedWildCreatures)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance <= DetectionRange)
                    {
                        Target = enemy.gameObject;
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
                if (distance <= DetectionRange)
                {
                    Target = PlayerManager.Instance.Player.gameObject;
                    return true;
                }
            }
            return false;
        }
    }

    private SkillBaseSO SelletedSkill()
    {
        // 현재 스킬 리스트에서 사용 가능한 스킬을 랜덤으로 선택하는 메소드
        List<SkillBaseSO> usableSkills = new List<SkillBaseSO>();

        // 현재 선택된 스킬을 반환하는 메소드
        if (CurrentSkillList.Count <= 0)
            return null;

        // 현재 스킬 리스트에서 사용 가능한 스킬을 찾음
        foreach (var skill in CurrentSkillList)
            if (skill.Useable)
                usableSkills.Add(skill);

        // 사용 가능한 스킬이 있다면 랜덤으로 선택
        if (usableSkills.Count > 0)
        {
            int randomIndex = Random.Range(0, usableSkills.Count);
            return usableSkills[randomIndex];
        }
        else
            return null; // 사용 가능한 스킬이 없으면 null 반환
    }

    public void AttackIsDone()
    {
        // 공격이 완료되었을 때 호출되는 메소드
        // 공격 쿨타임 초기화
        attackCooldownTimer = AttackCooldown;

        // 애니메이션 상태 초기화
        animator.ResetTrigger("Attack");
        animator.SetFloat("MoveSpeed", 0f, 0.1f, Time.deltaTime);

        // 현재 상태를 StandOff로 전환
        currentState = CreatureState.StandOff;

        IsUsingSkill = false; // 스킬 캐스팅 상태 변경
    }

    private void ConversionBattleBegin() => BattleBegin = true;
}