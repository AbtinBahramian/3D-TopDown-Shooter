using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class WeaponsSO : ScriptableObject
{
    public Transform prefab;
    public new string name;
    public AudioClip shootingAudio;
    public float msBetweenShots;
    public float bulletSpeed;
    public float damage;
    public float muzzleVelocity;

}
