using UnityEngine;

public class EnemyUIFollow : MonoBehaviour
{
    public GameObject uiPrefab; // UI 프리팹 할당
    public Canvas canvas; // 캔버스를 명시적으로 할당
    public Transform headTransform; // 적의 머리 Transform을 할당
    
    private GameObject uiInstance; // 적 위에 표시될 UI 인스턴스
    private RectTransform uiTransform; // UI의 RectTransform

    private Camera mainCamera; // 메인 카메라

    void Start()
    {
        // 메인 카메라 참조
        mainCamera = Camera.main;

        // UI 프리팹을 인스턴스화 하고, 할당된 캔버스의 자식으로 설정
        uiInstance = Instantiate(uiPrefab, canvas.transform);

        // UI RectTransform 참조
        uiTransform = uiInstance.GetComponent<RectTransform>();
    }

    void Update()
    {
        // 적 머리의 위치에서 UI가 따라가도록 하기 위해 headTransform 사용
        Vector3 targetPosition = (headTransform != null) ? headTransform.position : transform.position + Vector3.up * 2.0f;
        
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

        // 화면의 앞쪽에 있을 때만 UI 표시
        if (screenPosition.z > 0)
        {
            uiTransform.position = screenPosition;
        }
    }

    void OnDestroy()
    {
        if (uiInstance != null)
        {
            Destroy(uiInstance);
        }
    }
}
