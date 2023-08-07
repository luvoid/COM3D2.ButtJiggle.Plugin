using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace COM3D2.ButtJiggle
{
	[JsonObject]
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

	public static class OverrideExtensions
	{
		public static Override<T> DefaultValue<T>(T value)
		{
			return new Override<T>()
			{
				Enabled = false,
				Value = value,
			};
		}
	}
}
