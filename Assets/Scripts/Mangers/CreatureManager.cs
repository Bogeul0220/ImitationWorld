using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }

    public Dictionary<string, Creature> creaturePrefabs; // 크리쳐 프리팹 딕셔너리
    public List<Creature> CreaturePrefabList;   // 크리쳐 프리팹 저장 리스트

    [Header("길들인 크리쳐")]
    public List<Creature> SpawnedTamedCreatures; // 생성된 길들여진 크리쳐 리스트
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
    }

    // Start is called before the first frame update
    void Start()
    {
        _= StartCoroutine(SpawnCreatureCoroutine());
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
}
