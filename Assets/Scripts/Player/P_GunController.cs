using UnityEngine;

public class P_GunController : MonoBehaviour
{
    [SerializeField]
    private Gun currentGun; // 현재 장착된 총

    [SerializeField]
    private float currentFireRate; // 현재 총의 연사 속도

    void Update()
    {
        GunFireRateCalc();
        TryFire();
    }

    private void GunFireRateCalc()
    {
        // 연사 속도 계산
        if (currentGun.fireRate > 0)
            currentFireRate -= Time.deltaTime;
    }

    private void TryFire()
    {
        // 총을 발사하는 로직
        if (InputManager.Instance.FireHeld && currentFireRate <= 0)
        {
            
            Fire();
        }
    }

    private void Fire()
    {
        currentFireRate = currentGun.fireRate;
        Shoot();
    }

    private void Shoot()
    {
        // 총을 발사하는 로직
        // 총알 발사, 발사 소리 재생, 총구 화염 재생 등
        Debug.Log("Fire! " + currentGun.weaponName);
        currentGun.muzzleFlash.Play(); // 총구 화염 재생
        AudioSource.PlayClipAtPoint(currentGun.fireSound, transform.position); // 발사 소리 재생
    }
}
