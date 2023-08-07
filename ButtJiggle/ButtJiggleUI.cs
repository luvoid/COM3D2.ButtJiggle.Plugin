using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;
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
				Allow_UI_Selection_Outside_UIBase = true,
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
			try
			{
				SafeConstructPanelContent();
			}
			catch (System.Exception ex)
			{
				ButtJiggle.Logger.LogError(ex);
			}
		}

		protected void SafeConstructPanelContent()
		{
			//Text myText = UIFactory.CreateLabel(ContentRoot, "myText", "Hello world");
			//UIFactory.SetLayoutElement(myText.gameObject, minWidth: 200, minHeight: 25);

			UIControlFactory.CreateControl(ContentRoot, "Debug Mode", get: () => JiggleBoneHelper.DebugMode, set: (value) => JiggleBoneHelper.DebugMode = value);

			var overrideColumn = UIFactory.CreateVerticalGroup(ContentRoot, "overrides", false, false, true, true, childAlignment: TextAnchor.UpperLeft);
			UIFactory.SetLayoutElement(overrideColumn, minWidth: 300, minHeight: 300);
			{
				UIControlFactory.CreateControl(overrideColumn, "Use Global Override",
					get: () => JiggleBoneHelper.UseGlobalOverride,
					set: (value) => { JiggleBoneHelper.UseGlobalOverride = value; OnGlobalOverrideUIUpdated(); },
					listen: ButtJiggle.Instance.OnGlobalOverrideUpdated);

				List<GameObject> slotColumns = null;

				var slotDropdownRoot = UIFactory.CreateDropdown(overrideColumn, "slotSelectDropdown", out Dropdown slotSelectDropdown, "Select a slot...", 14, 
					(slotIndex) =>
					{
						for (int i = 0; i < slotColumns.Count; i++)
						{
							slotColumns[i].SetActive(i == slotIndex);
						}
					}
				);
				UIFactory.SetLayoutElement(slotDropdownRoot, minWidth: 300, minHeight: 25, preferredWidth: 300, preferredHeight: 25);

				slotColumns = new List<GameObject>() {
					CreateJiggleBoneOverrideColumn(overrideColumn, "Hips"  , () => ref JiggleBoneHelper.GlobalOverride.HipOverride   , OnGlobalOverrideUIUpdated, ButtJiggle.Instance.OnGlobalOverrideUpdated),
					CreateJiggleBoneOverrideColumn(overrideColumn, "Pelvis", () => ref JiggleBoneHelper.GlobalOverride.PelvisOverride, OnGlobalOverrideUIUpdated, ButtJiggle.Instance.OnGlobalOverrideUpdated),
				};

				int i = 0;
				slotSelectDropdown.AddOptions(
					slotColumns.Select((go) => 
					{ 
						ButtJiggle.Logger.LogDebug($"Add slotSelect {go.name}");
						go.SetActive(i++ == 0);
						return go.name;
					}).ToList()
				);
				slotSelectDropdown.RefreshShownValue();
				slotSelectDropdown.Show();
			}
		}

		delegate ref JiggleBoneOverride OverrideRefGetter();

		private static GameObject CreateJiggleBoneOverrideColumn(
			GameObject parent,
			string name,
			OverrideRefGetter refGet,
			UnityAction onSet,
			UnityEvent listen = null)
		{
			var column = UIFactory.CreateVerticalGroup(parent, name, false, false, true, true, childAlignment: TextAnchor.UpperLeft);
			UIFactory.SetLayoutElement(column, minWidth: 350, minHeight: 300);
			{
				UIControlFactory.CreateControl(column, "Blend Value (auto)", refGet: () => ref refGet().BlendValue      ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Blend Value 2"     , refGet: () => ref refGet().BlendValue2     ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Gravity"           , refGet: () => ref refGet().Gravity         ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Clothed Stiffness" , refGet: () => ref refGet().ClothedStiffness,  onSet, listen);
				UIControlFactory.CreateControl(column, "Naked Stiffness"   , refGet: () => ref refGet().NakedStiffness  ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Softness"          , refGet: () => ref refGet().Softness        ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Up & Down"         , refGet: () => ref refGet().UpDown          ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Yori"              , refGet: () => ref refGet().Yori            ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Squash & Stretch"  , refGet: () => ref refGet().SquashAndStretch,  onSet, listen);
				UIControlFactory.CreateControl(column, "Front Stretch"     , refGet: () => ref refGet().FrontStretch    ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Side Stretch"      , refGet: () => ref refGet().SideStretch     ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Enable Scale X"    , refGet: () => ref refGet().EnableScaleX    ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Limit Rotation"    , refGet: () => ref refGet().LimitRotation   ,  onSet, listen);
				UIControlFactory.CreateControl(column, "Limit Rot Decay"   , refGet: () => ref refGet().LimitRotDecay   ,  onSet, listen);
			}
			return column;
		}

		private void OnGlobalOverrideUIUpdated()
		{
			ButtJiggle.ConfigSaveGlobalOverride();
		}

		// override other methods as desired
	}
}
