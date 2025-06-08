using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectPoolManager
{
    // 각 프리팹에 대한 풀 정보 클래스
    private class PoolData
    {
        public GameObject Prefab;       // 원본 프리팹
        public Queue<GameObject> PoolQueue;  // 비활성화된 오브젝트를 저장하는 큐
        public int MaxSize;             // 최대 풀 크기

        public PoolData(GameObject prefab, int initialSize, int MaxSize = int.MaxValue)
        {
            this.Prefab = prefab;
            this.MaxSize = MaxSize;
            this.PoolQueue = new Queue<GameObject>();

            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Object.Instantiate(prefab);
                obj.SetActive(false);
                PoolQueue.Enqueue(obj);
            }
        }
    }

    // 프리팹 ID (GetInstanceID)와 풀 데이터를 매핑하는 딕셔너리
    private static Dictionary<int, PoolData> poolDictionary = new();

    // 특정 프리팹에 대한 풀을 생성하는 메서드
    public static void CreatePool(GameObject prefab, int initialSize, int maxSize = int.MaxValue)
    {
        int key = prefab.GetInstanceID();

        if (!poolDictionary.ContainsKey(key))
        {
            PoolData poolData = new PoolData(prefab, initialSize, maxSize);
            poolDictionary.Add(key, poolData);
        }
        else
            return; // 이미 풀 데이터가 존재하면 아무 작업도 하지 않음
    }

    // 오브젝트를 풀에서 가져오는 메서드
    public static T Get<T>(GameObject prefab) where T : MonoBehaviour
    {
        int key = prefab.GetInstanceID();

        // 해당 프리팹에 대한 풀 데이터가 존재하는지 확인 후 없으면 생성
        if (!poolDictionary.ContainsKey(key))
            CreatePool(prefab, 1);

        PoolData pool = poolDictionary[key];
        GameObject obj;

        if (pool.PoolQueue.Count > 0)
        {
            obj = pool.PoolQueue.Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = Object.Instantiate(pool.Prefab);  // 풀에 오브젝트가 없으면 새로 생성
        }

        return obj.GetComponent<T>();
    }

    public static void Return(GameObject obj)
    {
        obj.SetActive(false);
        int key = obj.GetInstanceID();

        if (poolDictionary.ContainsKey(key))
        {
            PoolData pool = poolDictionary[key];

            // 풀의 크기가 최대 크기를 초과하지 않는 경우에만 반환
            if (pool.PoolQueue.Count < pool.MaxSize)
            {
                pool.PoolQueue.Enqueue(obj);
                return;
            }
        }

        Object.Destroy(obj);  // 최대 크기를 초과하면 오브젝트를 파괴
    }

    public static void ClearPool(GameObject prefab)
    {
        int key = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(key))
        {
            PoolData pool = poolDictionary[key];

            while (pool.PoolQueue.Count > 0)
            {
                GameObject obj = pool.PoolQueue.Dequeue();
                Object.Destroy(obj);
            }

            poolDictionary.Remove(key);  // 풀 데이터 제거
        }
    }

    public static void ClearAllPools()
    {
        foreach (var pool in poolDictionary.Values)
        {
            while (pool.PoolQueue.Count > 0)
            {
                GameObject obj = pool.PoolQueue.Dequeue();
                Object.Destroy(obj);
            }
        }

        poolDictionary.Clear();  // 모든 풀 데이터 제거
    }
}
