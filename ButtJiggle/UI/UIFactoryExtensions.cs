using System;
using UnityEngine.Events;
using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Controls;
using UniverseLib.Utility;
using UnityEngine.UI;

namespace COM3D2.ButtJiggle.UI
{
	internal static class UIFactoryExtensions
	{
		public static GameObject OverrideProperty<T>(this UIFactory create, GameObject parent, string propertyName, 
			RefGetter<Override<T>> refGet, Action onSet = null, UnityEvent listen = null)
		{
			/*
			return create.OverrideProperty(
				parent,
				labelText,
				() => refGet(),
				(Override<T> value) => { refGet() = value; onSet?.Invoke(); },
				listen
			);
			*/

			var row = create.HorizontalGroup(parent, $"Override_{propertyName}".Replace(" ", ""));

			using var _nullContext = create.LayoutContext(null);

			Property valueProperty = null;
			void onEnabledSet()
			{
				valueProperty.Control.Component.interactable = refGet().Enabled;
				onSet?.Invoke();
			}
			ref bool refEnabledGet()
			{
				ref bool enabled = ref refGet().Enabled;
				valueProperty.Control.Component.interactable = enabled;
				return ref enabled;
			}

			BoolControl enabledControl = create.BoolControl(row, $"{propertyName}.{nameof(Override<T>.Enabled)}", refEnabledGet, onEnabledSet, listen);

			//var dividerLabel = create.Label(row, "Divider", " | ");

			if (refGet is RefGetter<Override<string>> refGetOverrideString)
			{
				valueProperty = create.StringProperty(row, propertyName, () => ref refGetOverrideString().Value, onSet, listen);
			}
			else if (refGet is RefGetter<Override<bool>> refGetOverrideBool)
			{
				valueProperty = create.BoolProperty(row, propertyName, () => ref refGetOverrideBool().Value, onSet, listen);
			}
			else if (refGet is RefGetter<Override<Stiffness>> refGetOverrideStiffness)
			{
				valueProperty = create.ParsedProperty<Stiffness, StiffnessParser>(row, propertyName,
					() => ref refGetOverrideStiffness().Value,
					onSet,
					listen
				);
			}
			else
			{
				valueProperty = create.ParsedProperty(row, propertyName, () => ref refGet().Value, onSet, listen);
			}

			UIFactory.SetLayoutElement(valueProperty.GameObject, flexibleWidth: 1);
			return row;
		}

		private class StiffnessParser : Parser<Stiffness>
		{
			public override string ToStringForInput(Stiffness obj, Type type)
			{
				return ParseUtility.ToStringForInput<Vector2>((Vector2)obj);
			}

			public override bool TryParse(string input, Type type, out Stiffness obj, out Exception parseException)
			{
				bool result = ParseUtility.TryParse(input, out Vector2 vector2, out parseException);
				obj = (Stiffness)vector2;
				return result;
			}

			public override string GetExampleInput(Type type)
			{
				if (type == typeof(Stiffness))
				{
					return "Unclothed Clothed";
				}
				else
				{
					return base.GetExampleInput(type);
				}
			}
		}

		/*
		public static GameObject OverrideProperty<T>(this UIFactory create, GameObject parent, string propertyName, Getter<Override<T>> get, Setter<Override<T>> set = null, UnityEvent listen = null)
		{
			var row = CreateHorizontalGroup(parent, propertyName, false, false, true, true, childAlignment: TextAnchor.MiddleLeft);
			{
				var enabledToggle = create.Toggle(row, $"{propertyName}.{nameof(Override<T>.Enabled)}");
				
				enabledLabel.text = " | ";
				enableToggle.isOn = get().Enabled;
				listen?.AddListener(() => enableToggle.isOn = get().Enabled);
				enableToggle.interactable = (set != null);
			}
			return row;
		}
		*/
	}
}
