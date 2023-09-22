using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class WeaponManager : ScriptableObject
{
    public string weaponName;
    public string weaponType;
    public float fireRate;
    public float bulletSpeed;
    public float knockBackValue;
    public GameObject weaponPrefab;
    public int bulletCount;
    public float explosionRadius;
    public float explosionForce;
    public float upwordDisplace;
    public float weaponOffset = 1;
    //  public GameObject[] weaponskin;
}
