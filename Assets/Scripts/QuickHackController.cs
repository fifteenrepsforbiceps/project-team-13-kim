using UnityEngine;
using TMPro; // TextMeshProUGUI 사용을 위해 추가
using System.Collections;
using JUTPS.JUInputSystem;
using UnityEngine.UI;
using JUTPS; 

public class QuickHackController : MonoBehaviour
{
    [Header("Quick Hack UI")]
    [SerializeField] private GameObject quickHackUIPrefab; // 퀵핵 UI 프리팹
    [SerializeField] private Transform quickHackPanel; // 퀵핵 UI가 소속된 패널
    [SerializeField] private Transform[] quickHackSlots; // 퀵핵 UI가 배치될 6개의 슬롯
    [SerializeField] private QuickHackData[] quickHacks; // 퀵핵 데이터들 (스크립터블 오브젝트 배열, 최대 6개)
    [SerializeField] private TextMeshProUGUI ramText; // 사이버덱 램 표시를 위한 UI 텍스트
    [SerializeField] private TextMeshProUGUI hackDescription; // 선택된 퀵핵의 설명 표시
    [SerializeField] private GameObject ramSquarePrefab; // 네모칸 이미지 프리팹
    [SerializeField] private Transform ramContainer; // 램 네모칸들을 배치할 부모 오브젝트
    private Image[] ramSquares; // 개별 램 네모칸 이미지 배열

    [Header("Time Control Settings")]
    private float detectionRadius = 100f; // 화면상의 감지 반경 (픽셀)
    [SerializeField] private float slowTimeScale = 0.3f; // 슬로우 모션 강도
    [SerializeField] private float transitionSpeed = 5f; // 전환 속도
    private float currentTimeScale = 1f;

    [Header("Crosshair Settings")]
    [SerializeField] private GameObject crosshair;
    private RectTransform crosshairRect;
    private Image crosshairImage;
    private Vector3 originalCrosshairScale;
    private Color originalCrosshairColor;
    private Vector3 targetScale;
    private Color targetColor;

    private bool isQuickHackUIActive = false;
    private int maxRam = 50; // 최대 사이버덱 램
    private int currentRam; // 현재 사이버덱 램
    private int selectedHackIndex = 0; // 현재 선택된 퀵핵 인덱스
    private Coroutine ramRestoreCoroutine = null;


    [SerializeField] private bool enemyDetectionGUI = false; // 인스펙터에서 UI 활성화 여부를 조절할 수 있도록 합니다.

    // 다른 클래스에서 퀵핵 UI 상태를 확인할 수 있도록 public 프로퍼티 추가
    public bool IsQuickHackUIActive => isQuickHackUIActive;

    private bool isEnemyTargeted = false; // 적이 조준되었는지 여부를 저장하는 플래그

    private void Start()
    {
        quickHackPanel.gameObject.SetActive(false);
        currentRam = maxRam;

        // 램 네모칸 초기화
        ramSquares = new Image[maxRam];
        for (int i = 0; i < maxRam; i++)
        {
            GameObject square = Instantiate(ramSquarePrefab, ramContainer);
            ramSquares[i] = square.GetComponent<Image>();

            // 네모칸의 위치 설정 (각 칸을 x축으로 15씩 띄워서 배치)
            RectTransform rectTransform = square.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(15 * i, 0); // X축으로 15씩 이동, Y는 그대로
            }
        }

        InitializeHackSlots();
        UpdateRamText();
        UpdateRamDisplay();

        crosshairRect = crosshair.GetComponent<RectTransform>();
        crosshairImage = crosshair.GetComponent<Image>();
        originalCrosshairScale = crosshairRect.localScale;
        originalCrosshairColor = crosshairImage.color;
        targetScale = new Vector3(0.5f, 0.5f, 0.5f);
        targetColor = Color.red;


