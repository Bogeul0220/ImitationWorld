using UnityEngine;

public enum WeaponType
{
    None,
    MeleeWeapon,
    GunWeapon,
}

public class Weapon : MonoBehaviour
{
    public string weaponName;  // 무기의 이름
    public WeaponType weaponType;  // 무기 타입
    public float fireRate;  // 연사 속도
    public int damage;  // 무기의 데미지
}
