using BepInEx;
using BepInEx.Logging;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Panels;
using UniverseLib.Utility;

namespace COM3D2.ButtJiggle
{
	public sealed partial class ButtJiggle : BaseUnityPlugin
	{
		public static UIBase UIBase => Instance?.m_UIBase;
		private UIBase m_UIBase;
		private PluginPanel m_PluginPanel;

		void OnUIStart()
		{
			// Create a UIBase and specify update callback
			m_UIBase = UniversalUI.RegisterUI(PluginInfo.PLUGIN_GUID, OnUIUpdate);
			
			// Is the panel displayed by default?
			m_UIBase.Enabled = false;

			// Create the main panel
			m_PluginPanel = new PluginPanel(m_UIBase);
		}

		void OnUIUpdate()
		{
			// Allows the UI to be visible in karaoke & VR mode
			m_UIBase.Canvas.worldCamera = null;

			// Usually nothing else is needed
			// Don't create UI elements here, use PluginPanel instead
		}

		internal void EnableUI()
		{
			if (m_UIBase == null) return;
			m_UIBase.Enabled = true;
		}

		internal void DisableUI()
		{
			if (m_UIBase == null) return;
			m_UIBase.Enabled = false;
		}

		internal void ToggleUI()
		{
			if (m_UIBase == null) return;

			if (m_UIBase.Enabled)
			{
				DisableUI();
			}
			else
			{
				EnableUI();
			}
		}

		#region Universe Helpers
		private void Universe_Init()
		{
			float startupDelay = 1f;
			UniverseLib.Config.UniverseLibConfig config = new()
			{
				Force_Unlock_Mouse = true,
				Allow_UI_Selection_Outside_UIBase = false,
				Disable_EventSystem_Override = false,
				Disable_Fallback_EventSystem_Search = false,
			};
			Universe.Init(startupDelay, OnUIStart, Universe_OnLog, config);
		}

		private void Universe_OnLog(string message, LogType type)
		{
			LogLevel logLevel = default;
			switch (type)
			{
				case LogType.Error:
				case LogType.Assert:
				case LogType.Exception:
					logLevel = LogLevel.Error;
					break;
				case LogType.Warning:
					logLevel = LogLevel.Warning;
					break;
				case LogType.Log:
					logLevel = LogLevel.Info;
					break;
			}
			Logger.Log(logLevel, message);
		}
		#endregion
	}

	internal class PluginPanel : PanelBase
	{
		public PluginPanel(UIBase owner) : base(owner) { }

		public override string Name => PluginInfo.PLUGIN_NAME;
		public override int MinWidth => 400;
		public override int MinHeight => 500;
		public override Vector2 DefaultAnchorMin => new(0.25f, 0.25f);
		public override Vector2 DefaultAnchorMax => DefaultAnchorMin;
		public override bool CanDragAndResize => true;

		protected override void OnClosePanelClicked()
		{
			ButtJiggle.Instance.DisableUI();
		}

