// #define 뭐시기

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class Health : MonoBehaviour
{
    [FormerlySerializedAs("Health")] [Header("Settings")]
    public float CurrentHealth = 100;
    public float MaxHealth = 100;

    [Header("Effects")]
    public bool BloodScreenEffect = false;
    public GameObject BloodHitParticle;

    [Header("On Death Event")]
    public UnityEvent OnDeath;

    [Header("Stats")]
    public bool IsDead;

    void Start()
    {
        LimitHealth();
        InvokeRepeating(nameof(CheckHealthState), 0, 0.5f);
    }
    private void LimitHealth()
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
    }
    public static void TakeDamage(Health health, float damage, Vector3 hitPosition = default(Vector3))
    {
        health.TakeDamage(damage, hitPosition);
    }
    public void TakeDamage(float damage, Vector3 hitPosition = default(Vector3))
    {
        CurrentHealth -= damage; // damage(float) 만큼 깜
        LimitHealth(); // 아마 초기화인듯함 마이너스를 0으로 취급하려는 것인듯함
        Invoke(nameof(CheckHealthState), 0.016f); // 헬스상태 다시 체크 

        
        #if 뭐시기
        if (BloodScreenEffect) BloodScreen.PlayerTakingDamaged(); 
        // BloodScreenEffect이면 BloodScreen 클래스에서 PalyerTakingDamaged 메서드 부름
        #endif
        if (hitPosition != Vector3.zero && BloodHitParticle != null)
        {
            GameObject fxParticle = Instantiate(BloodHitParticle, hitPosition, Quaternion.identity);
            fxParticle.hideFlags = HideFlags.HideInHierarchy;
            Destroy(fxParticle, 3);
        }
    }

    public void CheckHealthState()
    {
        LimitHealth();

        if (CurrentHealth <= 0 && IsDead == false)
        {
            CurrentHealth = 0;
            IsDead = true;

            #if 뭐시기
            //Disable all damagers0
            foreach (Damager dmg in GetComponentsInChildren<Damager>()) dmg.gameObject.SetActive(false);
            #endif
            OnDeath.Invoke();
        }

        if (CurrentHealth > 0) IsDead = false;
    }
}