using UnityEngine;
using UnityEngine.UI;
using System.Collections; // IEnumerator를 사용하기 위해 추가

public class HackUploadUI : MonoBehaviour
{
    [SerializeField] private GameObject hackUploadUIPrefab; // 퀵핵 UI 프리팹 (정사각형 모양)
    [SerializeField] private Vector3 offset = new Vector3(0, 2.0f, 0); // 적의 위치에서 오프셋 (적 근처 적당한 거리)
    private Transform enemyTransform; // 적의 Transform 참조
    private Camera mainCamera;
    private GameObject hackUploadUIInstance; // 퀵핵 UI 인스턴스
    private Image fillImage; // 업로드 진행을 나타내는 Fill 이미지

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

        // 퀵핵 UI 인스턴스 생성 및 초기 비활성화
        hackUploadUIInstance = Instantiate(hackUploadUIPrefab);
        hackUploadUIInstance.transform.SetParent(enemyTransform); // 적을 부모로 설정하여 적의 움직임에 따라 이동
        hackUploadUIInstance.transform.localPosition = offset; // 적의 머리 위에 위치하도록 설정
        hackUploadUIInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f); // 크기 조정
        hackUploadUIInstance.SetActive(false);

        // 진행 바 Fill 이미지 가져오기
        Transform fillTransform = hackUploadUIInstance.transform.Find("Fill Area/Fill");
        if (fillTransform != null)
        {
            fillImage = fillTransform.GetComponent<Image>();
        }

        if (fillImage == null)
        {
            Debug.LogError("Fill 이미지가 hackUploadUI에 없습니다.");
        }
    }

    private void Update()
    {
        if (enemyTransform != null && hackUploadUIInstance.activeSelf)
        {
            // UI가 항상 카메라를 향하도록 회전 설정
            if (mainCamera != null)
            {
                Vector3 direction = mainCamera.transform.position - hackUploadUIInstance.transform.position;
                direction.y = 0; // Y축 회전을 없애서 UI가 카메라와 평행하게 보이도록 함
                hackUploadUIInstance.transform.rotation = Quaternion.LookRotation(-direction);
            }
        }
    }

    public void StartUpload(float uploadDuration)
    {
        // 업로드 진행 UI 활성화하고 코루틴 시작
        hackUploadUIInstance.SetActive(true);
        StartCoroutine(UploadCoroutine(uploadDuration));
    }

    private IEnumerator UploadCoroutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            if (fillImage != null)
            {
                fillImage.fillAmount = progress; // 진행 바 이미지의 fillAmount를 업데이트하여 게이지가 차오르게 함
            }
            yield return null;
        }

        // 업로드 완료 후 UI 비활성화
        hackUploadUIInstance.SetActive(false);
    }
}
