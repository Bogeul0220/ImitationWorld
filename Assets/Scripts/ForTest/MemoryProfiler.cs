using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MemoryProfiler : MonoBehaviour
{
    [Header("메모리 측정 설정")]
    [SerializeField] private int itemCount = 1000;
    [SerializeField] private bool measureOnStart = true;
    
    [Header("측정 결과")]
    [SerializeField] private long initialMemory;
    [SerializeField] private long afterItemCreationMemory;
    [SerializeField] private long memoryDifference;
    [SerializeField] private float memoryUsageMB;

    private List<ItemBase> createdItems = new List<ItemBase>();

    void Start()
    {
        if (measureOnStart)
        {
            StartCoroutine(MeasureMemoryUsage());
        }
    }

    [ContextMenu("메모리 사용량 측정")]
    public void MeasureMemory()
    {
        StartCoroutine(MeasureMemoryUsage());
    }

    private IEnumerator MeasureMemoryUsage()
    {
        Debug.Log("=== 메모리 사용량 측정 시작 ===");
        
        // GC 강제 실행으로 메모리 정리
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        yield return new WaitForSeconds(1f);

        // 초기 메모리 측정
        initialMemory = GC.GetTotalMemory(false);
        Debug.Log($"초기 메모리: {FormatBytes(initialMemory)}");

        // 아이템 생성
        Debug.Log($"{itemCount}개의 아이템 생성 중...");
        yield return StartCoroutine(CreateItems());

        // GC 강제 실행
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        yield return new WaitForSeconds(1f);

        // 아이템 생성 후 메모리 측정
        afterItemCreationMemory = GC.GetTotalMemory(false);
        memoryDifference = afterItemCreationMemory - initialMemory;
        memoryUsageMB = memoryDifference / (1024f * 1024f);

        Debug.Log($"아이템 생성 후 메모리: {FormatBytes(afterItemCreationMemory)}");
        Debug.Log($"메모리 증가량: {FormatBytes(memoryDifference)} ({memoryUsageMB:F2} MB)");
        Debug.Log($"아이템당 평균 메모리: {FormatBytes(memoryDifference / itemCount)}");

        // 상세 분석
        AnalyzeMemoryUsage();
        
        Debug.Log("=== 메모리 측정 완료 ===");
    }

    private IEnumerator CreateItems()
    {
        createdItems.Clear();
        
        // 실제 아이템 데이터를 사용하여 아이템 생성
        var itemDataList = Resources.LoadAll<ItemData>("ScriptableObjects/Items");
        
        if (itemDataList.Length == 0)
        {
            Debug.LogWarning("아이템 데이터를 찾을 수 없습니다. 더미 아이템을 생성합니다.");
            CreateDummyItems();
        }
        else
        {
            for (int i = 0; i < itemCount; i++)
            {
                var randomItemData = itemDataList[UnityEngine.Random.Range(0, itemDataList.Length)];
                var item = randomItemData.CreateItem();
                createdItems.Add(item);
                
                if (i % 100 == 0)
                {
                    yield return null; // 프레임 분할
                }
            }
        }
    }

    private void CreateDummyItems()
    {
        // 더미 아이템 생성 (테스트용)
        for (int i = 0; i < itemCount; i++)
        {
            // 더미 데이터 생성
            var dummyData = ScriptableObject.CreateInstance<DummyItemData>();
            var dummyItem = new DummyItem(dummyData);
            createdItems.Add(dummyItem);
        }
    }

    private void AnalyzeMemoryUsage()
    {
        Debug.Log("=== 상세 메모리 분석 ===");
        
        // 힙 메모리 정보
        long heapSize = GC.GetTotalMemory(false);
        long maxHeapSize = GC.GetTotalMemory(true);
        
        Debug.Log($"현재 힙 크기: {FormatBytes(heapSize)}");
        Debug.Log($"최대 힙 크기: {FormatBytes(maxHeapSize)}");
        
        // GC 세대별 정보
        for (int i = 0; i <= GC.MaxGeneration; i++)
        {
            int count = GC.CollectionCount(i);
            Debug.Log($"GC 세대 {i} 수집 횟수: {count}");
        }
        
        // 메모리 사용량 평가
        if (memoryUsageMB < 1f)
        {
            Debug.Log("✅ 메모리 사용량이 매우 효율적입니다!");
        }
        else if (memoryUsageMB < 5f)
        {
            Debug.Log("✅ 메모리 사용량이 적절합니다.");
        }
        else if (memoryUsageMB < 10f)
        {
            Debug.Log("⚠️ 메모리 사용량이 다소 높습니다. 최적화를 고려해보세요.");
        }
        else
        {
            Debug.Log("❌ 메모리 사용량이 너무 높습니다. 최적화가 필요합니다.");
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        
        return $"{number:n1} {suffixes[counter]}";
    }

    [ContextMenu("메모리 정리")]
    public void ClearMemory()
    {
        createdItems.Clear();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Debug.Log("메모리 정리 완료");
    }

    // 더미 아이템 데이터 클래스 (테스트용)
    private class DummyItemData : ItemData
    {
        public override ItemBase CreateItem()
        {
            return new DummyItem(this);
        }
    }
    
    // 더미 아이템 클래스 (테스트용)
    private class DummyItem : ItemBase
    {
        public DummyItem(ItemData data) : base(data) { }
    }
} 