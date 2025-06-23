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
    public List<int> SpawnedTamedKey; // 생성된 길들여진 크리쳐 키 리스트
    public Dictionary<int, Creature> SpawnedTamedCreatures; // 생성된 길들여진 크리쳐 딕셔너리
    public Creature CurrentTakeOutCreature; // 현재 소환한 아군 크리쳐
    public int MaxTamedCreatures = 5; // 최대 길들여진 크리쳐 수

    [Header("야생 크리쳐")]
    public List<Creature> SpawnedWildCreatures; // 생성된 야생 크리쳐 리스트
    private GameObject WildCreaturesParent;
    public int MaxWildCreatures = 5; // 최대 야생 크리쳐 수
    public float SpawnInterval = 5f; // 크리쳐 생성 간격
    private float NextSpawnTime = 0f; // 다음 크리쳐 생성 시간

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
            
        // 초기화
        if (SpawnedTamedKey == null)
            SpawnedTamedKey = new List<int>();
            
        if (SpawnedTamedCreatures == null)
            SpawnedTamedCreatures = new Dictionary<int, Creature>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _ = StartCoroutine(SpawnCreatureCoroutine());
        InputManager.Instance.CallInAllyPressed += CallInAllyCreature;
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
        if (SpawnedTamedKey.Count <= 0)
            return;

        if (InputManager.Instance.SelectedAllyCreature == -1)
            return;

        if (InputManager.Instance.SelectedAllyCreature >= SpawnedTamedKey.Count)
            return;

        if (CurrentTakeOutCreature != null)
            return;

        var CreatureKey = SpawnedTamedKey[InputManager.Instance.SelectedAllyCreature];
        CurrentTakeOutCreature = SpawnedTamedCreatures[CreatureKey];
        Debug.Log("SpawnAllyCreature : " + CurrentTakeOutCreature.CreatureName + " : " + CurrentTakeOutCreature.CreatureIndex);

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
            targetCreature.transform.position = spawnPos;
            StartCoroutine(targetCreature.CreatrueSizeUp(spawnPos));
        }
        else
        {
            // 없다면 새로 생성
            var spawnedCreature = ObjectPoolManager.Get<Creature>(CurrentTakeOutCreature.gameObject);
            spawnedCreature.transform.SetParent(allyParent.transform);
            spawnedCreature.CreatureIndex = CreatureKey;    // 크리처 인덱스값 설정
            CurrentTakeOutCreature = spawnedCreature;
            
            // 크리처가 완전히 활성화된 후 초기화
            StartCoroutine(InitializeCreatureSafely(spawnedCreature, spawnPos));
        }
    }

    public void CallInAllyCreature()
    {
        if (CurrentTakeOutCreature == null)
            return;

        Debug.Log("CallInAllyCreature : " + CurrentTakeOutCreature.CreatureName);

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
        
        // 코루틴 완료 후 게임 오브젝트 비활성화
        creatureToCallIn.gameObject.SetActive(false);
        
        CurrentTakeOutCreature = null;
    }

    private IEnumerator InitializeCreatureSafely(Creature creature, Vector3 spawnPos)
    {
        // 한 프레임 대기하여 오브젝트가 완전히 활성화되도록 함
        yield return null;
        
        // 크리처 초기화
        creature.InitCreature(true);
        
        // 크기 변화 애니메이션 시작 (중복 호출 방지)
        StartCoroutine(creature.CreatrueSizeUp(spawnPos));
    }
}
