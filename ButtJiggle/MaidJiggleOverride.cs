using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace COM3D2.ButtJiggle
{
	using static OverrideExtensions;

	[JsonObject]
	public struct MaidJiggleOverride
	{
		public enum Slot
		{
			Hip,
			Pelvis,
		}

		public JiggleBoneOverride HipOverride;
		public JiggleBoneOverride PelvisOverride;

		[JsonIgnore]
		public static readonly MaidJiggleOverride Default = new MaidJiggleOverride()
		{
			HipOverride = JiggleBoneOverride.Default,
			PelvisOverride = new JiggleBoneOverride(JiggleBoneOverride.Default) {
				Softness = DefaultValue(0.475f),
			},
		};
		
		[JsonIgnore]
		public JiggleBoneOverride this[Slot slot]
		{
			get => slot switch
			{
				Slot.Hip => HipOverride,
				Slot.Pelvis => PelvisOverride,
				_ => throw new ArgumentOutOfRangeException(nameof(slot)),
			};
			set
			{
				switch (slot)
				{
					case Slot.Hip:
						HipOverride = value;
						break;
					case Slot.Pelvis:
						PelvisOverride = value;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(slot));
				}
			}
		}
	}
}