		protected override void ConstructPanelContent()
		{
			//Text myText = UIFactory.CreateLabel(ContentRoot, "myText", "Hello world");
			//UIFactory.SetLayoutElement(myText.gameObject, minWidth: 200, minHeight: 25);

			CreateControl(ContentRoot, "Debug Mode", get: () => JiggleBoneHelper.DebugMode, set: (value) => JiggleBoneHelper.DebugMode = value);

			var overrideColumn = UIFactory.CreateVerticalGroup(ContentRoot, "overrides", false, false, true, true, childAlignment: TextAnchor.UpperLeft);
			UIFactory.SetLayoutElement(overrideColumn, minWidth: 300, minHeight: 300);
			{
				CreateControl(overrideColumn, "Use Global Override",
					get: () => JiggleBoneHelper.UseGlobalOverride,
					set: (value) => JiggleBoneHelper.UseGlobalOverride = value);

				CreateControl(overrideColumn, "Blend Value"       , get: () => JiggleBoneHelper.GlobalOverride.BlendValue      , set: (value) => JiggleBoneHelper.GlobalOverride.BlendValue       = value);
				CreateControl(overrideColumn, "Blend Value 2"     , get: () => JiggleBoneHelper.GlobalOverride.BlendValue2     , set: (value) => JiggleBoneHelper.GlobalOverride.BlendValue2      = value);
				CreateControl(overrideColumn, "Gravity"           , get: () => JiggleBoneHelper.GlobalOverride.Gravity         , set: (value) => JiggleBoneHelper.GlobalOverride.Gravity          = value);
				CreateControl(overrideColumn, "Clothed Stiffness" , get: () => JiggleBoneHelper.GlobalOverride.ClothedStiffness, set: (value) => JiggleBoneHelper.GlobalOverride.ClothedStiffness = value);
				CreateControl(overrideColumn, "Naked Stiffness"   , get: () => JiggleBoneHelper.GlobalOverride.NakedStiffness  , set: (value) => JiggleBoneHelper.GlobalOverride.NakedStiffness   = value);
				CreateControl(overrideColumn, "Softness"          , get: () => JiggleBoneHelper.GlobalOverride.Softness        , set: (value) => JiggleBoneHelper.GlobalOverride.Softness         = value);
				CreateControl(overrideColumn, "Up & Down"         , get: () => JiggleBoneHelper.GlobalOverride.UpDown          , set: (value) => JiggleBoneHelper.GlobalOverride.UpDown           = value);
				CreateControl(overrideColumn, "Yori"              , get: () => JiggleBoneHelper.GlobalOverride.Yori            , set: (value) => JiggleBoneHelper.GlobalOverride.Yori             = value);
				CreateControl(overrideColumn, "Squash & Stretch"  , get: () => JiggleBoneHelper.GlobalOverride.SquashAndStretch, set: (value) => JiggleBoneHelper.GlobalOverride.SquashAndStretch = value);
				CreateControl(overrideColumn, "Front Stretch"     , get: () => JiggleBoneHelper.GlobalOverride.FrontStretch    , set: (value) => JiggleBoneHelper.GlobalOverride.FrontStretch     = value);
				CreateControl(overrideColumn, "Side Stretch"      , get: () => JiggleBoneHelper.GlobalOverride.SideStretch     , set: (value) => JiggleBoneHelper.GlobalOverride.SideStretch      = value);
				CreateControl(overrideColumn, "Enable Scale X"    , get: () => JiggleBoneHelper.GlobalOverride.EnableScaleX    , set: (value) => JiggleBoneHelper.GlobalOverride.EnableScaleX     = value);
				CreateControl(overrideColumn, "Limit Rotation"    , get: () => JiggleBoneHelper.GlobalOverride.LimitRotation   , set: (value) => JiggleBoneHelper.GlobalOverride.LimitRotation    = value);
				CreateControl(overrideColumn, "Limit Rot Decay"   , get: () => JiggleBoneHelper.GlobalOverride.LimitRotDecay   , set: (value) => JiggleBoneHelper.GlobalOverride.LimitRotDecay    = value);
			}
		}

