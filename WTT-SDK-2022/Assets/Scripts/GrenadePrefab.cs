using System;
using EFT.PrefabSettings;
using UnityEngine;

[DisallowMultipleComponent]
public class GrenadePrefab : WeaponPrefab
{

    [Header("Element 0 defines grenade initial position!")]
    public string[] ThrowingParts;

    public GrenadeSettings GrenadeItself;

    public TripwireVisual TripwireItself;
}