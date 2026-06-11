using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

// Token: 0x0200079C RID: 1948
public class CustomItem : MonoBehaviour
{
	// Token: 0x040034C8 RID: 13512
	[SerializeField]
	private MeshFilter _meshFilter;

	// Token: 0x040034C9 RID: 13513
	[SerializeField]
	private CustomItemData _data;

	// Token: 0x040034CA RID: 13514
	[SerializeField]
	private EClippingCustoms _itemType;
}
