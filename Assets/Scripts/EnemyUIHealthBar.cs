using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JUTPS;

namespace JUTPS.UI
{
    [AddComponentMenu("JU TPS/UI/Enemy UI Health Bar")]
    public class EnemyUIHealthBar : MonoBehaviour
    {
        [Header("Enemy UI Health Bar Settings")]
        [SerializeField] private JUHealth healthComponent;
        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform headTransform; // Transform for enemy head position
        [SerializeField] private Transform playerTransform; // Transform for player position
        [SerializeField] private float speed = 0.2f;
        [SerializeField] private float maxDistance = 50f; // Maximum distance to see the health bar
        [SerializeField] private float minScale = 0.5f; // Minimum scale for the health bar UI
        [SerializeField] private float maxScale = 1.5f; // Maximum scale for the health bar UI

        [Header("Health Bar Color Change")]
        [SerializeField] private Color emptyHPColor = Color.red;
        [SerializeField] private Color fullHPColor = Color.green;
        [SerializeField] private Color hpHealingColor = Color.cyan;
        [SerializeField] private Color hpLossColor = Color.yellow;
        [SerializeField] private bool changeHPTextColorToo = true;
        [SerializeField] private Color textColor = Color.white;
        
        private RectTransform uiTransform;
        private Camera mainCamera;
        private Image healthBarImage;
        private Text healthPointsText;
        private float oldFillAmount;

        void Start()
        {
            // Find main camera
            mainCamera = Camera.main;
            
            // Initialize the UI Transform and health component
            if (healthComponent == null)
            {
                healthComponent = GetComponent<JUHealth>();
            }

            if (uiTransform == null)
            {
                if (healthBarPrefab != null && canvas != null)
                {
                    GameObject healthBarInstance = Instantiate(healthBarPrefab, canvas.transform);
                    uiTransform = healthBarInstance.GetComponent<RectTransform>();

                    // Find specific child components by name
                    Transform healthTransform = healthBarInstance.transform.Find("Health");
                    if (healthTransform != null)
                    {
                        healthBarImage = healthTransform.GetComponent<Image>();
                    }
                    Transform textTransform = healthBarInstance.transform.Find("Text");
                    if (textTransform != null)
                    {
                        healthPointsText = textTransform.GetComponent<Text>();
                    }
                }
            }

            if (healthBarImage != null)
            {
                oldFillAmount = healthBarImage.fillAmount;
                // Set initial colors to match serialized settings
                healthBarImage.color = fullHPColor;
                if (healthPointsText != null && changeHPTextColorToo)
                {
                    healthPointsText.color = textColor;
                }
            }
        }

        void Update()
        {
            if (healthComponent == null || healthBarImage == null) return;

            // Update health bar fill amount
            float healthValueNormalized = healthComponent.Health / healthComponent.MaxHealth;
            healthBarImage.fillAmount = Mathf.MoveTowards(healthBarImage.fillAmount, healthValueNormalized, speed * Time.deltaTime);

            // Update health bar color based on health
            healthBarImage.color = Color.Lerp(emptyHPColor, fullHPColor, healthBarImage.fillAmount);

            // Update health points text
            if (healthPointsText != null)
            {
                healthPointsText.text = healthComponent.Health.ToString("000") + "/" + healthComponent.MaxHealth;
                if (changeHPTextColorToo)
                {
                    healthPointsText.color = Color.Lerp(healthBarImage.color, textColor, 0.6f);
                }
            }

            // Handle color change for healing and damage
            if (oldFillAmount != healthBarImage.fillAmount)
            {
                // Health Healing
                if (oldFillAmount < healthBarImage.fillAmount)
                {
                    healthBarImage.color = hpHealingColor;
                }
                // Health Loss
                if (oldFillAmount > healthBarImage.fillAmount)
                {
                    healthBarImage.color = hpLossColor;
                }

                oldFillAmount = healthBarImage.fillAmount;
            }

            // Update UI position to follow enemy head position
            if (mainCamera != null && headTransform != null)
            {
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(headTransform.position);
                if (screenPosition.z > 0)
                {
                    uiTransform.position = screenPosition;
                }
            }

            // Adjust health bar scale based on distance to player
            if (playerTransform != null && headTransform != null)
            {
                float distance = Vector3.Distance(playerTransform.position, headTransform.position);
                float scale = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);
                uiTransform.localScale = new Vector3(scale, scale, scale);
            }
        }
    }
}
