using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;
using UnityEngine;
using UniverseLib.UI;
using UniverseLib.Utility;

namespace COM3D2.ButtJiggle
{
	internal static class UIControlFactory
	{
		public delegate ref T RefGetter<T>();

		public static GameObject CreateControl<T>(GameObject parent, string labelText, RefGetter<T> refGet, UnityAction onSet = null, UnityEvent listen = null)
		{
			return CreateControl(
				parent,
				labelText,
				() => refGet(),
				(T value) => { refGet() = value; onSet?.Invoke(); },
				listen
			);
		}

		public static GameObject CreateControl<T>(GameObject parent, string labelText, Func<T> get, UnityAction<T> set = null, UnityEvent listen = null)
		{
			GameObject control = null;
			if (typeof(T) == typeof(bool))
			{
				if (get is not Func<bool> getBool) throw new ArgumentException();
				if (set is not UnityAction<bool> setBool) setBool = null;
				control = CreateBoolControl(parent, labelText, getBool, setBool, listen);
			}
			else if (ParseUtility.CanParse(typeof(T)))
			{
				control = CreateParsedControl(parent, labelText, get, set, listen);
			}
			else
			{
				var name = labelText.Replace(" ", "");
				var errorText = UIFactory.CreateLabel(parent, $"controlError_{name}", " " + labelText);
				errorText.text = $"{labelText} : Could not create control for type {typeof(T).Name}";
				control = errorText.gameObject;
				UIFactory.SetLayoutElement(control, minWidth: 200, minHeight: 25);
			}
			return control;
		}

		public static GameObject CreateControl<T>(GameObject parent, string labelText, RefGetter<Override<T>> refGet, UnityAction onSet = null, UnityEvent listen = null)
		{
			return CreateControl<T>(
				parent,
				labelText,
				() => refGet(),
				(Override<T> value) => { refGet() = value; onSet?.Invoke(); },
				listen
			);
		}

		public static GameObject CreateControl<T>(GameObject parent, string labelText, Func<Override<T>> get, UnityAction<Override<T>> set = null, UnityEvent listen = null)
		{
			var name = labelText.Replace(" ", "");

			var row = UIFactory.CreateHorizontalGroup(parent, $"controlOverride_{name}", false, false, true, true, childAlignment: TextAnchor.MiddleLeft);
			UIFactory.SetLayoutElement(row, minWidth: 200, minHeight: 25);
			{
				var enabledRoot = UIFactory.CreateToggle(row, $"controlBool_{name}_Enabled", out var enableToggle, out var enabledLabel);
				UIFactory.SetLayoutElement(enabledRoot, minWidth: 40, minHeight: 25, preferredWidth: 40, preferredHeight: 25);
				enabledLabel.text = " | ";
				enableToggle.isOn = get().Enabled;
				listen?.AddListener(() => enableToggle.isOn = get().Enabled);
				enableToggle.interactable = (set != null);
				if (set != null)
				{
					enableToggle.onValueChanged.AddListener((value) =>
					{
						var tempOverride = get();
						tempOverride.Enabled = value;
						set(tempOverride);
						ButtJiggle.Logger.LogDebug($"{name}.Enabled == {get().Enabled}");
					});
				}

				Func<T> valueGet = () =>
				{
					var tempOverride = get();
					return tempOverride.Value;
				};
				UnityAction<T> valueSet = (set == null) ? null : (value) =>
				{
					var tempOverride = get();
					tempOverride.Value = value;
					set(tempOverride);
					ButtJiggle.Logger.LogDebug($"{name}.Value == {get().Value}");
				};

				GameObject valueControl = null;
				if (typeof(T) == typeof(Stiffness))
				{
					if (valueGet is not Func<Stiffness> valueGetCasted) throw new ArgumentException();
					if (valueSet is not UnityAction<Stiffness> valueSetCasted) valueSetCasted = null;

					valueControl = CreateControl(row, labelText,
						get: () => (Vector2)valueGetCasted(),
						set: (value) => valueSetCasted((Stiffness)value),
						listen: listen);
				}
				else
				{
					valueControl = CreateControl(row, labelText, valueGet, valueSet, listen);
				}
			}
			return row;
		}


		private static GameObject CreateParsedControl<T>(GameObject parent, string labelText, Func<T> get, UnityAction<T> set = null, UnityEvent listen = null)
		{
			var name = labelText.Replace(" ", "");
			var row = UIFactory.CreateHorizontalGroup(parent, $"control{typeof(T).Name}_{name}", false, false, true, true, childAlignment: TextAnchor.MiddleLeft);
			UIFactory.SetLayoutElement(row, minWidth: 200, minHeight: 25);
			{
				var inputFieldRef = UIFactory.CreateInputField(row, "input", labelText);
				UIFactory.SetLayoutElement(inputFieldRef.GameObject, minWidth: 200, minHeight: 25);
				inputFieldRef.Text = ParseUtility.ToStringForInput(get(), typeof(T));
				listen?.AddListener(() => inputFieldRef.Text = ParseUtility.ToStringForInput(get(), typeof(T)));
				inputFieldRef.Component.interactable = (set != null);
				if (set != null)
				{
					inputFieldRef.OnValueChanged += (str) =>
					{
						if (ParseUtility.TryParse(str, typeof(T), out var newValue, out _))
						{
							set((T)newValue);
							ButtJiggle.Logger.LogDebug($"{name} = {get()}");
						}
						inputFieldRef.Text = ParseUtility.ToStringForInput(get(), typeof(T));
					};
				}

				UIFactory.CreateLabel(row, "label", " " + labelText);
				UIFactory.SetLayoutElement(row, minWidth: 200, minHeight: 25);
			}
			return row;
		}

		private static GameObject CreateBoolControl(GameObject parent, string labelText, Func<bool> get, UnityAction<bool> set = null, UnityEvent listen = null)
		{
			var name = "controlBool_" + labelText.Replace(" ", "");
			var toggleRoot = UIFactory.CreateToggle(parent, name, out var toggle, out var label);
			label.text = labelText;
			toggle.isOn = get();
			listen?.AddListener(() => toggle.isOn = get());
			toggle.interactable = (set != null);
			if (set != null)
			{
				toggle.onValueChanged.AddListener((value) =>
				{
					set(value);
					ButtJiggle.Logger.LogDebug($"{name} = {get()}");
				});
			}
			UIFactory.SetLayoutElement(toggleRoot, minWidth: 200, minHeight: 25);
			return toggleRoot;
		}
	}
}
