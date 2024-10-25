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
    public Color[] trailColors;
    public float colorLerpTime = 0.5f;
    
    private bool isActive = false;
    private List<GameObject> spawnedMeshes = new List<GameObject>();
    private float timeRemaining;
    private int colorIndex = 0;
    private float lerpProgress = 0f;
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
        currentColor = trailColors[0];
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
            sandevistanVolume.weight = 1;
        }

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
        if (trailColors.Length == 0) return;

        lerpProgress += Time.deltaTime * colorLerpTime;
        currentColor = Color.Lerp(currentColor, trailColors[colorIndex], lerpProgress);

        if (lerpProgress >= 1f)
        {
            lerpProgress = 0f;
            colorIndex = (colorIndex + 1) % trailColors.Length;
        }
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