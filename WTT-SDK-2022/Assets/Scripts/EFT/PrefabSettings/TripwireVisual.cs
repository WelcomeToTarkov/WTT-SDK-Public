
using System;
using UnityEngine;

namespace EFT.PrefabSettings
{
  public class TripwireVisual : MonoBehaviour
  {
    [SerializeField]
    private Transform _pivotPosition;
    [SerializeField]
    private GameObject _grenadeModel;

    public Transform PivotPosition => this._pivotPosition;

    public GameObject GrenadeModel => this._grenadeModel;
  }
}
