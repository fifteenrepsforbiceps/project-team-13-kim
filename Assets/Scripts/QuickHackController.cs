using UnityEngine;
using TMPro; // TextMeshProUGUI 사용을 위해 추가
using System.Collections;
using JUTPS.JUInputSystem;
using UnityEngine.UI;

public class QuickHackController : MonoBehaviour
{
    [SerializeField] private GameObject quickHackUI; // 퀵핵 UI 연결
    [SerializeField] private TextMeshProUGUI ramText; // 사이버덱 램 표시를 위한 UI 텍스트
    [SerializeField] private TextMeshProUGUI hackDescription; // 선택된 퀵핵의 설명 표시
    [SerializeField] private TextMeshProUGUI hackNameText; // 선택된 퀵핵의 이름 표시
    [SerializeField] private QuickHackData[] quickHacks; // 퀵핵 데이터들 (스크립터블 오브젝트 배열)
    [SerializeField] private Transform hackListParent; // 퀵핵 목록의 부모 오브젝트
    [SerializeField] private GameObject hackItemPrefab; // 퀵핵 항목을 표시하는 프리팹 (이름과 램 코스트 표시)

    private bool isQuickHackUIActive = false;
    private int maxRam = 10; // 최대 사이버덱 램
    private int currentRam; // 현재 사이버덱 램
    private int selectedHackIndex = 0; // 현재 선택된 퀵핵 인덱스

    private void Start()
    {
        // 퀵핵 UI 비활성화 상태로 시작
        quickHackUI.SetActive(false);
        currentRam = maxRam;
        UpdateRamText();

        // 초기 UI 업데이트
        InitializeHackList();

        // 사이버덱 램 회복 코루틴 시작
        StartCoroutine(RestoreRamOverTime());
    }

    private void Update()
    {
        // JUInputSystem 사용하여 탭 키나 마우스 측면 버튼으로 퀵핵 UI 활성화/비활성화 전환
        if (JUInput.GetButtonDown(JUInput.Buttons.OpenInventory) || Input.GetMouseButtonDown(3))
        {
            ToggleQuickHackUI();
        }

        // 퀵핵 UI가 활성화되었을 때만 퀵핵 관련 입력 처리
        if (isQuickHackUIActive)
        {
            HandleQuickHackSelection();
            if (JUInput.GetButtonDown(JUInput.Buttons.ShotButton)) // 좌클릭으로 퀵핵 실행
            {
                ExecuteHack();
            }
        }
    }

    private void ToggleQuickHackUI()
    {
        isQuickHackUIActive = !isQuickHackUIActive;
        quickHackUI.SetActive(isQuickHackUIActive);
    }

    private void HandleQuickHackSelection()
    {
        // JUInput 사용하여 마우스 휠 입력 처리
        float scroll = Input.mouseScrollDelta.y; // JUInput에서 직접 지원하지 않으므로 유니티 기본 입력 사용
        if (scroll > 0f)
        {
            // 마우스 휠을 위로 스크롤
            selectedHackIndex = Mathf.Max(0, selectedHackIndex - 1);
            UpdateHackSelection();
        }
        else if (scroll < 0f)
        {
            // 마우스 휠을 아래로 스크롤
            selectedHackIndex = Mathf.Min(quickHacks.Length - 1, selectedHackIndex + 1);
            UpdateHackSelection();
        }
    }

    private void ExecuteHack()
    {
        QuickHackData selectedHack = quickHacks[selectedHackIndex];

        if (currentRam >= selectedHack.ramCost)
        {
            // 램 충분하면 퀵핵 사용
            currentRam -= selectedHack.ramCost;
            UpdateRamText();
            Debug.Log($"{selectedHack.hackName} 실행됨.");
        }
        else
        {
            Debug.Log("램이 부족합니다.");
        }
    }

    private void UpdateHackSelection()
    {
        // 선택된 퀵핵의 설명 및 이름 업데이트
        QuickHackData selectedHack = quickHacks[selectedHackIndex];
        hackDescription.text = selectedHack.description;
        hackNameText.text = selectedHack.hackName;

        // 모든 항목의 강조 표시를 업데이트
        for (int i = 0; i < hackListParent.childCount; i++)
        {
            Transform hackItem = hackListParent.GetChild(i);
            TextMeshProUGUI hackText = hackItem.GetComponentInChildren<TextMeshProUGUI>();
            hackText.color = (i == selectedHackIndex) ? Color.yellow : Color.white;
        }
    }

    private void UpdateRamText()
    {
        // 현재 램 값 업데이트
        ramText.text = $"RAM: {currentRam}/{maxRam}";
    }

    private void InitializeHackList()
    {
        // 기존에 생성된 항목 제거
        foreach (Transform child in hackListParent)
        {
            Destroy(child.gameObject);
        }

        // 퀵핵 목록 초기화
        for (int i = 0; i < quickHacks.Length; i++)
        {
            QuickHackData hackData = quickHacks[i];
            GameObject hackItem = Instantiate(hackItemPrefab, hackListParent);
            TextMeshProUGUI hackText = hackItem.GetComponentInChildren<TextMeshProUGUI>();
            hackText.text = $"{hackData.hackName} - RAM Cost: {hackData.ramCost}";
        }

        // 선택된 항목 초기화
        UpdateHackSelection();
    }

    private IEnumerator RestoreRamOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (currentRam < maxRam)
            {
                currentRam += 1;
                UpdateRamText();
            }
        }
    }
}