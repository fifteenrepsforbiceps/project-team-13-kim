using UnityEngine;
using UnityEngine.UI;
using System.Collections; // IEnumerator를 사용하기 위해 추가

public class HackUploadUI : MonoBehaviour
{
    public QuickHackData quickHackData; // 퀵핵 데이터 스크립터블 오브젝트
    [SerializeField] private Canvas canvas; // 캔버스를 명시적으로 할당
    [SerializeField] private Transform hackUploadUIPosition; // UI의 위치를 지정할 Transform

    private Transform enemyTransform; // 적의 Transform 참조
    private Camera mainCamera;
    private GameObject hackUploadUIInstance; // 퀵핵 UI 인스턴스
    private Image uploadFillImage; // 업로드 진행을 나타내는 Fill 이미지
    private Image durationFillImage; // 지속 시간 진행을 나타내는 Fill 이미지

    private void Start()
    {
        // 적의 Transform 설정
        enemyTransform = transform; // 적의 Transform을 가져옴

        // 메인 카메라 가져오기
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("메인 카메라를 찾을 수 없습니다. 카메라 태그를 확인하세요.");
        }

        // 캔버스 확인
        if (canvas == null)
        {
            Debug.LogError("캔버스가 할당되지 않았습니다. 캔버스를 할당해 주세요.");
            return;
        }
    }

    private void Update()
    {
        if (hackUploadUIPosition != null && hackUploadUIInstance != null && hackUploadUIInstance.activeSelf)
        {
            // UI의 위치를 적의 위치를 따라가도록 설정
            if (mainCamera != null)
            {
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(hackUploadUIPosition.position);
                if (screenPosition.z > 0)
                {
                    hackUploadUIInstance.transform.position = screenPosition;
                }
            }
        }
    }

    public void StartUpload(float uploadDuration, float hackDuration)
    {
        if (quickHackData == null || quickHackData.hackUIPrefab == null)
        {
            Debug.LogError("퀵핵 데이터 또는 UI 프리팹이 할당되지 않았습니다.");
            return;
        }

        // 퀵핵 UI 인스턴스 생성 및 초기화
        if (hackUploadUIInstance == null)
        {
            hackUploadUIInstance = Instantiate(quickHackData.hackUIPrefab, canvas.transform); // 퀵핵 데이터에서 프리팹을 가져와 캔버스를 부모로 설정
            hackUploadUIInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // 크기 조정

            // UploadTimeImage와 DurationImage 하위 오브젝트 가져오기
            Transform uploadTransform = hackUploadUIInstance.transform.Find("UploadTimeImage");
            if (uploadTransform != null)
            {
                uploadFillImage = uploadTransform.GetComponent<Image>();
            }

            Transform durationTransform = hackUploadUIInstance.transform.Find("DurationImage");
            if (durationTransform != null)
            {
                durationFillImage = durationTransform.GetComponent<Image>();
            }

            if (uploadFillImage == null || durationFillImage == null)
            {
                Debug.LogError("UploadTimeImage 또는 DurationImage 하위 오브젝트에 Fill 이미지가 없습니다.");
                return;
            }
        }

        // UploadTimeImage 활성화 전 DurationImage 비활성화
        durationFillImage.gameObject.SetActive(false);
        uploadFillImage.gameObject.SetActive(true);
        uploadFillImage.fillAmount = 0f;

        // 업로드 진행 UI 활성화하고 코루틴 시작
        hackUploadUIInstance.SetActive(true);
        StartCoroutine(UploadCoroutine(uploadDuration, hackDuration));
    }

    private IEnumerator UploadCoroutine(float uploadDuration, float hackDuration)
    {
        float elapsed = 0f;
        while (elapsed < uploadDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / uploadDuration);
            if (uploadFillImage != null)
            {
                uploadFillImage.fillAmount = progress; // 진행 바 이미지의 fillAmount를 업데이트하여 게이지가 차오르게 함
            }
            yield return null;
        }

        // 업로드 완료 후 UploadTimeImage 비활성화 및 DurationImage 활성화
        uploadFillImage.gameObject.SetActive(false);
        durationFillImage.gameObject.SetActive(true);
        durationFillImage.fillAmount = 1f;

        // 지속 시간 코루틴 시작
        StartCoroutine(DurationCoroutine(hackDuration));
    }

    private IEnumerator DurationCoroutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(1f - (elapsed / duration));
            if (durationFillImage != null)
            {
                durationFillImage.fillAmount = progress; // 지속 시간 이미지의 fillAmount를 업데이트하여 게이지가 줄어들게 함
            }
            yield return null;
        }

        // 지속 시간이 끝난 후 UI 비활성화
        if (hackUploadUIInstance != null)
        {
            hackUploadUIInstance.SetActive(false);
        }
    }
}
