using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiFlare
{
	[Serializable]
	public class Flare
	{
		public static string ScalePropertyName
		{
			get
			{
				return "_scale";
			}
		}

		public static string AlphaPropertyName
		{
			get
			{
				return "_alpha";
			}
		}

		public static string MinDistPropertyName
		{
			get
			{
				return "_minDist";
			}
		}

		public static string MaxDistPropertyName
		{
			get
			{
				return "_maxDist";
			}
		}

		public static string MinScalePropertyName
		{
			get
			{
				return "_minScale";
			}
		}

		public static string MaxScalePropertyName
		{
			get
			{
				return "_maxScale";
			}
		}

		public static string MinAlphaPropertyName
		{
			get
			{
				return "_minAlpha";
			}
		}

		public static string MaxAlphaPropertyName
		{
			get
			{
				return "_maxAlpha";
			}
		}

		public static string ColorPropertyName
		{
			get
			{
				return "_color";
			}
		}

		public static string TexIdPropertyName
		{
			get
			{
				return "_texId";
			}
		}

		public static string FlareTypePropertyName
		{
			get
			{
				return "_flareType";
			}
		}

		public Vector2 Scale
		{
			get
			{
				return this._scale;
			}
		}

		public float Alpha
		{
			get
			{
				return this._alpha;
			}
		}

		public float MinDist
		{
			get
			{
				return this._minDist;
			}
		}

		public float MaxDist
		{
			get
			{
				return this._maxDist;
			}
		}

		public float MinScale
		{
			get
			{
				return this._minScale;
			}
		}

		public float MaxScale
		{
			get
			{
				return this._maxScale;
			}
		}

		public float MinAlpha
		{
			get
			{
				return this._minAlpha;
			}
		}

		public float MaxAlpha
		{
			get
			{
				return this._maxAlpha;
			}
		}

		public Color Color
		{
			get
			{
				return this._color;
			}
		}

		public FlareType FlareType
		{
			get
			{
				return this._flareType;
			}
		}

		public int TexId
		{
			get
			{
				return this._texId;
			}
		}

		public Flare()
		{
		}

		[Tooltip("Размер выбранной флары - ширина и высота")]
		[SerializeField]
		[FormerlySerializedAs("Scale")]
		public Vector2 _scale;

		[Tooltip("Прозрачность выбранной флары")]
		[SerializeField]
		[FormerlySerializedAs("Alpha")]
		[Range(0f, 1f)]
		public float _alpha;

		[Tooltip("Цвет выбранной флары")]
		[SerializeField]
		[FormerlySerializedAs("Color")]
		public Color _color;

		[Tooltip("Тип выбранной флары. В зависимости от типа, флары могут иметь специфическую логику отображения или материал. Общие настройки каждого типа можно найти в FlareSceneSettings внутри скриптовой сцены. Более опдробно о типах флар можно узнать из документации")]
		[SerializeField]
		[FormerlySerializedAs("Type")]
		public FlareType _flareType;

		[Tooltip("Расстояние, которое определяет размер и прозрачность флары. Когда расстояние от камеры до флары больше или равно End, ее размер(Scale) будет умножен на MaxScale. Когда расстояние от камеры до флары меньше или равно Start, ее размер(Scale) будет умножен на MinScale. Промежуточные значения будут линейно интерполированные между этими граничными значениями. Аналогично будет вычислена прозрачность.")]
		[SerializeField]
		[FormerlySerializedAs("MinDist")]
		public float _minDist;

		[SerializeField]
		[FormerlySerializedAs("MaxDist")]
		public float _maxDist;

		[Tooltip("Диапазон изменения размера флары в зависимости от знвчения DistanceRange")]
		[SerializeField]
		[FormerlySerializedAs("MinScale")]
		public float _minScale;

		[SerializeField]
		[FormerlySerializedAs("MaxScale")]
		public float _maxScale;

		[Tooltip("Диапазон изменения прозрачности флары в зависимости от знвчения DistanceRange")]
		[SerializeField]
		[FormerlySerializedAs("MinAlpha")]
		public float _minAlpha;

		[SerializeField]
		[FormerlySerializedAs("MaxAlpha")]
		public float _maxAlpha;

		[Tooltip("Текстура флары. Все текстуры собранны в атлас. Значения атласа можно найти в FlareSceneSettings")]
		[SerializeField]
		[FormerlySerializedAs("TextureId")]
		public int _texId;
	}
}
