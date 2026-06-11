using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiFlare
{
	[ExecuteInEditMode]
	[HelpURL("https://eodproject.atlassian.net/wiki/spaces/FRONT/pages/1254981646/Unity.#Flare")]
	public class FlareLight : MonoBehaviour
	{
		public static string ScalePropertyName
		{
			get
			{
				return "_totalScale";
			}
		}

		public static string AlphaPropertyName
		{
			get
			{
				return "_totalAlpha";
			}
		}

		public static string FlaresPropertyName
		{
			get
			{
				return "_flares";
			}
		}

		public IReadOnlyList<Flare> Flares
		{
			get
			{
				return this._flares;
			}
		}

		public float Alpha
		{
			get
			{
				return this._totalAlpha;
			}
		}

		public float Scale
		{
			get
			{
				return this._totalScale;
			}
		}

		public bool Enabled
		{
			get
			{
				return base.isActiveAndEnabled;
			}
		}

		public void OnDestroy()
		{

			this.bool_0 = true;
		}

		public void SetAlpha(float value)
		{
			this._totalAlpha = value;
		}

		public void SetScale(float value)
		{
			this._totalScale = value;
		}

		public void OnEnable()
		{

		}

		public void OnDisable()
		{

		}

		public void OnDrawGizmos()
		{
			Gizmos.DrawIcon(base.transform.position, "LensFlare Gizmo");
		}

		public FlareLight()
		{
		}

		private bool bool_0;

		[Tooltip("The common size factor for all flares in this FlareLight")]
		[FormerlySerializedAs("_scale")]
		[FormerlySerializedAs("Scale")]
		[SerializeField]
		private float _totalScale = 1f;

		[Tooltip("The combined transparency factor of all flares for this FlareLight")]
		[FormerlySerializedAs("_alpha")]
		[FormerlySerializedAs("Alpha")]
		[SerializeField]
		[Range(0f, 1f)]
		private float _totalAlpha = 1f;

		[SerializeField]
		[FormerlySerializedAs("Flares")]
		private Flare[] _flares;
	}
}
