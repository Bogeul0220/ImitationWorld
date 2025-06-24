using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

/// <summary>
/// 간단한 Billboard 스크립트 - UI가 카메라를 향함
/// </summary>
public class Billboard : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float maxDistance = 15f;

    [Header("거리 기반 숨김")]
    [SerializeField] private bool useDistanceFade = true;
    [SerializeField] private float fadeStartDistance = 12f; // 페이드 시작 거리

    private Camera mainCamera;
    private Transform cameraTransform;
    private CanvasGroup canvasGroup;


    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }

        // Canvas 자동 설정
        SetupCanvas();
        
        // Canvas Group 설정
        SetupCanvasGroup();
    }

    private void SetupCanvas()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = mainCamera;
        }
    }

    private void SetupCanvasGroup()
    {
        // Canvas Group 컴포넌트 가져오기 또는 추가
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Update()
    {
        if (cameraTransform == null) return;

        // 거리 체크
        float distance = Vector3.Distance(transform.position, cameraTransform.position);
        
        // 거리 기반 숨김 처리
        if (useDistanceFade)
        {
            HandleDistanceFade(distance);
        }
        
        // 최대 거리 체크
        if (distance > maxDistance) return;

        // 카메라를 향해 회전
        if (smoothRotation)
        {
            Vector3 direction = cameraTransform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.LookAt(cameraTransform);
        }
    }

    /// <summary>
    /// 거리에 따른 페이드 효과 처리
    /// </summary>
    private void HandleDistanceFade(float distance)
    {
        if (canvasGroup == null) return;
 
        if (distance >= maxDistance)
        {
            // 완전히 숨김
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else if (distance <= fadeStartDistance)
        {
            // 완전히 보임
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            // 페이드 효과
            float fadeRange = maxDistance - fadeStartDistance;
            float currentRange = distance - fadeStartDistance;
            float alpha = 1f - (currentRange / fadeRange);
            
            canvasGroup.alpha = alpha;
            canvasGroup.interactable = alpha > 0.5f; // 반투명할 때는 상호작용 비활성화
            canvasGroup.blocksRaycasts = alpha > 0.5f;
        }
    }
}