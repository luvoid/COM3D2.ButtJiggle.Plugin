using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.ButtJiggle
{
	[JsonObject]
	public struct Stiffness
	{
		public float Min;
		public float Max;

		public Stiffness(float min, float max)
		{
			Min = min;
			Max = max;
		}

		public void CopyTo(float[] floats)
		{
			if (floats.Length < 2) throw new ArgumentException();
			floats[1] = Min;
			floats[0] = Max;
		}

		public static implicit operator Stiffness(float[] floats)
		{
			if (floats.Length < 2) throw new ArgumentException();
			return new Stiffness(floats[1], floats[0]);
		}

		public static explicit operator float[](Stiffness stiffness)
		{
			float[] floats = new float[2];
			stiffness.CopyTo(floats);
			return floats;
		}

		public static explicit operator UnityEngine.Vector2(Stiffness stiffness)
		{
			return new UnityEngine.Vector2(stiffness.Min, stiffness.Max);
		}

		public static explicit operator Stiffness(UnityEngine.Vector2 vector)
		{
			return new Stiffness(vector.x, vector.y);
		}
	}

	public static class OverrideStiffnessExtensions
	{
		public static float[] AssignToIfEnabled(this Override<Stiffness> stiffnessOverride, ref float[] floats)
		{
			if (stiffnessOverride.Enabled) stiffnessOverride.Value.CopyTo(floats);
			return stiffnessOverride.Enabled ? (float[])stiffnessOverride.Value : floats;
		}
	}
}
