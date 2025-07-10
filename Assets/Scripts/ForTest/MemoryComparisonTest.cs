using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MemoryComparisonTest : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private int testItemCount = 1000;
    [SerializeField] private bool runComparisonOnStart = true;
    [SerializeField] private ItemData itemData;
    
    [Header("테스트 결과")]
    [SerializeField] private long oldWayMemory;
    [SerializeField] private long newWayMemory;
    [SerializeField] private long memorySaved;
    [SerializeField] private float savingsPercentage;

    void Start()
    {
        if (runComparisonOnStart)
        {
            StartCoroutine(RunMemoryComparison());
        }
    }

    [ContextMenu("메모리 사용량 비교 테스트")]
    public void RunComparison()
    {
        StartCoroutine(RunMemoryComparison());
    }

    private IEnumerator RunMemoryComparison()
    {
        Debug.Log("=== 메모리 사용량 비교 테스트 시작 ===");
        Debug.Log($"테스트 아이템 수: {testItemCount}개");
        
        // 1. 기존 방식 테스트 (데이터 중복 방식)
        Debug.Log("1. 기존 방식 테스트 (각 아이템이 데이터를 복사해서 가짐)");
        yield return StartCoroutine(TestOldWay());
        
        // 메모리 정리
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        yield return new WaitForSeconds(2f);
        
        // 2. 개선된 방식 테스트 (ScriptableObject 참조 방식)
        Debug.Log("2. 개선된 방식 테스트 (ScriptableObject 참조 - 현재 프로젝트 방식)");
        yield return StartCoroutine(TestNewWay());
        
        // 3. 결과 분석
        AnalyzeResults();
        
        Debug.Log("=== 메모리 비교 테스트 완료 ===");
    }

    private IEnumerator TestOldWay()
    {
        // 기존 방식: 각 아이템이 데이터를 복사해서 가짐 (가상의 비효율적인 방식)
        var oldWayItems = new List<OldWayItem>();
        
        long initialMemory = GC.GetTotalMemory(false);
        Debug.Log($"기존 방식 초기 메모리: {FormatBytes(initialMemory)}");
        
        for (int i = 0; i < testItemCount; i++)
        {
            var item = new OldWayItem
            {
                itemName = "철검",  // 실제 아이템 이름
                itemType = ItemType.Equipment,
                description = "강력한 철검입니다. 기본적인 공격 무기로 사용됩니다.",
                itemID = 1001,
                itemIcon = null, // 실제 스프라이트가 있다면 여기에 할당
                amount = UnityEngine.Random.Range(1, 100),
                damage = 15,
                maxAmount = 99,
                equipmentType = EquipmentType.Weapon
            };
            oldWayItems.Add(item);
            
            if (i % 100 == 0)
            {
                yield return null;
            }
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        long finalMemory = GC.GetTotalMemory(false);
        oldWayMemory = finalMemory - initialMemory;
        
        Debug.Log($"기존 방식 메모리 사용량: {FormatBytes(oldWayMemory)}");
        Debug.Log($"기존 방식 아이템당 평균: {FormatBytes(oldWayMemory / testItemCount)}");
        
        // 메모리 정리
        oldWayItems.Clear();
    }

    private IEnumerator TestNewWay()
    {
        // 개선된 방식: ScriptableObject 참조 (현재 프로젝트의 방식)
        var newWayItems = new List<ItemBase>();
        
        long initialMemory = GC.GetTotalMemory(false);
        Debug.Log($"개선된 방식 초기 메모리: {FormatBytes(initialMemory)}");
        
        for (int i = 0; i < testItemCount; i++)
        {
            var item = itemData.CreateItem(); // 실제 프로젝트의 아이템 생성 방식
            newWayItems.Add(item);
            
            if (i % 100 == 0)
            {
                yield return null;
            }
        }
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        long finalMemory = GC.GetTotalMemory(false);
        newWayMemory = finalMemory - initialMemory;
        
        Debug.Log($"개선된 방식 메모리 사용량: {FormatBytes(newWayMemory)}");
        Debug.Log($"개선된 방식 아이템당 평균: {FormatBytes(newWayMemory / testItemCount)}");
        
        // 메모리 정리
        newWayItems.Clear();
    }

    private void AnalyzeResults()
    {
        memorySaved = oldWayMemory - newWayMemory;
        savingsPercentage = (float)memorySaved / oldWayMemory * 100f;
        
        Debug.Log("=== 메모리 사용량 비교 결과 ===");
        Debug.Log($"기존 방식: {FormatBytes(oldWayMemory)}");
        Debug.Log($"개선된 방식: {FormatBytes(newWayMemory)}");
        Debug.Log($"절약된 메모리: {FormatBytes(memorySaved)}");
        Debug.Log($"절약률: {savingsPercentage:F1}%");
        
        if (savingsPercentage > 50f)
        {
            Debug.Log("✅ 매우 효율적인 메모리 절약을 달성했습니다!");
        }
        else if (savingsPercentage > 30f)
        {
            Debug.Log("✅ 상당한 메모리 절약을 달성했습니다.");
        }
        else if (savingsPercentage > 10f)
        {
            Debug.Log("✅ 적절한 메모리 절약을 달성했습니다.");
        }
        else
        {
            Debug.Log("⚠️ 메모리 절약 효과가 미미합니다. 추가 최적화를 고려해보세요.");
        }
        
        // 포트폴리오용 결과 출력
        Debug.Log("=== 포트폴리오용 결과 요약 ===");
        Debug.Log($"아이템 {testItemCount}개 생성 시:");
        Debug.Log($"• 기존 방식 (데이터 복사): {FormatBytes(oldWayMemory)} ({oldWayMemory / testItemCount}바이트/아이템)");
        Debug.Log($"• 개선된 방식 (ScriptableObject 참조): {FormatBytes(newWayMemory)} ({newWayMemory / testItemCount}바이트/아이템)");
        Debug.Log($"• 메모리 절약: {FormatBytes(memorySaved)} ({savingsPercentage:F1}%)");
        Debug.Log($"• 절약 효과: 같은 아이템 {testItemCount}개 생성 시 {savingsPercentage:F1}% 메모리 절약");
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

    // 기존 방식 아이템 클래스 (비교용) - 데이터를 복사해서 가짐
    private class OldWayItem
    {
        public string itemName;
        public ItemType itemType;
        public string description;
        public int itemID;
        public Sprite itemIcon;
        public int amount;
        public int damage;
        public int maxAmount;
        public EquipmentType equipmentType;
    }

    // 테스트용 아이템 데이터
    private class TestItemData : ItemData
    {
        // 테스트용으로 public 필드 추가
        public string testItemName = "테스트 아이템";
        public ItemType testItemType = ItemType.Equipment;
        public string testDescription = "테스트용 아이템입니다.";
        public int testItemID = 1;
        public Sprite testItemIcon = null;
        
        public override ItemBase CreateItem()
        {
            return new TestItem(this);
        }
    }

    // 테스트용 아이템
    private class TestItem : ItemBase
    {
        public TestItem(ItemData data) : base(data) { }
    }
} 