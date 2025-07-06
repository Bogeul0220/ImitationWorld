using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }

    public Dictionary<string, Creature> creaturePrefabs; // 크리쳐 프리팹 딕셔너리
    public List<Creature> CreaturePrefabList;   // 크리쳐 프리팹 저장 리스트1

    [Header("길들인 크리쳐")]
    public List<int> TamedCreatureKey; // 생성된 길들여진 크리쳐 키 리스트
    public Dictionary<int, Creature> TamedCreatures; // 생성된 길들여진 크리쳐 딕셔너리
    public Dictionary<int, Coroutine> RetireAllyReviveTimer; // 퇴장한 아군 크리쳐 부활 타이머 딕셔너리
    public Dictionary<int, float> RetireAllyReviveProgress; // 퇴장한 아군 크리쳐 부활 진행률 딕셔너리
    public Creature CurrentTakeOutCreature; // 현재 소환한 아군 크리쳐
    public Belligerent CurrentAllyBelligerent; // 현재 아군 크리쳐 호전성
    public int MaxTamedCreatures = 5; // 최대 길들여진 크리쳐 수

    [Header("야생 크리쳐")]
    public List<Creature> SpawnedWildCreatures; // 생성된 야생 크리쳐 리스트
    private GameObject WildCreaturesParent;
    public int MaxWildCreatures = 5; // 최대 야생 크리쳐 수
    public float SpawnInterval = 5f; // 크리쳐 생성 간격

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        // 초기화
        if (TamedCreatureKey == null)
            TamedCreatureKey = new List<int>();

        if (TamedCreatures == null)
            TamedCreatures = new Dictionary<int, Creature>();

        if (RetireAllyReviveTimer == null)
            RetireAllyReviveTimer = new Dictionary<int, Coroutine>();

        if (RetireAllyReviveProgress == null)
            RetireAllyReviveProgress = new Dictionary<int, float>();

        CurrentAllyBelligerent = Belligerent.NonAggressive;
    }

    // Start is called before the first frame update
    void Start()
    {
        _ = StartCoroutine(SpawnCreatureCoroutine());
        InputManager.Instance.CallInAllyPressed += CallInAllyCreature;
        InputManager.Instance.BelligerentPressedInt += ChangeCreatureBelligerent;
    }

    public IEnumerator SpawnCreatureCoroutine()
    {
        while (true)
        {
            if (WildCreaturesParent == null)
            {
                GameObject createParents = new GameObject();
                createParents.name = "WildCreatureParents";
                createParents.transform.parent = this.transform;
                WildCreaturesParent = createParents;
            }

            if (SpawnedWildCreatures.Count < MaxTamedCreatures)
            {
                var setCreature = CreaturePrefabList[UnityEngine.Random.Range(0, CreaturePrefabList.Count)];
                var spawnedCreature = ObjectPoolManager.Get<Creature>(setCreature.gameObject);

                int terrainNum = UnityEngine.Random.Range(0, EnvironmentManager.Instance.Terrains.Length);
                Terrain selectTerrain = EnvironmentManager.Instance.Terrains[terrainNum];

                Vector3 spawnPos = EnvironmentManager.Instance.GetRandomPositionTerrains(selectTerrain);
                EnvironmentManager.Instance.SetObjectOnTerrain(selectTerrain, spawnedCreature.gameObject, spawnPos);

                SpawnedWildCreatures.Add(spawnedCreature);
                spawnedCreature.transform.SetParent(WildCreaturesParent.transform);
                spawnedCreature.InitCreature(false);
            }

            yield return new WaitForSeconds(SpawnInterval);
        }
    }

    public void SpawnAllyCreature(Vector3 spawnPos)
    {
        if (TamedCreatureKey.Count <= 0)
            return;

        if (InputManager.Instance.SelectedAllyCreature == -1)
            return;

        if (InputManager.Instance.SelectedAllyCreature >= TamedCreatureKey.Count)
            return;

        if (CurrentTakeOutCreature != null)
            return;

        var CreatureKey = TamedCreatureKey[InputManager.Instance.SelectedAllyCreature];

        if (RetireAllyReviveProgress.ContainsKey(CreatureKey))
            return;

        CurrentTakeOutCreature = TamedCreatures[CreatureKey];

        // 이미 같은 이름의 부모 오브젝트가 있는지 확인
        GameObject allyParent = GameObject.Find("AllyCreatureParent");

        if (allyParent == null)
        {
            // 없다면 새로 생성
            allyParent = new GameObject();
            allyParent.name = "AllyCreatureParent";
            allyParent.transform.parent = this.transform;
        }

        // 기존 자식들 중에서 타겟 크리처가 있는지 확인
        Creature targetCreature = null;
        int targetCreatureIndex = CurrentTakeOutCreature.CreatureIndex;

        for (int i = 0; i < allyParent.transform.childCount; i++)
        {
            Transform child = allyParent.transform.GetChild(i);
            if (child.TryGetComponent<Creature>(out var creature) &&
                creature.CreatureIndex == targetCreatureIndex)
            {
                targetCreature = creature;
                break;
            }
        }

        if (targetCreature != null)
        {
            // 기존 자식이 있다면 활성화하고 재사용
            targetCreature.gameObject.SetActive(true);
            CurrentTakeOutCreature = targetCreature;
            StartCoroutine(targetCreature.CreatrueSizeUp(spawnPos));
        }
        else
        {
            //없다면 포획에 성공하여 WildCreatureParents에 비활성화된 크리쳐가 있는지 확인
            for (int i = 0; i < WildCreaturesParent.transform.childCount; i++)
            {
                Transform child = WildCreaturesParent.transform.GetChild(i);
                if (child.TryGetComponent<Creature>(out var creature) &&
                    creature.CreatureIndex == CreatureKey)
                {
                    creature.transform.SetParent(allyParent.transform);
                    creature.gameObject.SetActive(true);
                    CurrentTakeOutCreature = creature;
                    StartCoroutine(creature.CreatrueSizeUp(spawnPos));
                    targetCreature = creature;
                    break;
                }
            }
        }

        // 크리처가 완전히 활성화된 후 초기화
        StartCoroutine(InitializeCreatureSafely(targetCreature, spawnPos));
    }

    public void CallInAllyCreature()
    {
        if (CurrentTakeOutCreature == null)
            return;

        // 코루틴을 먼저 시작하고, 완료 후 비활성화
        StartCoroutine(CallInAllyCreatureCoroutine());
    }

    private IEnumerator CallInAllyCreatureCoroutine()
    {
        var creatureToCallIn = CurrentTakeOutCreature;

        // 부모를 allyParent로 설정하여 자식으로 유지
        GameObject allyParent = GameObject.Find("AllyCreatureParent");
        if (allyParent != null)
        {
            creatureToCallIn.transform.SetParent(allyParent.transform);
        }

        // CreatureSizeDown 코루틴 완료 대기
        yield return StartCoroutine(creatureToCallIn.CreatureSizeDown());

        creatureToCallIn.BattleBegin = false;
        creatureToCallIn.Target = null;
        // 코루틴 완료 후 게임 오브젝트 비활성화
        creatureToCallIn.gameObject.SetActive(false);

        CurrentTakeOutCreature = null;
    }

    private IEnumerator InitializeCreatureSafely(Creature allyCreature, Vector3 spawnPos)
    {
        // 한 프레임 대기하여 오브젝트가 완전히 활성화되도록 함
        yield return null;

        allyCreature.transform.localScale = Vector3.one;
        // 크리처 초기화
        allyCreature.InitCreature(true);

        // 크기 변화 애니메이션 시작 (중복 호출 방지)
        StartCoroutine(allyCreature.CreatrueSizeUp(spawnPos));
    }

    private IEnumerator RetireAllyReviveCoroutine(int creatureIndex)
    {
        float reviveTime = 10f;

        while (reviveTime > 0f)
        {
            reviveTime -= Time.deltaTime;
            RetireAllyReviveProgress[creatureIndex] = 1 - (reviveTime / 10f);
            yield return null;
        }

        RetireAllyReviveProgress.Remove(creatureIndex);
        RetireAllyReviveTimer.Remove(creatureIndex);
    }

    public void AllyCreatureDead(int creatureIndex)
    {
        CurrentTakeOutCreature = null;

        // 기존 타이머가 있다면 중지하고 제거
        if (RetireAllyReviveTimer.ContainsKey(creatureIndex))
        {
            StopCoroutine(RetireAllyReviveTimer[creatureIndex]);
            RetireAllyReviveTimer.Remove(creatureIndex);
        }

        if (RetireAllyReviveProgress.ContainsKey(creatureIndex))
        {
            RetireAllyReviveProgress.Remove(creatureIndex);
        }

        // 새로운 타이머 시작 (Add 대신 인덱서 사용)
        RetireAllyReviveTimer[creatureIndex] = StartCoroutine(RetireAllyReviveCoroutine(creatureIndex));
        RetireAllyReviveProgress[creatureIndex] = 0f;

        foreach (var item in TamedCreatures.Values)
        {
            item.CreatureStat.isDead = false;
            item.CreatureStat.RestoreHealth(item.CreatureStat.maxHealth);
        }
    }

    public void ChangeCreatureBelligerent(int belligerentIndex)
    {
        CurrentAllyBelligerent = (Belligerent)belligerentIndex;
        if(CurrentTakeOutCreature != null)
            CurrentTakeOutCreature.CurrentBelligerent = (Belligerent)belligerentIndex;
    }
}
