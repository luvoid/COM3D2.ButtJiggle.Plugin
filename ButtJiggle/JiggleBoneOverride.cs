using System;

namespace COM3D2.ButtJiggle
{
	public struct JiggleBoneOverride
	{
		public Override<float    > BlendValue      ;
		public Override<float    > BlendValue2     ;

		public Override<float    > Gravity         ;
		public Override<Stiffness> ClothedStiffness;
		public Override<Stiffness> NakedStiffness  ;
		public Override<float    > Softness        ;
		public Override<float    > UpDown          ;
		public Override<float    > Yori            ;

		public Override<bool     > SquashAndStretch;
		public Override<float    > SideStretch     ;
		public Override<float    > FrontStretch    ;
		public Override<bool     > EnableScaleX    ;

		public Override<float    > LimitRotation   ;
		public Override<float    > LimitRotDecay   ;
		
		public static JiggleBoneOverride None = new JiggleBoneOverride()
		{
			BlendValue       = DefaultValue(1f), 
			BlendValue2      = DefaultValue(1f), 
			Gravity          = DefaultValue(0.1f), 
			ClothedStiffness = DefaultValue(new Stiffness(0.070f, 0.27f)), 
			NakedStiffness   = DefaultValue(new Stiffness(0.055f, 0.22f)), 
			Softness         = DefaultValue(0.5f), 
			UpDown           = DefaultValue(30f), 
			Yori             = DefaultValue(0f), 
			SquashAndStretch = DefaultValue(true), 
			SideStretch      = DefaultValue(-0.2f),
			FrontStretch     = DefaultValue( 0.3f),
			EnableScaleX     = DefaultValue(false),
			LimitRotation    = DefaultValue(0.3f),
			LimitRotDecay    = DefaultValue(0.8f),

		};

		public static JiggleBoneOverride From(jiggleBone jb)
		{
			JiggleBoneOverride jbOverride = new();
			jbOverride.BlendValue       = jb.BlendValue      ;
			jbOverride.BlendValue2      = jb.BlendValue2     ;
			jbOverride.Gravity          = jb.bGravity        ;
			jbOverride.ClothedStiffness = (Stiffness)jb.bStiffnessBRA;
			jbOverride.NakedStiffness   = (Stiffness)jb.bStiffness   ;
			jbOverride.Softness         = jb.m_fMuneYawaraka ;
			jbOverride.UpDown           = jb.MuneUpDown      ;
			jbOverride.Yori             = jb.MuneYori        ;
			jbOverride.SquashAndStretch = jb.SquashAndStretch;
			jbOverride.SideStretch      = jb.sideStretch     ;
			jbOverride.FrontStretch     = jb.frontStretch    ;
			jbOverride.EnableScaleX     = jb.m_bEnableScaleX ;
			jbOverride.LimitRotation    = jb.m_fLimitRot     ;
			jbOverride.LimitRotDecay    = jb.m_fLimitRotDecay;
			return jbOverride;
		}

		public void ApplyTo(jiggleBone jb)
		{
			jb.BlendValue       = this.BlendValue      .AssignToIfEnabled(ref jb.BlendValue      );
			jb.BlendValue2      = this.BlendValue2     .AssignToIfEnabled(ref jb.BlendValue2     );
			jb.bGravity         = this.Gravity         .AssignToIfEnabled(ref jb.bGravity        );
			jb.bStiffnessBRA    = this.ClothedStiffness.AssignToIfEnabled(ref jb.bStiffnessBRA   );
			jb.bStiffness       = this.NakedStiffness  .AssignToIfEnabled(ref jb.bStiffness      );
			jb.m_fMuneYawaraka  = this.Softness        .AssignToIfEnabled(ref jb.m_fMuneYawaraka );
			jb.MuneUpDown       = this.UpDown          .AssignToIfEnabled(ref jb.MuneUpDown      );
			jb.MuneYori         = this.Yori            .AssignToIfEnabled(ref jb.MuneYori        );
			jb.sideStretch      = this.SideStretch     .AssignToIfEnabled(ref jb.sideStretch     );
			jb.frontStretch     = this.FrontStretch    .AssignToIfEnabled(ref jb.frontStretch    );
			jb.m_bEnableScaleX  = this.EnableScaleX    .AssignToIfEnabled(ref jb.m_bEnableScaleX );
			jb.m_fLimitRot      = this.LimitRotation   .AssignToIfEnabled(ref jb.m_fLimitRot     );
			jb.m_fLimitRotDecay = this.LimitRotDecay   .AssignToIfEnabled(ref jb.m_fLimitRotDecay);
		}


		public static Override<T> DefaultValue<T>(T value)
		{
			return new Override<T>()
			{
				Enabled = false,
				Value = value,
			};
		}
	}

	public struct Override<T>
	{
		public bool Enabled;
		public T Value;

		public T AssignToIfEnabled(ref T field)
		{
			if (Enabled) field = Value;
			return Enabled ? Value : field;
		}

		public static implicit operator Override<T>(T value)
		{
			return new Override<T>()
			{
				Enabled = true,
				Value = value,
			};
		}
	}

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
	public static class StiffnessExtensions
	{
		public static float[] AssignToIfEnabled(this Override<Stiffness> stiffnessOverride, ref float[] floats)
		{
			if (stiffnessOverride.Enabled) stiffnessOverride.Value.CopyTo(floats);
			return stiffnessOverride.Enabled ? (float[])stiffnessOverride.Value : floats;
		}
	}
}