        // 시간 관련 초기화
        currentTimeScale = 1f;
        Time.timeScale = 1f;

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
            CheckEnemyAim();
            CheckEnemyInDetectionRadius();
            if (isEnemyTargeted && Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F)) // 좌클릭 또는 F키로 퀵핵 실행
            {
                ExecuteHack();
            }
        }
        else
        {
            // UI 비활성화시 시간과 크로스헤어 초기화
            Time.timeScale = 1f;
            currentTimeScale = 1f;
            // UI 비활성화시 크로스헤어 초기 상태로
            crosshairRect.localScale = originalCrosshairScale;
            crosshairImage.color = originalCrosshairColor;
        }

        UpdateRamDisplay();

        // Debug.Log("isEnemyTargeted: " + isEnemyTargeted);
    }

    private void ToggleQuickHackUI()
    {
        isQuickHackUIActive = !isQuickHackUIActive;
        quickHackPanel.gameObject.SetActive(isQuickHackUIActive);

        if (isQuickHackUIActive)
        {
            crosshair.SetActive(true);  // 퀵핵 UI 활성화시 크로스헤어 표시
            // 이전에 선택한 퀵핵 인덱스를 불러오기
            selectedHackIndex = PlayerPrefs.GetInt("LastSelectedHackIndex", 0);
            
            // 패널이 활성화되면 슬롯도 활성화하고 선택된 퀵핵 업데이트
            foreach (var slot in quickHackSlots)
            {
                slot.gameObject.SetActive(true);
            }

            // 선택된 퀵핵 UI 업데이트
            UpdateHackSelection();
        }
        else
        {
            crosshair.SetActive(false);  // 퀵핵 UI 비활성화시 크로스헤어 숨김
            // 패널이 비활성화되면 현재 선택된 퀵핵 인덱스를 저장
            PlayerPrefs.SetInt("LastSelectedHackIndex", selectedHackIndex);
        }
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
            UpdateRamDisplay();
            Debug.Log($"{selectedHack.hackName} 실행됨.");

            // 적에게 피해를 주는 로직 추가
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    HackUploadUI enemyHackUI = hit.collider.GetComponent<HackUploadUI>();
                    if (enemyHackUI != null)
                    {
                        enemyHackUI.StartUpload(selectedHack.uploadTime);
                    }

                    JUHealth enemyHealth = hit.collider.GetComponent<JUHealth>();
                    if (enemyHealth != null)
                    {
                        StartCoroutine(DealDamageOverTime(enemyHealth, selectedHack));
                    }
                }
            }

            // RAM이 소모되었을 때만 리젠 코루틴 시작
            if (ramRestoreCoroutine == null)
            {
                ramRestoreCoroutine = StartCoroutine(RestoreRamOverTime());
            }
        }
        else
        {
            Debug.Log("램이 부족합니다.");
        }
    }


    private void UpdateHackSelection()
    {
        QuickHackData selectedHack = quickHacks[selectedHackIndex];
        hackDescription.text = selectedHack.description;

        // 모든 항목의 강조 표시를 업데이트
        for (int i = 0; i < quickHackSlots.Length; i++)
        {
            var hackText = quickHackSlots[i].GetComponentInChildren<TextMeshProUGUI>();
            var outline = quickHackSlots[i].GetComponentInChildren<Outline>();

            if (hackText != null)
            {
                hackText.color = (i == selectedHackIndex) ? Color.yellow : Color.white;
            }

            if (outline != null)
            {
                outline.enabled = (i == selectedHackIndex);
            }
        }

        // 선택된 퀵핵의 램 소모량을 오른쪽부터 아웃라인으로 강조 표시
        int ramCost = selectedHack.ramCost;
        for (int i = 0; i < maxRam; i++)
        {
            var outline = ramSquares[i].GetComponent<Outline>();
            if (outline != null)
            {
                if (i >= currentRam - ramCost && i < currentRam)
                {
                    outline.enabled = true; // 램 코스트 만큼의 네모칸 아웃라인 활성화
                }
                else
                {
                    outline.enabled = false;
                }
            }
        }
    }


    private void UpdateRamText()
    {
        // 현재 램 값 업데이트
        ramText.text = $"{currentRam}/{maxRam}";
    }

    private void UpdateRamDisplay()
    {
        // 현재 선택된 퀵핵의 램 소모량
        int ramCost = quickHacks[selectedHackIndex].ramCost;

        for (int i = 0; i < maxRam; i++)
        {
            var outline = ramSquares[i].GetComponent<Outline>();
            if (outline != null)
            {
                // 선택된 퀵핵의 예상 사용량 표시를 위한 아웃라인
                outline.enabled = (i >= currentRam - ramCost && i < currentRam);
            }

            // RAM 상태 표시 업데이트
            if (i < currentRam)
            {
                // 현재 색상이 회색이고 사용 가능한 상태로 바뀌는 경우에만 페이드 효과 적용
                if (ramSquares[i].color == Color.gray)
                {
                    StartCoroutine(FadeInSquare(ramSquares[i]));
                }
                else
                {
                    ramSquares[i].color = Color.white;
                }
            }
            else
            {
                ramSquares[i].color = Color.gray;
            }
        }
    }

    private IEnumerator FadeInSquare(Image square)
    {
        Color startColor = Color.gray;
        Color endColor = Color.white;
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            square.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        square.color = endColor;
    }


    private void InitializeHackSlots()
    {
        // 퀵핵 슬롯 초기화 (최대 6개)
        for (int i = 0; i < quickHackSlots.Length; i++)
        {
            if (i < quickHacks.Length)
            {
                QuickHackData hackData = quickHacks[i];
                GameObject hackItem = Instantiate(quickHackUIPrefab, quickHackSlots[i]);
                TextMeshProUGUI[] textComponents = hackItem.GetComponentsInChildren<TextMeshProUGUI>();

                foreach (var textComponent in textComponents)
                {
                    if (textComponent.name == "QuickHackName")
                    {
                        textComponent.text = hackData.hackName;
                    }
                    else if (textComponent.name == "QuickHackCost")
                    {
                        textComponent.text = $"{hackData.ramCost}";
                    }
                }
            }
        }
    }

    private IEnumerator RestoreRamOverTime()
    {
        while (currentRam < maxRam)
        {
            yield return new WaitForSeconds(1f);
            if (currentRam < maxRam)
            {
                currentRam += 1;
                UpdateRamText();
            }
        }
        // RAM이 최대치에 도달하면 코루틴 참조 제거
        ramRestoreCoroutine = null;
    }


    private void CheckEnemyAim()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                // 적이 조준되었음을 기록
                isEnemyTargeted = true;

                // 기존 크로스헤어 효과는 그대로 유지
                crosshairRect.localScale = Vector3.Lerp(crosshairRect.localScale, targetScale, Time.deltaTime * 10f);
                crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * 10f);
            }
            else
            {
                isEnemyTargeted = false;

                crosshairRect.localScale = Vector3.Lerp(crosshairRect.localScale, originalCrosshairScale, Time.deltaTime * 10f);
                crosshairImage.color = Color.Lerp(crosshairImage.color, originalCrosshairColor, Time.deltaTime * 10f);
            }
        }
        else
        {
            isEnemyTargeted = false;

            crosshairRect.localScale = Vector3.Lerp(crosshairRect.localScale, originalCrosshairScale, Time.deltaTime * 10f);
            crosshairImage.color = Color.Lerp(crosshairImage.color, originalCrosshairColor, Time.deltaTime * 10f);
        }
    }

    private void CheckEnemyInDetectionRadius()
    {
        Vector2 screenCenter = new Vector2(Screen.width/2f, Screen.height/2f);
        int circumferenceRayCount = 10;  // 원 둘레에서 쏠 ray 개수
        int randomRayCount = 10;         // 원 내부에서 랜덤하게 쏠 ray 개수
        bool enemyDetected = false;      // 적 감지 여부를 추적
        
        // 1. 중앙에서 ray 발사
        Ray centerRay = Camera.main.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(centerRay, out RaycastHit centerHit))
        {
            if (centerHit.collider.CompareTag("Enemy"))
            {
                enemyDetected = true;
            }
        }

        // 2. 원 둘레를 따라 균등하게 ray 발사
        if (!enemyDetected)  // 중앙에서 적을 못 찾았을 경우에만 계속 검사
        {
            float angleStep = 360f / circumferenceRayCount;
            for (int i = 0; i < circumferenceRayCount; i++)
            {
                float angle = i * angleStep;
                Vector2 rayPos = screenCenter + new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * detectionRadius * 0.8f,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * detectionRadius * 0.8f
                );

                Ray ray = Camera.main.ScreenPointToRay(rayPos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        enemyDetected = true;
                        break;
                    }
                }
            }
        }

        // 3. 원 내부의 랜덤한 위치에서 ray 발사
        if (!enemyDetected)  // 아직도 적을 못 찾았을 경우에만 계속 검사
        {
            for (int i = 0; i < randomRayCount; i++)
            {
                float randomAngle = Random.Range(0f, 360f);
                float randomRadius = Random.Range(0f, detectionRadius * 0.7f);
                
                Vector2 rayPos = screenCenter + new Vector2(
                    Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomRadius,
                    Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomRadius
                );

                Ray ray = Camera.main.ScreenPointToRay(rayPos);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        enemyDetected = true;
                        break;
                    }
                }
            }
        }

        // 적 감지 여부에 따른 시간 조절
        if (enemyDetected)
        {
            // 적이 감지되면 시간을 느리게
            currentTimeScale = Mathf.Lerp(currentTimeScale, slowTimeScale, Time.unscaledDeltaTime * transitionSpeed);
        }
        else
        {
            // 적이 감지되지 않으면 시간을 정상으로
            currentTimeScale = Mathf.Lerp(currentTimeScale, 1f, Time.unscaledDeltaTime * transitionSpeed);
        }

        // 실제 시간 스케일 적용
        Time.timeScale = currentTimeScale;

        // 디버그용 로그
        //Debug.Log($"Enemy Detected: {enemyDetected}, Time Scale: {Time.timeScale}");
    }
    
    // OnGUI로 원형 영역 시각화 (디버그용)
    private void OnGUI()
    {
        if (isQuickHackUIActive && enemyDetectionGUI)
        {
            float circleX = Screen.width / 2f - detectionRadius;
            float circleY = Screen.height / 2f - detectionRadius;
            GUI.color = new Color(1, 1, 1, 0.1f);
            GUI.DrawTexture(new Rect(circleX, circleY, detectionRadius * 2, detectionRadius * 2), 
                            Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, 
                            Color.white, 1, 20);
        }
    }

    // 디버그용 - Scene 뷰에서 ray들을 시각화
    private void OnDrawGizmos()
    {
        if (!isQuickHackUIActive) return;
        
        Vector2 screenCenter = new Vector2(Screen.width/2f, Screen.height/2f);
        
        // 1. 중앙 ray 시각화
        Ray centerRay = Camera.main.ScreenPointToRay(screenCenter);
        Gizmos.color = Color.red;  // 중앙 ray는 빨간색
        Gizmos.DrawRay(centerRay.origin, centerRay.direction * 100f);
        
        // 2. 원 둘레의 ray 시각화
        int circumferenceRayCount = 10;
        float angleStep = 360f / circumferenceRayCount;
        Gizmos.color = Color.yellow;  // 원 둘레 ray는 노란색
        for (int i = 0; i < circumferenceRayCount; i++)
        {
            float angle = i * angleStep;
            Vector2 rayPos = screenCenter + new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad) * detectionRadius * 0.8f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * detectionRadius * 0.8f
            );
            
            Ray ray = Camera.main.ScreenPointToRay(rayPos);
            Gizmos.DrawRay(ray.origin, ray.direction * 100f);
        }
        
        // 3. 랜덤 ray 시각화 (매 프레임 위치가 바뀌게 됨)
        int randomRayCount = 10;
        Gizmos.color = Color.green;  // 랜덤 ray는 초록색
        for (int i = 0; i < randomRayCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            float randomRadius = Random.Range(0f, detectionRadius * 0.7f);
            
            Vector2 rayPos = screenCenter + new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomRadius,
                Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomRadius
            );
            
            Ray ray = Camera.main.ScreenPointToRay(rayPos);
            Gizmos.DrawRay(ray.origin, ray.direction * 100f);
        }
    }

    private IEnumerator DealDamageAfterUpload(JUHealth enemyHealth, QuickHackData hackData)
    {
        yield return new WaitForSeconds(hackData.uploadTime);
        enemyHealth.DoDamage(hackData.damage);
        Debug.Log($"적에게 {hackData.damage} 만큼의 피해를 주었습니다.");
    }

    private IEnumerator DealDamageOverTime(JUHealth enemyHealth, QuickHackData hackData)
    {
        float elapsedTime = 0f;
        float tickInterval = Time.fixedDeltaTime; // FixedUpdate와 동일한 간격으로 틱 설정 (일반적으로 0.02초)
        float damagePerTick = hackData.damage / (hackData.duration / tickInterval); // 틱당 데미지 계산

        while (elapsedTime < hackData.duration)
        {
            enemyHealth.DoDamage(damagePerTick); // 적에게 틱당 데미지 적용
            elapsedTime += tickInterval;
            yield return new WaitForSeconds(tickInterval); // 매 틱 간격마다 대기
        }

        Debug.Log($"적에게 총 {hackData.damage} 만큼의 피해를 {hackData.duration}초 동안 가했습니다.");
    }
}
