using System;
using UnityEngine;

public class SmokeGrenadeSettings : GrenadeSettings
{
    public void OnValidate()
    {
        if (this._emissionArea != null)
        {
            this._initialRadius = this._emissionArea.radius;
            this._pivot = this._emissionArea.transform.localPosition;
        }
    }

    [SerializeField]
    public SphereCollider _emissionArea;

    [SerializeField]
    public AnimationCurve _sizeOverTime;

    [SerializeField]
    public float _initialRadius;

    [SerializeField]
    public float _radiusMultiplier = 1f;

    [SerializeField]
    public Vector3 _pivot;

    [SerializeField]
    public Vector3 _torque;

    [SerializeField]
    public float _torqueDelta = 0.3f;

    [SerializeField]
    public float _areaStartPosNorm = 0.5f;
}
