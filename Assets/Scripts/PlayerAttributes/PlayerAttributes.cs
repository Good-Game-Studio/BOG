using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerAttributes : ScriptableObject
{
    public string className;
    [Range (10, 100)]
    public float health;
    [Range(1, 100)]
    public float speed;
    [Range(1, 100)]
    public float jumpHeight;
    [Range(1, 100)]
    public float accuracy;
    [Range(1, 100)]
    public float knockbackResistance;
    [Range(1, 100)]
    public float reloadSpeed;
    [Range(10, 100)]
    public float size;
    [Range(0, 100)]
    public float armor;
    [Range(10, 100)]
    public float stamina;
    [Range(0, 100)]
    public float healthRecovery;
    [Range(1, 100)]
    public float meleeStrength;
    [Range(1, 100)]
    public float specialChargeRate;
    [Range(10, 100)]
    public float dodgeDistance;
    [Range(10, 100)]
    public float meleeRange;
    [Range(1, 25)]
    public float critChance;
    [Range(125, 200)]
    public float critDamage;
    [Range(0, 25)]
    public float evadeChance;
}
