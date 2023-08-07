using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace COM3D2.ButtJiggle
{
	using static OverrideExtensions;

	[JsonObject]
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
		
		[JsonIgnore] 
		public static readonly JiggleBoneOverride Default = new JiggleBoneOverride()
		{
			BlendValue       = DefaultValue(1f), 
			BlendValue2      = DefaultValue(1f), 
			Gravity          = DefaultValue(0.1f), 
			ClothedStiffness = DefaultValue(new Stiffness(0.070f, 0.27f)), 
			NakedStiffness   = DefaultValue(new Stiffness(0.055f, 0.22f)), 
			Softness         = DefaultValue(0.5f), 
			UpDown           = DefaultValue(0f), 
			Yori             = DefaultValue(0f), 
			SquashAndStretch = DefaultValue(true), 
			SideStretch      = DefaultValue(-0.2f),
			FrontStretch     = DefaultValue( 0.3f),
			EnableScaleX     = DefaultValue(false),
			LimitRotation    = DefaultValue(0.3f),
			LimitRotDecay    = DefaultValue(0.8f),

		};

		public JiggleBoneOverride(JiggleBoneOverride toCopy)
		{
			BlendValue       = toCopy.BlendValue      ; 
			BlendValue2      = toCopy.BlendValue2     ; 
			Gravity          = toCopy.Gravity         ; 
			ClothedStiffness = toCopy.ClothedStiffness; 
			NakedStiffness   = toCopy.NakedStiffness  ; 
			Softness         = toCopy.Softness        ; 
			UpDown           = toCopy.UpDown          ; 
			Yori             = toCopy.Yori            ; 
			SquashAndStretch = toCopy.SquashAndStretch; 
			SideStretch      = toCopy.SideStretch     ; 
			FrontStretch     = toCopy.FrontStretch    ; 
			EnableScaleX     = toCopy.EnableScaleX    ; 
			LimitRotation    = toCopy.LimitRotation   ;
			LimitRotDecay    = toCopy.LimitRotDecay   ;
		}

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
	}
}
