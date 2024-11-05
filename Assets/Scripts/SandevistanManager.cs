using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SandevistanManager : MonoBehaviour
{
    public static SandevistanManager instance;

    [Header("Sandevistan Settings")]
    public float sandevistanDuration = 10f;
    public Volume sandevistanVolume;
    public VolumeProfile sandevistanProfile;
    public VolumeProfile defaultProfile;
    public Material sandevistanMaterial;
    public Transform playerTransform;
    public SkinnedMeshRenderer[] skinnedMeshRenderers;
    public float meshRefreshRate = 0.1f;
    public float meshDestroyDelay = 3f;
    public Gradient trailGradient;
    public float colorLerpTime = 0.5f;

    private bool isActive = false;
    private List<GameObject> spawnedMeshes = new List<GameObject>();
    private float timeRemaining;
    private float gradientTime = 0f;
    private Color currentColor;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        skinnedMeshRenderers = playerTransform.GetComponentsInChildren<SkinnedMeshRenderer>();
        timeRemaining = sandevistanDuration;
        currentColor = trailGradient.Evaluate(0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !isActive)
        {
            Debug.Log("T key pressed, activating Sandevistan...");
            StartCoroutine(ActivateSandevistan());
        }

        if (isActive)
        {
            LerpTrailColor();
        }
    }

    private IEnumerator ActivateSandevistan()
    {
        Debug.Log("Sandevistan activated!");
        isActive = true;
        timeRemaining = sandevistanDuration;

        // Activate Post Processing and Visual Effects
        if (sandevistanVolume != null && sandevistanProfile != null)
        {
            sandevistanVolume.profile = sandevistanProfile;
            sandevistanVolume.weight = 0;

            // 초기 1초 동안 청록색이 강하게 적용되도록 설정
            float effectDuration = 1f;
            float elapsedTime = 0f;

            while (elapsedTime < effectDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                sandevistanVolume.weight = Mathf.Lerp(0, 1, elapsedTime / effectDuration);
                yield return null;
            }

            // 이후 얕은 청록색 효과 유지
            sandevistanVolume.weight = 0.5f;
        }

        // 스킬 지속 시간 동안 처리
        while (timeRemaining > 0)
        {
            timeRemaining -= meshRefreshRate;

            foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
            {
                GameObject meshCopy = CreateMeshCopy(smr);
                spawnedMeshes.Add(meshCopy);
                StartCoroutine(DestroyMeshAfterDelay(meshCopy, meshDestroyDelay));
            }

            yield return new WaitForSeconds(meshRefreshRate);
        }

        // Reset Effects
        if (sandevistanVolume != null && defaultProfile != null)
        {
            // 효과를 원래대로 되돌리기
            float fadeOutDuration = 1f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                sandevistanVolume.weight = Mathf.Lerp(0.5f, 0, elapsedTime / fadeOutDuration);
                yield return null;
            }

            sandevistanVolume.profile = defaultProfile;
            sandevistanVolume.weight = 0;
        }

        foreach (GameObject mesh in spawnedMeshes)
        {
            StartCoroutine(FadeOutAndDestroy(mesh, 1f));
        }
        spawnedMeshes.Clear();

        isActive = false;
    }

    private GameObject CreateMeshCopy(SkinnedMeshRenderer smr)
    {
        GameObject meshCopy = new GameObject("SandevistanMeshCopy");
        MeshRenderer meshRenderer = meshCopy.AddComponent<MeshRenderer>();
        MeshFilter meshFilter = meshCopy.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        smr.BakeMesh(mesh);
        meshFilter.mesh = mesh;
        meshRenderer.material = new Material(sandevistanMaterial);
        meshRenderer.material.color = currentColor;
        meshCopy.transform.SetPositionAndRotation(playerTransform.position, playerTransform.rotation);
        return meshCopy;
    }

    private void LerpTrailColor()
    {
        if (trailGradient == null) return;

        gradientTime += Time.deltaTime / colorLerpTime;
        currentColor = trailGradient.Evaluate(gradientTime % 1f);
    }

    private IEnumerator DestroyMeshAfterDelay(GameObject mesh, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(FadeOutAndDestroy(mesh, 1f));
    }

    private IEnumerator FadeOutAndDestroy(GameObject mesh, float fadeDuration)
    {
        MeshRenderer meshRenderer = mesh.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            Material mat = meshRenderer.material;
            Color initialColor = mat.color;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                mat.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
                yield return null;
            }
        }
        Destroy(mesh);
    }
}
