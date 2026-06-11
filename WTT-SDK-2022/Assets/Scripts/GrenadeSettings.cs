using System;
using UnityEngine;

public class GrenadeSettings : ThrowableSettings
{
	public string EmmisionEffect
	{
		get
		{
			return base.name.Replace("(Clone)", "");
		}
	}

	public GrenadeSettings.CollisionSounds CollisionSound;

	public Transform Skoba;

	public enum CollisionSounds
	{
		frag,
		smoke,
		stun,
		smokeM18,
		stunM7920
	}
}
