using System.Collections;
using Cinemachine;
using StarterAssets;
using UnityEngine;

public class P_CombatController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public CinemachineVirtualCamera virtualCamera;
    [SerializeField]
    private bool prevZoomInHeld;
    private Coroutine cameraLerpCoroutine;

    private Weapon currentWeapon;
    public Weapon CurrentWeapon
    {
        get { return currentWeapon; }
        set
        {
            switch (value.weaponType)
            {
                case WeaponType.GunWeapon:
                    currentWeapon = value;
                    break;
                case WeaponType.MeleeWeapon:
                    currentWeapon = value;
                    break;
                default:
                    currentWeapon = null;
                    break;
            }
        }
    }

    [SerializeField]
    private float currentFireRate;

    private bool prevFireHeld;

    void Update()
    {
        bool fireHeld = InputManager.Instance.FireHeld;

        //FireRateCalc();
        //TryFire();

        if (fireHeld != prevFireHeld)
        {
            if (InputManager.Instance.FireClicked)
            {
                // FireClicked가 true일 때만 애니메이션 트리거를 설정
                animator.SetTrigger("Fire");
            }
            animator.SetBool("FireHeld", fireHeld);
            prevFireHeld = fireHeld;
        }

        CamZoomIn();
    }

    private IEnumerator LockOnLerpCoroutine(Cinemachine3rdPersonFollow tPerson,
        Vector3 fromOffset, float fromDistance,
        Vector3 toOffset, float toDistance,
        float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            tPerson.ShoulderOffset = Vector3.Lerp(fromOffset, toOffset, t);
            tPerson.CameraDistance = Mathf.Lerp(fromDistance, toDistance, t);

            yield return null;
        }

        tPerson.ShoulderOffset = toOffset;
        tPerson.CameraDistance = toDistance;
    }

    private void CamZoomIn()
    {
        bool currentZoomInHeld = InputManager.Instance.ZoomInHeld;
        
        if (currentZoomInHeld != prevZoomInHeld)
        {
            if (cameraLerpCoroutine != null)
            {
                StopCoroutine(cameraLerpCoroutine);
                cameraLerpCoroutine = null;
            }

            if (virtualCamera != null && virtualCamera.TryGetComponent<Cinemachine3rdPersonFollow>(out var tPerson))
            {
                if (currentZoomInHeld)
                {
                    // ZoomInHeld true일 때 카메라 줌 인
                    cameraLerpCoroutine = StartCoroutine(LockOnLerpCoroutine(tPerson,
                        tPerson.ShoulderOffset, tPerson.CameraDistance,
                        new Vector3(2.0f, 0.2f, 0.1f), 1.2f, 0.5f));
                }
                else
                {
                    // ZoomInHeld false일 때 카메라 줌 아웃
                    cameraLerpCoroutine = StartCoroutine(LockOnLerpCoroutine(tPerson,
                        tPerson.ShoulderOffset, tPerson.CameraDistance,
                        new Vector3(1.0f, 0.0f, 0.0f), 4f, 0.5f));
                }
            }
            prevZoomInHeld = currentZoomInHeld;
        }
    }

    private void FireRateCalc()
    {
        // 연사 속도 계산
        if (currentWeapon == null)
        {

        }

        if (currentWeapon.fireRate > 0)
            currentFireRate -= Time.deltaTime;
    }

    private void TryFire()
    {
        // 총을 발사하는 로직
        if (InputManager.Instance.FireHeld && currentFireRate <= 0)
        {
            animator.SetTrigger("Fire");
            Fire();
        }
    }

    private void Fire()
    {
        currentFireRate = currentWeapon.fireRate;
        if (currentWeapon.weaponType == WeaponType.GunWeapon)
        {
            Shoot(currentWeapon as Gun);
        }
        else
        {
            // 근접 무기 발사 로직
            Debug.Log("Melee Attack!");
        }
    }

    private void Shoot(Gun currentGun)
    {
        // 총을 발사하는 로직
        // 총알 발사, 발사 소리 재생, 총구 화염 재생 등
        Debug.Log("Fire! " + currentGun.weaponName);
        currentGun.muzzleFlash.Play(); // 총구 화염 재생
        AudioSource.PlayClipAtPoint(currentGun.fireSound, transform.position); // 발사 소리 재생
    }

    private void SetWeapon(Weapon weapon)
    {

    }
}
