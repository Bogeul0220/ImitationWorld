using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }

    public Dictionary<string, Creature> creaturePrefabs; // 크리쳐 프리팹 딕셔너리

    [Header("길들인 크리쳐")]
    public List<Creature> spawnedTamedCreatures; // 생성된 길들여진 크리쳐 리스트
    public Creature currentTakeOutCreature; // 현재 소환한 아군 크리쳐

    [Header("야생 크리쳐")]
    public List<Creature> spawnedWildCreatures; // 생성된 야생 크리쳐 리스트
    public int maxTamedCreatures = 5; // 최대 길들여진 크리쳐 수
    public int maxWildCreatures = 20; // 최대 야생 크리쳐 수
    public float spawnRadius = 50f; // 크리쳐 생성 반경
    public float spawnInterval = 10f; // 크리쳐 생성 간격
    private float nextSpawnTime = 0f; // 다음 크리쳐 생성 시간
    public LayerMask terrainLayer; // 필드 레이어


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }
    
    public IEnumerator SpawnCreatures()
    {
        while (true)
        {
            yield return null;
        }
    }
}