		private GameObject CreateControl<T>(GameObject parent, string labelText, System.Func<T> get, UnityAction<T> set = null)
		{
			GameObject control = null;
			if (typeof(T) == typeof(bool)) {
				if (get is not System.Func<bool> getCasted) throw new System.ArgumentException();
				if (set is not UnityAction<bool> setCasted) setCasted = null;
				control = CreateBoolControl(parent, labelText, getCasted, setCasted);
			}
			else if (ParseUtility.CanParse(typeof(T)))
			{
				control = CreateParsedControl(parent, labelText, get, set);
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

		private GameObject CreateBoolControl(GameObject parent, string labelText, System.Func<bool> get, UnityAction<bool> set = null)
		{
			var name = "controlBool_" + labelText.Replace(" ", "");
			var toggleRoot = UIFactory.CreateToggle(parent, name, out var toggle, out var label);
			label.text = labelText;
			toggle.isOn = get();
			toggle.interactable = (set != null);
			if (set != null)
			{
				toggle.onValueChanged.AddListener((value) =>
				{
					set(value);
					ButtJiggle.Logger.LogInfo($"{name} = {get()}");
				});
			}
			UIFactory.SetLayoutElement(toggleRoot, minWidth: 200, minHeight: 25);
			return toggleRoot;
		}

		private GameObject CreateParsedControl<T>(GameObject parent, string labelText, System.Func<T> get, UnityAction<T> set = null)
		{
			var name = labelText.Replace(" ", "");
			var row = UIFactory.CreateHorizontalGroup(parent, $"control{typeof(T).Name}_{name}", false, false, true, true, childAlignment: TextAnchor.MiddleLeft);
			UIFactory.SetLayoutElement(row, minWidth: 200, minHeight: 25);
			{
				var inputFieldRef = UIFactory.CreateInputField(row, "input", labelText);
				UIFactory.SetLayoutElement(inputFieldRef.GameObject, minWidth: 200, minHeight: 25);
				inputFieldRef.Text = ParseUtility.ToStringForInput(get(), typeof(T));
				inputFieldRef.Component.interactable = (set != null);
				if (set != null)
				{
					inputFieldRef.OnValueChanged += (str) =>
					{
						if (ParseUtility.TryParse(str, typeof(T), out var newValue, out _)) {
							set((T)newValue);
							ButtJiggle.Logger.LogInfo($"{name} = {get()}");
						}
						inputFieldRef.Text = ParseUtility.ToStringForInput(get(), typeof(T));
					};
				}

				UIFactory.CreateLabel(row, "label", " "+labelText);
				UIFactory.SetLayoutElement(row, minWidth: 200, minHeight: 25);
			}
			return row;
		}

		private GameObject CreateControl<T>(GameObject parent, string labelText, System.Func<Override<T>> get, UnityAction<Override<T>> set = null)
		{
			var name = labelText.Replace(" ", "");

			var row = UIFactory.CreateHorizontalGroup(parent, $"controlOverride_{name}", false, false, true, true, childAlignment: TextAnchor.MiddleLeft);
			UIFactory.SetLayoutElement(row, minWidth: 200, minHeight: 25);
			{
				var tempOverride = get();

				var enabledRoot = UIFactory.CreateToggle(row, $"controlBool_{name}_Enabled", out var enableToggle, out var enabledLabel);
				UIFactory.SetLayoutElement(enabledRoot, minWidth: 40, minHeight: 25, preferredWidth: 40, preferredHeight: 25);
				enabledLabel.text = " | ";
				enableToggle.isOn = tempOverride.Enabled;
				enableToggle.interactable = (set != null);
				if (set != null)
				{
					enableToggle.onValueChanged.AddListener((value) =>
					{
						tempOverride = get();
						tempOverride.Enabled = value;
						set(tempOverride);
						ButtJiggle.Logger.LogInfo($"{name}.Enabled == {get().Enabled}");
					});
				}

				System.Func<T> valueGet = () =>
				{
					tempOverride = get();
					return tempOverride.Value;
				};
				UnityAction<T> valueSet = (set == null) ? null : (value) =>
				{
					tempOverride = get();
					tempOverride.Value = value;
					set(tempOverride);
					ButtJiggle.Logger.LogInfo($"{name}.Value == {get().Value}");
				};

				GameObject valueControl = null;
				if (typeof(T) == typeof(Stiffness))
				{
					if (valueGet is not System.Func<Stiffness> valueGetCasted) throw new System.ArgumentException();
					if (valueSet is not UnityAction<Stiffness> valueSetCasted) valueSetCasted = null;

					valueControl = CreateControl(row, labelText,
						get: () => (Vector2)valueGetCasted(),
						set: (value) => valueSetCasted((Stiffness)value));
				}
				else
				{
					valueControl = CreateControl(row, labelText, valueGet, valueSet);
				}
			}
			return row;
		}

		// override other methods as desired
	}
}
