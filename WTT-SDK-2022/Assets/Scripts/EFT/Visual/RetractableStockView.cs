
using EFT.InventoryLogic;
using System;
using UnityEngine;

namespace EFT.Visual
{
  public class RetractableStockView : MonoBehaviour
  {
    [SerializeField]
    private RetractableStockView.BonePosition[] _bonePositions;
    private FoldableComponent foldableComponent_0;
    private Action action_0;


    [Serializable]
    public class BonePosition
    {
      public string BoneName;
      public Transform Bone;
      public Vector3 RetractedPosition;
      public Vector3 NormalPosition;
    }
  }
}
