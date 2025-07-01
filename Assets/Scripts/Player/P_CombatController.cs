using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;

public class P_CombatController : MonoBehaviour
{
    [Header("애니메이션")]
    [SerializeField]
    private Animator animator;

    [Header("카메라")]
    public CinemachineVirtualCamera virtualCamera;
    [SerializeField] private bool prevZoomInHeld;
    private Coroutine cameraLerpCoroutine;

    [Header("무기")]
    public Weapon[] Hands;
    public Weapon CurrentWeapon;
    [SerializeField] private float currentFireRate;
    private bool prevFireHeld;
    public GameObject WeaponParent;
    private bool prevThrowBallHeld;
    private bool prevSpawnAllyHeld;
    public GameObject BallParent;
    public BallObject[] BallObjects;
    public SpawnAllyObject SpawnAllyObject;

    [Header("상태")]
    public UnitStats PlayerStat;
    public bool IsDied;
    public bool InBattle;

    void Start()
    {
        if (PlayerStat == null)
            PlayerStat = GetComponent<UnitStats>();

        PlayerStat.Init();
        GetComponent<Damageable>().InitDamageable(PlayerStat);
        PlayerStat.OnDamaged += StartBattle;
        InputManager.Instance.CallInAllyPressed += CallInAlly;
    }

    void Update()
    {
        bool fireHeld = InputManager.Instance.FireHeld;
        bool throwBallHeld = InputManager.Instance.ThrowBallHeld;
        bool spawnAllyHeld = InputManager.Instance.SpawnAllyHeld;
        animator.SetBool("HaveMelee", PlayerManager.Instance.WeaponEquiped);

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

        if (throwBallHeld != prevThrowBallHeld)
        {
            if (throwBallHeld == true)
            {
                WeaponParent.SetActive(false);
                BallParent.SetActive(true);
            }
            animator.SetBool("ThrowHeld", throwBallHeld);

            for (int i = 0; i < BallParent.transform.childCount; i++)
            {
                if (i == InputManager.Instance.SelectedBallIndex)
                    BallParent.transform.GetChild(i).gameObject.SetActive(true);
                else
                    BallParent.transform.GetChild(i).gameObject.SetActive(false);
            }

            SpawnAllyObject.gameObject.SetActive(false);

            prevThrowBallHeld = throwBallHeld;
        }

        if (spawnAllyHeld != prevSpawnAllyHeld)
        {
            if (spawnAllyHeld == true)
            {
                WeaponParent.SetActive(false);
                BallParent.SetActive(true);
            }
            animator.SetBool("SpawnAllyHeld", spawnAllyHeld);

            for (int i = 0; i < BallParent.transform.childCount; i++)
                BallParent.transform.GetChild(i).gameObject.SetActive(false);

            BallParent.transform.GetChild(BallParent.transform.childCount - 1).gameObject.SetActive(true);
            SpawnAllyObject.gameObject.SetActive(true);

            prevSpawnAllyHeld = spawnAllyHeld;
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
        if (CurrentWeapon == null)
        {

        }

        if (CurrentWeapon.fireRate > 0)
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
        currentFireRate = CurrentWeapon.fireRate;
        if (CurrentWeapon.weaponType == WeaponType.GunWeapon)
        {
            Shoot(CurrentWeapon as Gun);
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

    public void EnableHandCollider()
    {
        if (CurrentWeapon == null)
        {
            foreach (MeleeWeapon hand in Hands)
                hand.StartAttack();
        }
        else
        {
            (CurrentWeapon as MeleeWeapon).StartAttack();
        }
    }

    public void DisableHandCollider()
    {
        if (CurrentWeapon == null)
        {
            foreach (MeleeWeapon hand in Hands)
                hand.EndAttack();
        }
        else
        {
            (CurrentWeapon as MeleeWeapon).EndAttack();
        }
    }

    // 공격 애니메이션 실행 시 무기 Collider를 켜고 꺼서 공격 로직 실행
    public void EnableMeleeWeaponCollider()
    {
        if (CurrentWeapon == null) return;

        (CurrentWeapon as MeleeWeapon).weaponCollider.enabled = true;
    }

    public void DisableMeleeWeaponCollider()
    {
        if (CurrentWeapon == null) return;

        (CurrentWeapon as MeleeWeapon).weaponCollider.enabled = false;
    }

    public void ThrowBallPooling()
    {
        WeaponObjectActive();
        InventoryManager.Instance.RemoveItem(InventoryManager.Instance.CurrentBallCheck[InputManager.Instance.SelectedBallIndex]);
        var setBall = ObjectPoolManager.Get<BallObject>(BallObjects[InputManager.Instance.SelectedBallIndex].gameObject);
        setBall.transform.position = BallParent.transform.position;
        setBall.Init(Camera.main.transform);
    }

    public void SpawnAlly()
    {
        WeaponObjectActive();
        var setSpawnAlly = ObjectPoolManager.Get<SpawnAllyObject>(SpawnAllyObject.gameObject);
        setSpawnAlly.transform.position = BallParent.transform.position;
        setSpawnAlly.Init(Camera.main.transform);
    }

    public void CallInAlly()
    {
        WeaponObjectActive();
        animator.SetTrigger("CallInAlly");
    }

    public void WeaponObjectActive()
    {
        WeaponParent.SetActive(true);
        BallParent.SetActive(false);
    }

    public void StartBattle() => InBattle = true;

    public void EndBattle() => InBattle = false;
}
