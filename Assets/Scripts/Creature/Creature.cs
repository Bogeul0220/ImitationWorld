using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
    private Vector3 velocity;
    private float gravity = -9.81f; // 중력 값
    public float DistanceToTarget;

    [Header("크리쳐 스탯 세팅")]
    [SerializeField] UnitStats unitstat;
    public float AttackRange = 1f; // 공격 범위
    public float AttackSpeed = 1f; // 공격 속도
    public float AttackBaseDamage = 1f; // 공격력
    public float BaseDefecse = 1f; // 방어력
    public GameObject Target; // 전투 / 작업 시 타겟 오브젝트

    [Header("크리쳐 프리팹 세팅")]
    [SerializeField] private GameObject creaturePrefab;
    private Animator animator;
    private SkinnedMeshRenderer skinnedMeshRenderer;
    public List<Collider> hitColliderList = new List<Collider>();

    [Header("크리쳐 컴포넌트 세팅")]

    [Header("크리쳐 전투 세팅")]
    public List<SkillBaseSO> CurrentSkillList = new List<SkillBaseSO>(4);
    public Dictionary<string, SkillBaseSO> SkillDict = new Dictionary<string, SkillBaseSO>();
    public float AttackCooldown;
    private bool isDead = false; // 사망 여부

    void Start()
    {
        if (creaturePrefab != null)
        {
            characterController = GetComponent<CharacterController>();
            animator = creaturePrefab.GetComponent<Animator>();
            skinnedMeshRenderer = creaturePrefab.GetComponentInChildren<SkinnedMeshRenderer>();
            foreach (Collider col in creaturePrefab.GetComponentsInChildren<Collider>())
                if (col != null)
                    hitColliderList.Add(col);
        }
    }

    public virtual void InitCreatrue(bool isAlly)
    {
        if (creaturePrefab)
        {
            animator = creaturePrefab.GetComponent<Animator>();
        }
        currentState = CreatureState.Idle;
        AllyEnemyConversion = isAlly;
        BattleBegin = false;
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

    protected virtual void IdleState()
    {
        switch (AllyEnemyConversion)
        {
            case true:
                // 아군일 때의 행동
                if (Belligerent == Belligerent.Peaceful)
                {
                    
                    var player = PlayerManager.instance.Player;
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    // 평화 상태일 때의 행동
                    // 공격하지 않음
                    DistanceToTarget = distance;
                    if (player != null)
                    {
                        if (distance < 7f)
                            animator.SetFloat("MoveSpeed", 0f);
                        else
                        {
                            MoveToPoint(player.transform, distance);
                        }
                    }
                }
                else if (Belligerent == Belligerent.NonAggressive)
                {
                    // 후공 상태일 때의 행동
                    // 플레이어가 데미지를 입거나 공격하면 Battle 상태로 전환
                    animator.SetFloat("MoveSpeed", 0f);
                    var player = PlayerManager.instance.Player;
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    if (player != null)
                    {
                        if (player.GetComponent<P_CombatController>().InBattle)
                            currentState = CreatureState.Battle;
                    }
                }
                else if (Belligerent == Belligerent.Aggressive)
                {
                    // 선공 상태일 때의 행동 (CreatureManager를 만들고 현재 크리쳐와 적군을 관리하는 방식으로 변경 필요)
                    // 범위에 들어오면 Battle 상태로 전환
                    animator.SetFloat("MoveSpeed", 0f);
                    var player = PlayerManager.instance.Player;
                    float distance = Vector3.Distance(transform.position, player.transform.position);
                    if (player != null)
                    {
                        if (player.GetComponent<P_CombatController>().InBattle)
                            currentState = CreatureState.Battle;
                    }
                }
                break;
            case false:
                // 적군일 때의 행동
                if (Belligerent == Belligerent.Peaceful)
                {
                    animator.SetFloat("MoveSpeed", 0f);
                    // 비전투 상태일 때의 행동
                    // 시간에 따라 Idle 상태를 유지하거나 Patrol 상태로 전환
                }
                else if (Belligerent == Belligerent.NonAggressive)
                {
                    // 후공 상태일 때의 행동
                    // 데미지를 입으면 Battle 상태로 전환
                }
                else if (Belligerent == Belligerent.Aggressive)
                {
                    // 선공 상태일 때의 행동
                    // 데미지를 입거나 플레이어가 범위에 들어오면 Battle 상태로 전환
                }
                break;
        }
    }
    protected virtual void PatrolState()
    {
        if (!AllyEnemyConversion)
        {

        }
    }

    protected virtual void EscapeState()
    {

    }
    protected virtual void StandOffState()
    {

    }
    protected virtual void BattleState()
    {

    }
    protected virtual void TakeHitState()
    {

    }
    protected virtual void DiedState()
    {

    }

    protected void MoveToPoint(Transform targetPos, float distance)
    {
        if (characterController == null)
            return;

        Vector3 finalDir = (targetPos.position - transform.position).normalized;
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

        if (characterController.isGrounded && velocity.y < 0f)
        {
            // 중력 적용
            velocity.y = -2f;
        }
        else
        {
            // 공중에 있을 때 중력 적용
            velocity.y += gravity * Time.deltaTime;
        }

        Vector3 horizontalVelocity = distance > 15f ? finalDir.normalized * MoveSpeed * moveToTargetSpeed : finalDir.normalized * MoveSpeed * 3f * moveToTargetSpeed;
        Vector3 move = (horizontalVelocity + velocity) * Time.deltaTime;

        characterController.Move(move);
    }
}