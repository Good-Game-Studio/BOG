using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu]
public class WeaponManager : ScriptableObject
{
    public float fireRate;
    public float reloadingTime;
    public float bulletSpeed;
    public float knockBackValue;
    public float damage;
    public int totalBullet;
    public int bulletPerMag;
    public float explosionRadius;
    public float explosionForce;
    public float upwordDisplace;
    public float weaponOffset = 1;
    public bool isAutomatitc;
    public bool oneTimeUse;
    public float bulletLifetime;
}
